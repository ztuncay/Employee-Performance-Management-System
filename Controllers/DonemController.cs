using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Infrastructure.Data;
using PerformansSitesi.Web.ViewModels;

namespace PerformansSitesi.Controllers;

[Authorize(Roles = "Admin,IK")]
public class DonemController : Controller
{
    private readonly PerformansDbContext _db;

    public DonemController(PerformansDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var list = await _db.Donemler
            .OrderByDescending(x => x.AktifMi)
            .ThenByDescending(x => x.BaslangicTarihi)
            .ToListAsync();

        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var vm = new DonemFormViewModel
        {
            BaslangicTarihi = DateTime.Today,
            BitisTarihi = DateTime.Today.AddMonths(1).AddDays(-1),
            AktifMi = false
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DonemFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        if (vm.BitisTarihi < vm.BaslangicTarihi)
        {
            vm.Hata = "Bitiş tarihi başlangıç tarihinden küçük olamaz.";
            return View(vm);
        }

        if (vm.AktifMi)
        {
            var aktifler = await _db.Donemler.Where(x => x.AktifMi).ToListAsync();
            foreach (var a in aktifler) a.AktifMi = false;
        }

        var entity = new Donem
        {
            Ad = vm.Ad.Trim(),
            BaslangicTarihi = vm.BaslangicTarihi.Date,
            BitisTarihi = vm.BitisTarihi.Date,
            AktifMi = vm.AktifMi
        };

        _db.Donemler.Add(entity);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var d = await _db.Donemler.FirstOrDefaultAsync(x => x.DonemId == id);
        if (d == null) return NotFound();

        var vm = new DonemFormViewModel
        {
            DonemId = d.DonemId,
            Ad = d.Ad,
            BaslangicTarihi = d.BaslangicTarihi,
            BitisTarihi = d.BitisTarihi,
            AktifMi = d.AktifMi
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DonemFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        if (vm.BitisTarihi < vm.BaslangicTarihi)
        {
            vm.Hata = "Bitiş tarihi başlangıç tarihinden küçük olamaz.";
            return View(vm);
        }

        var d = await _db.Donemler.FirstOrDefaultAsync(x => x.DonemId == vm.DonemId);
        if (d == null) return NotFound();

        if (vm.AktifMi)
        {
            var aktifler = await _db.Donemler.Where(x => x.AktifMi && x.DonemId != vm.DonemId).ToListAsync();
            foreach (var a in aktifler) a.AktifMi = false;
        }

        d.Ad = vm.Ad.Trim();
        d.BaslangicTarihi = vm.BaslangicTarihi.Date;
        d.BitisTarihi = vm.BitisTarihi.Date;
        d.AktifMi = vm.AktifMi;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var d = await _db.Donemler.FirstOrDefaultAsync(x => x.DonemId == id);
        if (d == null) return NotFound();

        var bagli = await _db.Degerlendirmeler.AnyAsync(x => x.DonemId == id);
        if (bagli)
            return BadRequest("Bu döneme bağlı değerlendirmeler var. Silme işlemi iptal edildi.");

        _db.Donemler.Remove(d);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
