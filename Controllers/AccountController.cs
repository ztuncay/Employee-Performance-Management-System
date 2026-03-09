using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Domain.Enums;
using PerformansSitesi.Infrastructure.Data;
using PerformansSitesi.Web.ViewModels;

namespace PerformansSitesi.Controllers;

public class AccountController : Controller
{
    private readonly PerformansDbContext _db;
    private readonly IPasswordHasher<Kullanici> _hasher;
    private readonly ILogger<AccountController> _logger;

    // Kilit politikası (genel kullanım): 10 deneme, 5 dakika kilit
    private const int MaxFailedAttempts = 10;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

    public AccountController(PerformansDbContext db, IPasswordHasher<Kullanici> hasher, ILogger<AccountController> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(RedirectByRole));

        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            var userInfo = await _db.Kullanicilar
                .AsNoTracking()
                .Where(x => x.KullaniciAdi == vm.KullaniciAdi)
                .Select(x => new
                {
                    x.KullaniciId,
                    AdSoyad = x.AdSoyad ?? x.KullaniciAdi,
                    x.KullaniciAdi,
                    SifreHash = x.SifreHash ?? string.Empty,
                    RolValue = EF.Property<int?>(x, "Rol"),
                    x.FailedLoginCount,
                    x.LockoutEnd
                })
                .FirstOrDefaultAsync();

            if (userInfo == null)
            {
                vm.Error = "Kullanici adi veya sifre hatali.";
                return View(vm);
            }

            if (!userInfo.RolValue.HasValue || !Enum.IsDefined(typeof(Rol), userInfo.RolValue.Value))
            {
                _logger.LogWarning("Login blocked due to invalid role for user {User}", userInfo.KullaniciAdi);
                vm.Error = "Hesap rolü tanımlı değil. Lutfen yoneticinizle iletisime gecin.";
                return View(vm);
            }

            var resolvedRole = (Rol)userInfo.RolValue!.Value;

            // Kilit süresi geçmişse otomatik aç ve sayacı sıfırla
            if (userInfo.LockoutEnd.HasValue && userInfo.LockoutEnd.Value <= DateTime.Now)
            {
                await _db.Kullanicilar
                    .Where(k => k.KullaniciId == userInfo.KullaniciId)
                    .ExecuteUpdateAsync(up => up
                        .SetProperty(k => k.FailedLoginCount, 0)
                        .SetProperty(k => k.LockoutEnd, (DateTime?)null));

                userInfo = userInfo with { LockoutEnd = null, FailedLoginCount = 0 };
            }

            if (string.IsNullOrWhiteSpace(userInfo.SifreHash) || userInfo.SifreHash == "[DISABLED]")
            {
                vm.Error = "Hesap devre disi veya eksik. Lutfen yoneticinizle iletisime gecin.";
                return View(vm);
            }

            if (userInfo.LockoutEnd.HasValue && userInfo.LockoutEnd.Value > DateTime.Now)
            {
                var remainingMinutes = (int)(userInfo.LockoutEnd.Value - DateTime.Now).TotalMinutes;
                vm.Error = $"Hesap kilitli. Lutfen {remainingMinutes} dakika sonra tekrar deneyin.";

            _db.AuditLogs.Add(new AuditLog
            {
                EventType = "Login_Lockout",
                UserId = userInfo.KullaniciId,
                UserName = userInfo.KullaniciAdi,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Note = $"Lockout attempt. Remaining: {remainingMinutes}m",
                CreatedAt = DateTime.UtcNow
            });
                await _db.SaveChangesAsync();

                return View(vm);
            }

            var userForHash = new Kullanici
            {
                KullaniciId = userInfo.KullaniciId,
                KullaniciAdi = userInfo.KullaniciAdi,
                AdSoyad = userInfo.AdSoyad,
                SifreHash = userInfo.SifreHash,
                Rol = resolvedRole
            };

