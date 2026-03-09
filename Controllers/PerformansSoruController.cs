using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Infrastructure.Data;
using PerformansSitesi.Web.ViewModels;

namespace PerformansSitesi.Controllers;

[Authorize(Roles = "Admin,SistemAdmin,IK")]
public class PerformansSoruController : Controller
{
    private readonly PerformansDbContext _db;

    public PerformansSoruController(PerformansDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int sablonId = 1)
    {
        ViewBag.SablonId = sablonId;

        var list = await _db.PerformansSorulari
            .AsNoTracking()
            .Where(x => x.SablonId == sablonId)
            .OrderBy(x => x.SiraNo)
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public IActionResult Create(int sablonId = 1)
    {
        var vm = new PerformansSoruEditViewModel
        {
            SablonId = sablonId,
            SiraNo = 1,
            ZorunluMu = true,
            Kategori = "",
            SoruBaslik = "",
            SoruMetni = ""
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PerformansSoruEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var sameOrder = await _db.PerformansSorulari
            .AnyAsync(x => x.SablonId == vm.SablonId && x.SiraNo == vm.SiraNo);

        if (sameOrder)
        {
            vm.Hata = "Bu �ablonda ayn� s�ra numaras� zaten var.";
            return View(vm);
        }

        var entity = new Domain.Entities.PerformansSorusu
        {
            SablonId = vm.SablonId,
            SiraNo = vm.SiraNo,
            ZorunluMu = vm.ZorunluMu,
            Kategori = vm.Kategori.Trim(),
            SoruBaslik = vm.SoruBaslik.Trim(),
            SoruMetni = vm.SoruMetni.Trim()
        };

        _db.PerformansSorulari.Add(entity);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { sablonId = vm.SablonId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var s = await _db.PerformansSorulari.FirstOrDefaultAsync(x => x.SoruId == id);
        if (s == null) return NotFound();

        var vm = new PerformansSoruEditViewModel
        {
            SoruId = s.SoruId,
            SablonId = s.SablonId,
            SiraNo = s.SiraNo,
            ZorunluMu = s.ZorunluMu,
            Kategori = s.Kategori,
            SoruBaslik = s.SoruBaslik,
            SoruMetni = s.SoruMetni
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PerformansSoruEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var s = await _db.PerformansSorulari.FirstOrDefaultAsync(x => x.SoruId == vm.SoruId);
        if (s == null) return NotFound();

        var sameOrder = await _db.PerformansSorulari
            .AnyAsync(x => x.SablonId == vm.SablonId && x.SiraNo == vm.SiraNo && x.SoruId != vm.SoruId);

        if (sameOrder)
        {
            vm.Hata = "Bu �ablonda ayn� s�ra numaras� zaten var.";
            return View(vm);
        }

        s.SablonId = vm.SablonId;
        s.SiraNo = vm.SiraNo;
        s.ZorunluMu = vm.ZorunluMu;
        s.Kategori = vm.Kategori.Trim();
        s.SoruBaslik = vm.SoruBaslik.Trim();
        s.SoruMetni = vm.SoruMetni.Trim();

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { sablonId = vm.SablonId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.PerformansSorulari.FirstOrDefaultAsync(x => x.SoruId == id);
        if (s == null) return NotFound();

        var kullanilmis = await _db.DegerlendirmeDetaylari.AnyAsync(x => x.SoruId == id);
        if (kullanilmis)
            return BadRequest("Bu soru de�erlendirme detaylar�nda kullan�lm��. Silme i�lemi iptal edildi.");

        _db.PerformansSorulari.Remove(s);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { sablonId = s.SablonId });
    }
}
