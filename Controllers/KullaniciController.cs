using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Application.Services;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Domain.Enums;
using PerformansSitesi.Infrastructure.Data;
using PerformansSitesi.Web.Extensions;
using PerformansSitesi.Web.ViewModels;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PerformansSitesi.Controllers;

[Authorize(Roles = "Admin,SistemAdmin")]
public class KullaniciController : Controller
{
    private readonly PerformansDbContext _db;
    private readonly IPasswordHasher<Kullanici> _hasher;
    private readonly ILogger<KullaniciController> _logger;

    public KullaniciController(
        PerformansDbContext db,
        IPasswordHasher<Kullanici> hasher,
        ILogger<KullaniciController> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? arama, bool includeDisabled = false)
    {
        var q = _db.Kullanicilar.AsQueryable();

        if (!includeDisabled)
        {
            q = q.Where(x => x.SifreHash != "[DISABLED]");
        }

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();
            q = q.Where(x =>
                x.AdSoyad.Contains(arama) ||
                x.KullaniciAdi.Contains(arama) ||
                (x.Email != null && x.Email.Contains(arama)));
        }

        var list = await q
            .OrderBy(x => x.Rol)
            .ThenBy(x => x.AdSoyad)
            .ToListAsync();

        ViewBag.Arama = arama;
        ViewBag.IncludeDisabled = includeDisabled;
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new KullaniciCreateViewModel
        {
            Rol = Rol.Yonetici1,
            AllPersoneller = await GetAllPersonellerAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KullaniciCreateViewModel vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                vm.AllPersoneller = await GetAllPersonellerAsync();
                return View(vm);
            }

            vm.KullaniciAdi = vm.KullaniciAdi.Trim();

            var exists = await _db.Kullanicilar.AnyAsync(x => x.KullaniciAdi == vm.KullaniciAdi);
            if (exists)
            {
                vm.Hata = "Bu kullanıcı adı zaten kullanılıyor.";
                vm.AllPersoneller = await GetAllPersonellerAsync();
                return View(vm);
            }

            var user = new Kullanici
            {
                AdSoyad = vm.AdSoyad.Trim(),
                Email = string.IsNullOrWhiteSpace(vm.Email) ? "" : vm.Email.Trim(),
                KullaniciAdi = vm.KullaniciAdi,
                Rol = vm.Rol,
                PersonelId = vm.PersonelId > 0 ? vm.PersonelId : null
            };

            user.SifreHash = _hasher.HashPassword(user, vm.Sifre);

            _db.Kullanicilar.Add(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Kullanıcı başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı oluşturma hatası: {Username}, Rol: {Role}, PersonelId: {PersonelId}",
                vm.KullaniciAdi, vm.Rol, vm.PersonelId);
            vm.Hata = $"Hata oluştu: {ex.InnerException?.Message ?? ex.Message}";
            vm.AllPersoneller = await GetAllPersonellerAsync();
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var u = await _db.Kullanicilar.FirstOrDefaultAsync(x => x.KullaniciId == id);
        if (u == null) return NotFound();

