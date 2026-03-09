using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Application.Services;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Infrastructure.Data;
using PerformansSitesi.Web.ViewModels;

namespace PerformansSitesi.Controllers;

[Authorize(Roles = "Admin,IK,SistemAdmin")]
public class PersonelController : Controller
{
    private readonly PerformansDbContext _db;

    public PersonelController(PerformansDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var personeller = await _db.Personeller
            .Include(p => p.Yonetici1)
            .Include(p => p.Yonetici2)
            .Include(p => p.NihaiYonetici)
            .OrderBy(p => p.AdSoyad)
            .ToListAsync();

        return View(personeller);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var vm = new PersonelFormViewModel
        {
            AllYoneticiler = GetAllYoneticiler()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PersonelFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.AllYoneticiler = GetAllYoneticiler();
            return View(vm);
        }

        var personel = new Personel
        {
            SicilNo = vm.SicilNo,
            AdSoyad = vm.AdSoyad,
            Gorev = vm.Gorev,
            ProjeAdi = vm.ProjeAdi,
            Mudurluk = vm.Mudurluk,
            Lokasyon = vm.Lokasyon,
            IseGirisTarihi = vm.IseGirisTarihi,
            IstenCikisTarihi = vm.IstenCikisTarihi,
            Yonetici1Id = vm.Yonetici1Id,
            Yonetici2Id = vm.Yonetici2Id,
            NihaiYoneticiId = vm.NihaiYoneticiId,
            AktifMi = true
        };

        _db.Personeller.Add(personel);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Personel başarıyla eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var personel = await _db.Personeller.FindAsync(id);
        if (personel == null)
            return NotFound();

        var vm = new PersonelFormViewModel
        {
            PersonelId = personel.PersonelId,
            SicilNo = personel.SicilNo,
            AdSoyad = personel.AdSoyad,
            Gorev = personel.Gorev,
            ProjeAdi = personel.ProjeAdi,
            Mudurluk = personel.Mudurluk,
            Lokasyon = personel.Lokasyon,
            IseGirisTarihi = personel.IseGirisTarihi,
            IstenCikisTarihi = personel.IstenCikisTarihi,
            Yonetici1Id = personel.Yonetici1Id,
            Yonetici2Id = personel.Yonetici2Id,
            NihaiYoneticiId = personel.NihaiYoneticiId,
            AllYoneticiler = GetAllYoneticiler()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PersonelFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.AllYoneticiler = GetAllYoneticiler();
            return View(vm);
        }

        var personel = await _db.Personeller.FindAsync(vm.PersonelId);
        if (personel == null)
            return NotFound();

        personel.SicilNo = vm.SicilNo;
        personel.AdSoyad = vm.AdSoyad;
        personel.Gorev = vm.Gorev;
        personel.ProjeAdi = vm.ProjeAdi;
        personel.Mudurluk = vm.Mudurluk;
        personel.Lokasyon = vm.Lokasyon;
        personel.IseGirisTarihi = vm.IseGirisTarihi;
        personel.IstenCikisTarihi = vm.IstenCikisTarihi;
        personel.Yonetici1Id = vm.Yonetici1Id;
        personel.Yonetici2Id = vm.Yonetici2Id;
        personel.NihaiYoneticiId = vm.NihaiYoneticiId;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Personel bilgileri güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Import()
    {
        return View(new PersonelImportViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportPreview(IFormFile? Excel)
    {
        var vm = new PersonelImportViewModel();

        if (Excel == null || Excel.Length == 0)
        {
            vm.Error = "Lütfen bir Excel dosyası seçin.";
            return View("Import", vm);
        }

        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            await Excel.CopyToAsync(ms);
            bytes = ms.ToArray();
        }

        var parsed = PersonelImportService.Parse(bytes, previewMaxRow: 50);

        if (!string.IsNullOrWhiteSpace(parsed.FatalError))
        {
            vm.Error = parsed.FatalError;
            return View("Import", vm);
        }

        vm.Headers = parsed.Headers;
        vm.Warnings = parsed.Warnings;
        vm.TotalRows = parsed.Rows.Count;
        vm.ValidRows = parsed.Rows.Count(r => r.Errors.Count == 0);
        vm.InvalidRows = parsed.Rows.Count(r => r.Errors.Count > 0);

        vm.PreviewRows = parsed.Rows.Select(r => new PersonelImportViewModel.RowPreview
        {
            RowNo = r.RowNo,
            SicilNo = r.SicilNo,
            AdSoyad = r.AdSoyad,
            Gorev = r.Gorev,
            ProjeAdi = r.ProjeAdi,
            IseGirisTarihi = r.IseGirisTarihi,
            IstenCikisTarihi = r.IstenCikisTarihi,
            Mudurluk = r.Mudurluk,
            Lokasyon = r.Lokasyon,
            Yonetici1 = r.Yonetici1,
            Yonetici2 = r.Yonetici2,
            NihaiYonetici = r.NihaiYonetici,
            Shifted = r.Shifted,
            Notes = r.Notes.ToList(),
            RowErrors = r.Errors.ToList()
        }).ToList();

        vm.PreviewReady = true;
        vm.CanImport = vm.InvalidRows == 0;

        if (!vm.CanImport)
        {
            vm.Error = $"Excel'de {vm.InvalidRows} hatalı satır var. Lütfen düzeltin.";
        }

        // Session'a kaydet (IIS-safe)
        HttpContext.Session.Set("PersonelImportBytes", bytes);

        return View("Import", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportExecute()
    {
        var bytes = HttpContext.Session.Get("PersonelImportBytes");
        if (bytes == null || bytes.Length == 0)
        {
            TempData["Error"] = "Oturum sona ermiş. Lütfen dosyayı yeniden yükleyiniz.";
            return RedirectToAction(nameof(Import));
        }

        var parsed = PersonelImportService.Parse(bytes, previewMaxRow: int.MaxValue);

        if (!string.IsNullOrWhiteSpace(parsed.FatalError) || parsed.Rows.Any(r => r.Errors.Count > 0))
        {
            TempData["Error"] = "Hatalı satırlar var. İşlem iptal edildi.";
            return RedirectToAction(nameof(Import));
        }

        var result = await PersonelImportService.DoImportAsync(_db, parsed.Rows);

        HttpContext.Session.Remove("PersonelImportBytes");

        var vm = new PersonelImportViewModel
        {
            ImportLog = $"İşlem Tamamlandı!\n\n" +
                       $"✓ Yeni Eklenen: {result.Eklenen}\n" +
                       $"✓ Güncellenen: {result.Guncellenen}\n"
        };

        if (result.Hatalar.Any())
        {
            vm.ImportLog += $"\n⚠ Hatalar:\n{string.Join("\n", result.Hatalar)}";
        }

        return View("Import", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var personel = await _db.Personeller.FindAsync(id);
        if (personel == null)
            return NotFound();

        _db.Personeller.Remove(personel);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Personel silindi.";
        return RedirectToAction(nameof(Index));
    }

    // ============================================
    // HELPER METHOD - Kod Tekrarını Önler
    // ============================================
    private List<Kullanici> GetAllYoneticiler()
    {
        return _db.Kullanicilar
            .Where(k => k.Rol == Domain.Enums.Rol.Yonetici1 ||
                        k.Rol == Domain.Enums.Rol.Yonetici2 ||
                        k.Rol == Domain.Enums.Rol.NihaiYonetici)
            .OrderBy(k => k.AdSoyad)
            .ToList();
    }
}