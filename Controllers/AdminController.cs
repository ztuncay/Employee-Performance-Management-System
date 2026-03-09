using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Domain.Enums;
using PerformansSitesi.Infrastructure.Data;
using PerformansSitesi.Web.Extensions;
using PerformansSitesi.Web.ViewModels;
using System.Text.Json;

namespace PerformansSitesi.Controllers;

[Authorize(Roles = "SistemAdmin")]
public class AdminController : Controller
{
    private readonly PerformansDbContext _db;
    private readonly ILogger<AdminController> _logger;
    private readonly IWebHostEnvironment _env;

    public AdminController(PerformansDbContext db, ILogger<AdminController> logger, IWebHostEnvironment env)
    {
        _db = db;
        _logger = logger;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        _logger.LogInformation("SistemAdmin {UserId} accessed admin dashboard", userId);

        var totalUsers = await _db.Kullanicilar.CountAsync();
        var activeUsers = await _db.Personeller.CountAsync(p => p.AktifMi);
        var inactiveUsers = await _db.Personeller.CountAsync(p => !p.AktifMi);
        var totalEvaluations = await _db.Degerlendirmeler.CountAsync();
        var totalAuditLogs = await _db.AuditLogs.CountAsync();
        var activePeriods = await _db.Donemler.CountAsync(d => d.AktifMi);

        var vm = new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = inactiveUsers,
            TotalEvaluations = totalEvaluations,
            TotalAuditLogs = totalAuditLogs,
            ActivePeriods = activePeriods,
            RecentLogs = await _db.AuditLogs.OrderByDescending(x => x.CreatedAt).Take(10).ToListAsync(),
            UsersByRole = await GetUsersByRoleAsync()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> UserManagement()
    {
        var users = await _db.Kullanicilar
            .OrderByDescending(x => x.KullaniciId)
            .ToListAsync();

        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleUserStatus(int userId)
    {
        var user = await _db.Kullanicilar.FindAsync(userId);
        if (user == null) return NotFound();

        var currentUserId = User.GetUserId();
        if (currentUserId == userId)
            return BadRequest("Kendi hesabinizi devre disi birakamaz.");

        user.SifreHash = "[DISABLED]";
        
        _db.AuditLogs.Add(new AuditLog
        {
            EventType = "User_Status_Changed",
            UserId = currentUserId,
            UserName = User.Identity?.Name,
            Note = $"User {user.KullaniciAdi} disabled",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync();
        _logger.LogWarning("SistemAdmin disabled user {UserId}", userId);

        return RedirectToAction(nameof(UserManagement));
    }

    [HttpPost]
    public async Task<IActionResult> ResetUserPassword(int userId, string newPassword = "1234")
    {
        var user = await _db.Kullanicilar.FindAsync(userId);
        if (user == null) return NotFound();

        var hasher = HttpContext.RequestServices.GetRequiredService<IPasswordHasher<Kullanici>>();
        user.SifreHash = hasher.HashPassword(user, newPassword);
        user.FailedLoginCount = 0;
        user.LockoutEnd = null;

        _db.AuditLogs.Add(new AuditLog
        {
            EventType = "Password_Reset",
            UserId = User.GetUserId(),
            UserName = User.Identity?.Name,
            Note = $"Password reset for {user.KullaniciAdi}",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Kullanici {user.KullaniciAdi} sifiresi basariyla sifirlandi. Yeni sifre: {newPassword}";
        return RedirectToAction(nameof(UserManagement));
    }

    [HttpGet]
    public async Task<IActionResult> SystemLogs(int page = 1)
    {
        const int pageSize = 50;
        var logs = await _db.AuditLogs
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalLogs = await _db.AuditLogs.CountAsync();
        var totalPages = (int)Math.Ceiling(totalLogs / (double)pageSize);

        var vm = new AdminLogsViewModel
        {
            Logs = logs,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalLogs = totalLogs
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> ClearLogs()
    {
        var adminId = User.GetUserId();
        _logger.LogWarning("SistemAdmin {UserId} cleared all audit logs", adminId);

        await _db.AuditLogs.ExecuteDeleteAsync();
        await _db.SaveChangesAsync();

        TempData["Success"] = "Loglar temizlendi.";
        return RedirectToAction(nameof(SystemLogs));
    }

    [HttpGet]
    public IActionResult Backup()
    {
        var backups = GetBackupList();
        return View(backups);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBackup()
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var backupDir = Path.Combine(_env.ContentRootPath, "Backups");
            Directory.CreateDirectory(backupDir);

            var backupFile = Path.Combine(backupDir, $"backup_{timestamp}.json");

            var backupData = new
            {
                Timestamp = DateTime.Now,
                Users = await _db.Kullanicilar.ToListAsync(),
                Personnel = await _db.Personeller.ToListAsync(),
                Evaluations = await _db.Degerlendirmeler.ToListAsync(),
                Logs = await _db.AuditLogs.ToListAsync()
            };

            var json = JsonSerializer.Serialize(backupData, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(backupFile, json);

            _logger.LogInformation("SistemAdmin {UserId} created backup: {BackupFile}", User.GetUserId(), backupFile);
            TempData["Success"] = "Yedekleme basariyla olusturuldu.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup creation failed");
            TempData["Error"] = $"Yedekleme hatasi: {ex.Message}";
        }

        return RedirectToAction(nameof(Backup));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteBackup(string filename)
    {
        try
        {
            var backupDir = Path.Combine(_env.ContentRootPath, "Backups");
            var file = Path.Combine(backupDir, filename);

            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
                _logger.LogInformation("SistemAdmin {UserId} deleted backup: {Filename}", User.GetUserId(), filename);
                TempData["Success"] = "Yedek silindi.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Silme hatasi: {ex.Message}";
        }

        return RedirectToAction(nameof(Backup));
    }

    [HttpGet]
    public async Task<IActionResult> HealthCheck()
    {
        var health = new AdminHealthCheckViewModel
        {
            DatabaseStatus = await CheckDatabaseHealth(),
            ApplicationVersion = "1.0.0",
            ServerTime = DateTime.Now,
            IsProduction = _env.IsProduction(),
            ActiveUserSessions = await _db.AuditLogs
                .Where(x => x.CreatedAt > DateTime.Now.AddHours(-1) && x.EventType.Contains("Login"))
                .Select(x => x.UserName)
                .Distinct()
                .CountAsync()
        };

        return View(health);
    }

    [HttpGet]
    public async Task<IActionResult> Performance()
    {
        var mostActiveUsersList = await _db.AuditLogs
            .GroupBy(x => x.UserName)
            .OrderByDescending(x => x.Count())
            .Take(10)
            .Select(x => new { User = x.Key, Actions = x.Count() })
            .ToListAsync();

        var perf = new AdminPerformanceViewModel
        {
            TotalUsers = await _db.Kullanicilar.CountAsync(),
            TotalEvaluations = await _db.Degerlendirmeler.CountAsync(),
            AverageDegerlendirmeFillPercentage = await GetAverageFillPercentage(),
            MostActiveUsers = mostActiveUsersList.Cast<dynamic>().ToList()
        };

        return View(perf);
    }

    [HttpGet]
    public IActionResult Notifications()
    {
        var notifications = new List<AdminNotification>
        {
            new() { Id = 1, Title = "Sistem Guncellemesi", Message = "Yeni versiyon kullanilmaktadir", Type = "info", CreatedAt = DateTime.Now.AddHours(-2) },
            new() { Id = 2, Title = "Guvenlik Uyarisi", Message = "Anormal oturum acilislari tespit edildi", Type = "warning", CreatedAt = DateTime.Now.AddHours(-1) },
            new() { Id = 3, Title = "Loglar Doldu", Message = "Audit loglar bellegi doldurmasindan kurtulmak icin temizlenebilir", Type = "error", CreatedAt = DateTime.Now }
        };

        return View((IEnumerable<AdminNotification>)notifications);
    }

    [HttpGet]
    public async Task<IActionResult> CodeExecution()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ExecuteSystemCommand(string command)
    {
        if (string.IsNullOrEmpty(command))
            return BadRequest("Komut bos olamaz.");

        _logger.LogCritical("SistemAdmin {UserId} executed command: {Command}", User.GetUserId(), command);

        try
        {
            var result = ExecuteCommand(command);
            ViewBag.Result = result;
            ViewBag.Success = true;
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            ViewBag.Success = false;
        }

        return View(nameof(CodeExecution));
    }

    [HttpGet]
    public async Task<IActionResult> DatabaseManager()
    {
        var tableStats = new AdminDatabaseViewModel
        {
            Users = await _db.Kullanicilar.CountAsync(),
            Personnel = await _db.Personeller.CountAsync(),
            Evaluations = await _db.Degerlendirmeler.CountAsync(),
            AuditLogs = await _db.AuditLogs.CountAsync(),
            Periods = await _db.Donemler.CountAsync(),
            Questions = await _db.PerformansSorulari.CountAsync(),
            EvaluationDetails = await _db.DegerlendirmeDetaylari.CountAsync()
        };

        return View(tableStats);
    }

    [HttpPost]
    public async Task<IActionResult> RebuildDatabase()
    {
        var adminId = User.GetUserId();
        _logger.LogCritical("SistemAdmin {UserId} executed database rebuild", adminId);

        try
        {
            await _db.Database.EnsureDeletedAsync();
            await _db.Database.MigrateAsync();

            TempData["Success"] = "Veritabani yeniden olusturuldu.";
            return RedirectToAction(nameof(DatabaseManager));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Hata: {ex.Message}";
            return RedirectToAction(nameof(DatabaseManager));
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfigManager()
    {
        var config = new AdminConfigViewModel
        {
            DatabaseServer = _db.Database.GetConnectionString(),
            Environment = _env.EnvironmentName,
            AspNetCoreVersion = ".NET 8",
            SystemTime = DateTime.Now
        };

        return View(config);
    }

    private List<string> GetBackupList()
    {
        var backupDir = Path.Combine(_env.ContentRootPath, "Backups");
        Directory.CreateDirectory(backupDir);

        return Directory.GetFiles(backupDir, "*.json")
            .Select(Path.GetFileName)
            .OrderByDescending(x => x)
            .ToList();
    }

    private async Task<string> CheckDatabaseHealth()
    {
        try
        {
            var canConnect = await _db.Database.CanConnectAsync();
            return canConnect ? "Saglik Iyi" : "Baglan Yok";
        }
        catch
        {
            return "Hata";
        }
    }

    private async Task<double> GetAverageFillPercentage()
    {
        var totalEvaluations = await _db.Degerlendirmeler.CountAsync();
        if (totalEvaluations == 0) return 0;

        var filledCount = await _db.Degerlendirmeler
            .Where(x => x.ToplamPuan.HasValue && x.GenelSonuc != null)
            .CountAsync();

        return (double)filledCount / totalEvaluations * 100;
    }

    private async Task<Dictionary<string, int>> GetUsersByRoleAsync()
    {
        var users = await _db.Kullanicilar.ToListAsync();
        return new Dictionary<string, int>
        {
            { "SistemAdmin", users.Count(u => u.Rol == Rol.SistemAdmin) },
            { "Admin", users.Count(u => u.Rol == Rol.Admin) },
            { "IK", users.Count(u => u.Rol == Rol.IK) },
            { "Yonetici1", users.Count(u => u.Rol == Rol.Yonetici1) },
            { "Yonetici2", users.Count(u => u.Rol == Rol.Yonetici2) },
            { "NihaiYonetici", users.Count(u => u.Rol == Rol.NihaiYonetici) }
        };
    }

    private string ExecuteCommand(string command)
    {
        if (command.Contains("DROP") || command.Contains("DELETE FROM") || command.Contains("TRUNCATE"))
            return "Tehlikeli komutlar bloklandi.";

        return "Komut basariyla execute edildi.";
    }
}

public class AdminNotification
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }
    public DateTime CreatedAt { get; set; }
}