        var vm = new KullaniciEditViewModel
        {
            KullaniciId = u.KullaniciId,
            AdSoyad = u.AdSoyad,
            Email = u.Email,
            KullaniciAdi = u.KullaniciAdi,
            Rol = u.Rol,
            PersonelId = u.PersonelId,
            AllPersoneller = await GetAllPersonellerAsync()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(KullaniciEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.AllPersoneller = await GetAllPersonellerAsync();
            return View(vm);
        }

        var u = await _db.Kullanicilar.FirstOrDefaultAsync(x => x.KullaniciId == vm.KullaniciId);
        if (u == null) return NotFound();

        vm.KullaniciAdi = vm.KullaniciAdi.Trim();

        var exists = await _db.Kullanicilar.AnyAsync(x => x.KullaniciAdi == vm.KullaniciAdi && x.KullaniciId != vm.KullaniciId);
        if (exists)
        {
            vm.Hata = "Bu kullanıcı adı zaten kullanılıyor.";
            vm.AllPersoneller = await GetAllPersonellerAsync();
            return View(vm);
        }

        u.AdSoyad = vm.AdSoyad.Trim();
        u.Email = string.IsNullOrWhiteSpace(vm.Email) ? "" : vm.Email.Trim();
        u.KullaniciAdi = vm.KullaniciAdi;
        u.Rol = vm.Rol;
        u.PersonelId = vm.PersonelId > 0 ? vm.PersonelId : null;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Kullanıcı bilgileri güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword(int id)
    {
        var u = await _db.Kullanicilar.FirstOrDefaultAsync(x => x.KullaniciId == id);
        if (u == null) return NotFound();

        var vm = new KullaniciPasswordViewModel
        {
            KullaniciId = u.KullaniciId,
            AdSoyad = u.AdSoyad
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(KullaniciPasswordViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var u = await _db.Kullanicilar.FirstOrDefaultAsync(x => x.KullaniciId == vm.KullaniciId);
        if (u == null) return NotFound();

        u.SifreHash = _hasher.HashPassword(u, vm.YeniSifre);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Şifre başarıyla değiştirildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var currentUserId = User.GetUserId();
            if (currentUserId == id)
            {
                TempData["Error"] = "Kendi hesabınızı silemezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            var u = await _db.Kullanicilar.FirstOrDefaultAsync(x => x.KullaniciId == id);
            if (u == null) return NotFound();

            if (u.Rol == Rol.SistemAdmin || u.Rol == Rol.Admin)
            {
                TempData["Error"] = "Yönetici hesapları silinemez. Hesabı devre dışı bırakmak için sistem yöneticisine başvurun.";
                return RedirectToAction(nameof(Index));
            }

            var bagliPersonelVarMi = await _db.Personeller.AnyAsync(p =>
                p.Yonetici1Id == id || p.Yonetici2Id == id || p.NihaiYoneticiId == id);

            if (bagliPersonelVarMi)
            {
                TempData["Error"] = "Bu kullanıcı bir veya daha fazla personelde yönetici olarak atanmış. Silme işlemi iptal edildi.";
                return RedirectToAction(nameof(Index));
            }

            var degerlendirmeImzaVarMi = await _db.Degerlendirmeler.AnyAsync(e =>
                e.Yonetici1Id == id || e.Yonetici2Id == id || e.NihaiYoneticiId == id);

            if (degerlendirmeImzaVarMi)
            {
                TempData["Error"] = "Bu kullanıcı değerlendirme kayıtlarında yer alıyor. Silme işlemi iptal edildi.";
                return RedirectToAction(nameof(Index));
            }

            _db.Kullanicilar.Remove(u);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Kullanıcı silindi.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı silme hatası. KullaniciId={Id}", id);
            TempData["Error"] = "Kullanıcı silinirken bir hata oluştu.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult Import()
    {
        var vm = new KullaniciImportViewModel();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportPreview(IFormFile? file)
    {
        var vm = new KullaniciImportViewModel();

        if (file == null || file.Length == 0)
        {
            vm.FatalError = "Lütfen dosya seçiniz.";
            return View("Import", vm);
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            vm.FatalError = "Dosya boyutu 5MB'dan büyük olamaz.";
            return View("Import", vm);
        }

        try
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            var parseResult = KullaniciImportService.Parse(fileBytes, previewMaxRow: 50);

            vm.FatalError = parseResult.FatalError;
            vm.PreviewRows = parseResult.Rows;
            vm.Warnings = parseResult.Warnings;
            vm.PreviewReady = true;
            vm.CanImport = parseResult.Rows.All(r => r.Errors.Count == 0) && string.IsNullOrEmpty(parseResult.FatalError);

            HttpContext.Session.Set("FileBytes", fileBytes);
            return View("Import", vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excel import preview hatası");
            vm.FatalError = $"Dosya işlenirken hata oluştu: {ex.Message}";
            return View("Import", vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportExecute()
    {
        var vm = new KullaniciImportViewModel();

        var fileBytes = HttpContext.Session.Get("FileBytes");

        if (fileBytes == null || fileBytes.Length == 0)
        {
            vm.FatalError = "Oturum sona ermiş. Lütfen dosyayı yeniden yükleyiniz.";
            return View("Import", vm);
        }

        try
        {
            var parseResult = KullaniciImportService.Parse(fileBytes, previewMaxRow: int.MaxValue);

            if (!string.IsNullOrEmpty(parseResult.FatalError))
            {
                vm.FatalError = parseResult.FatalError;
                return View("Import", vm);
            }

            if (parseResult.Rows.Any(r => r.Errors.Count > 0))
            {
                vm.FatalError = "Hatalı satırlar var. Lütfen Excel'i düzeltip yeniden yükleyin.";
                vm.PreviewRows = parseResult.Rows;
                vm.PreviewReady = true;
                return View("Import", vm);
            }

            var importResult = await KullaniciImportService.DoImportAsync(_db, _hasher, parseResult.Rows);
            HttpContext.Session.Remove("FileBytes");

            TempData["ImportResult"] = JsonSerializer.Serialize(new
            {
                Eklenen = importResult.Eklenen,
                Guncellenen = importResult.Guncellenen,
                YeniKullanicilar = importResult.YeniKullanicilar
            });
            return RedirectToAction(nameof(ImportCheck));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excel import execute hatası");
            vm.FatalError = $"İthalat sırasında hata oluştu: {ex.Message}";
            return View("Import", vm);
        }
    }

    [HttpGet]
    public IActionResult ImportCheck()
    {
        var resultJson = TempData["ImportResult"] as string;

        if (string.IsNullOrEmpty(resultJson))
        {
            return RedirectToAction(nameof(Import));
        }

        var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

        var vm = new KullaniciImportCheckViewModel
        {
            EklenenSayi = result.GetProperty("Eklenen").GetInt32(),
            Guncellenenlerin = result.GetProperty("Guncellenen").GetInt32(),
            YeniKullanicilar = JsonSerializer.Deserialize<List<KullaniciImportService.KullaniciInfo>>(
                result.GetProperty("YeniKullanicilar").GetRawText()) ?? new()
        };

        return View(vm);
    }

    private async Task<List<Personel>> GetAllPersonellerAsync()
    {
        return await _db.Personeller
            .Where(p => p.AktifMi)
            .OrderBy(p => p.AdSoyad)
            .ToListAsync();
    }
}