            var verify = _hasher.VerifyHashedPassword(userForHash, userInfo.SifreHash, vm.Sifre);
            if (verify == PasswordVerificationResult.Failed)
            {
                var failedCount = userInfo.FailedLoginCount + 1;
                var lockoutEnd = failedCount >= MaxFailedAttempts ? DateTime.Now.Add(LockoutDuration) : userInfo.LockoutEnd;

                await _db.Kullanicilar
                    .Where(k => k.KullaniciId == userInfo.KullaniciId)
                    .ExecuteUpdateAsync(up => up
                        .SetProperty(k => k.FailedLoginCount, failedCount)
                        .SetProperty(k => k.LockoutEnd, lockoutEnd));

                if (failedCount >= MaxFailedAttempts)
                {
                    _db.AuditLogs.Add(new AuditLog
                    {
                        EventType = "Account_Locked",
                        UserId = userInfo.KullaniciId,
                        UserName = userInfo.KullaniciAdi,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Note = $"Account locked after {failedCount} failed attempts",
                    CreatedAt = DateTime.UtcNow
                    });

                    _logger.LogWarning("Account locked: {UserName} after {Count} failed attempts", userInfo.KullaniciAdi, failedCount);
                }

                await _db.SaveChangesAsync();

                vm.Error = failedCount >= MaxFailedAttempts
                    ? $"Hesap kilitlendi. Lutfen {LockoutDuration.TotalMinutes} dakika sonra tekrar deneyin."
                    : $"Kullanici adi veya sifre hatali. Kalan deneme: {Math.Max(0, MaxFailedAttempts - failedCount)}";

                return View(vm);
            }

            await _db.Kullanicilar
                .Where(k => k.KullaniciId == userInfo.KullaniciId)
                .ExecuteUpdateAsync(up => up
                    .SetProperty(k => k.FailedLoginCount, 0)
                    .SetProperty(k => k.LockoutEnd, (DateTime?)null));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.KullaniciId.ToString()),
                new Claim(ClaimTypes.Name, userInfo.AdSoyad ?? userInfo.KullaniciAdi),
                new Claim(ClaimTypes.Role, resolvedRole.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = vm.HatirlaBeni });

            _db.AuditLogs.Add(new AuditLog
            {
                EventType = "Login_Success",
                UserId = userInfo.KullaniciId,
                UserName = userInfo.KullaniciAdi,
                UserRole = resolvedRole.ToString(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(RedirectByRole));
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error during login for user {User}", vm.KullaniciAdi);
            vm.Error = "Veritabani hatasi olustu. Lutfen tekrar deneyin.";
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected login error for user {User}", vm.KullaniciAdi);
            vm.Error = "Giriste beklenmeyen bir hata olustu. Lutfen tekrar deneyin.";
            return View(vm);
        }
    }

    [Authorize]
    public async Task<IActionResult> RedirectByRole()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return RedirectToAction(nameof(Login));

        var user = await _db.Kullanicilar.AsNoTracking()
            .FirstOrDefaultAsync(k => k.KullaniciId == userId);

        if (user == null)
            return RedirectToAction(nameof(Login));

        // Kullanıcı bazlı URL
        return LocalRedirect($"/dashboard/{user.KullaniciAdi}");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> LogoutGet()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult Denied()
    {
        return View();
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            vm.Error = "Kullanici bilgisi bulunamadi.";
            return View(vm);
        }

        var user = await _db.Kullanicilar.FirstOrDefaultAsync(k => k.KullaniciId == userId);
        if (user == null)
        {
            vm.Error = "Kullanici bulunamadi.";
            return View(vm);
        }

        var verify = _hasher.VerifyHashedPassword(user, user.SifreHash, vm.MevcutSifre);
        if (verify == PasswordVerificationResult.Failed)
        {
            vm.Error = "Mevcut sifre yanlis.";
            return View(vm);
        }

        if (vm.YeniSifre != vm.YeniSifreTekrar)
        {
            vm.Error = "Yeni sifre ve onay eslesmiyor.";
            return View(vm);
        }

        user.SifreHash = _hasher.HashPassword(user, vm.YeniSifre);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Sifreniz basariyla degistirildi.";
        return RedirectToAction(nameof(ChangePassword));
    }
}
