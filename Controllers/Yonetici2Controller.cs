using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Application.Services;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Domain.Enums;
using PerformansSitesi.Infrastructure.Data;
using PerformansSitesi.Web.Extensions;
using PerformansSitesi.Web.ViewModels;

namespace PerformansSitesi.Controllers;

[Authorize(Roles = "Yonetici2")]
public class Yonetici2Controller : Controller
{
    private readonly PerformansDbContext _db;

    public Yonetici2Controller(PerformansDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? donemId, string? arama)
    {
        var userId = User.GetUserId();

        var donemler = await _db.Donemler
            .AsNoTracking()
            .OrderByDescending(x => x.AktifMi)
            .ThenByDescending(x => x.BaslangicTarihi)
            .ToListAsync();

        if (donemler.Count == 0) return Content("D�nem yok. �nce d�nem ekleyin.");

        var selectedDonemId = donemId
            ?? donemler.FirstOrDefault(x => x.AktifMi)?.DonemId
            ?? donemler.First().DonemId;

        var personelQuery = _db.Personeller.AsNoTracking().Where(p => p.Yonetici2Id == userId);

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();
            personelQuery = personelQuery.Where(p =>
                p.AdSoyad.Contains(arama) ||
                p.SicilNo.Contains(arama) ||
                p.Mudurluk.Contains(arama) ||
                p.ProjeAdi.Contains(arama) ||
                p.Gorev.Contains(arama));
        }

        var personeller = await personelQuery.OrderBy(p => p.AdSoyad).ToListAsync();
        var personelIds = personeller.Select(p => p.PersonelId).ToList();

        var evals = await _db.Degerlendirmeler
            .AsNoTracking()
            .Where(e => e.DonemId == selectedDonemId && personelIds.Contains(e.PersonelId))
            .ToListAsync();

        var rows = new List<Yonetici2ListViewModel.Row>();

        foreach (var p in personeller)
        {
            var e = evals.FirstOrDefault(x => x.PersonelId == p.PersonelId);
            var durum = e == null ? 0 : (int)e.Durum;

            rows.Add(new Yonetici2ListViewModel.Row
            {
                PersonelId = p.PersonelId,
                PersonelAd = p.AdSoyad,
                SicilNo = p.SicilNo,
                Gorev = p.Gorev,
                ProjeAdi = p.ProjeAdi,
                Mudurluk = p.Mudurluk,

                ToplamPuan = e?.ToplamPuan,
                GenelSonuc = e?.GenelSonuc,

                Durum = durum,
                DurumMetni = DurumHelper.DurumMetni(durum),
                DurumCssClass = DurumHelper.DurumCssClass(durum),
                DegerlendirmeId = e?.DegerlendirmeId
            });
        }

        var vm = new Yonetici2ListViewModel
        {
            DonemId = selectedDonemId,
            Donemler = donemler.Select(d => new Yonetici2ListViewModel.DonemItem { DonemId = d.DonemId, Ad = d.Ad }).ToList(),
            Arama = arama,
            ToplamPersonelSayisi = rows.Count,
            DegerlendirilenSayisi = rows.Count(r => r.Durum != 0),
            Rows = rows
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Form(int donemId, int personelId)
    {
        var userId = User.GetUserId();

        var eval = await _db.Degerlendirmeler
            .Include(e => e.Personel)
            .Include(e => e.Detaylar).ThenInclude(d => d.Soru)
            .FirstOrDefaultAsync(e => e.DonemId == donemId && e.PersonelId == personelId);

        if (eval == null) return NotFound();
        if (eval.Yonetici2Id != userId) return Forbid();

        var donem = await _db.Donemler.AsNoTracking().FirstAsync(d => d.DonemId == donemId);

        var duzenlenebilir = (eval.Durum == DegerlendirmeDurum.Yonetici2Asamasi || eval.Durum == DegerlendirmeDurum.Yonetici2Iade);
        
        if (eval.Durum == DegerlendirmeDurum.IKKalibrasyon || eval.Durum == DegerlendirmeDurum.Tamamlandi)
        {
            duzenlenebilir = false;
        }

        return View(MapToVm(donem.Ad, eval, duzenlenebilir));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDraft(Yonetici2FormViewModel vm)
    {
        var userId = User.GetUserId();

        var eval = await _db.Degerlendirmeler
            .Include(e => e.Personel)
            .Include(e => e.Detaylar).ThenInclude(d => d.Soru)
            .FirstOrDefaultAsync(e => e.DegerlendirmeId == vm.DegerlendirmeId);

        if (eval == null) return NotFound();
        if (eval.Yonetici2Id != userId) return Forbid();
        
        if (eval.Durum == DegerlendirmeDurum.IKKalibrasyon || eval.Durum == DegerlendirmeDurum.Tamamlandi)
            return Forbid();
            
        if (!(eval.Durum == DegerlendirmeDurum.Yonetici2Asamasi || eval.Durum == DegerlendirmeDurum.Yonetici2Iade)) 
            return Forbid();

        eval.Yonetici2Notu = vm.Yonetici2Notu;
        eval.Durum = DegerlendirmeDurum.Yonetici2Asamasi;
        eval.GuncellemeTarihi = DateTime.Now;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Form), new { donemId = eval.DonemId, personelId = eval.PersonelId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitToNihai(Yonetici2FormViewModel vm)
    {
        var userId = User.GetUserId();

        var eval = await _db.Degerlendirmeler
            .FirstOrDefaultAsync(e => e.DegerlendirmeId == vm.DegerlendirmeId);

        if (eval == null) return NotFound();
        if (eval.Yonetici2Id != userId) return Forbid();
        
        if (eval.Durum == DegerlendirmeDurum.IKKalibrasyon || eval.Durum == DegerlendirmeDurum.Tamamlandi)
            return Forbid();
            
        if (!(eval.Durum == DegerlendirmeDurum.Yonetici2Asamasi || eval.Durum == DegerlendirmeDurum.Yonetici2Iade)) 
            return Forbid();

        eval.Yonetici2Notu = vm.Yonetici2Notu;
        eval.Durum = DegerlendirmeDurum.NihaiYoneticiAsamasi;
        eval.GuncellemeTarihi = DateTime.Now;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { donemId = eval.DonemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnToYonetici1(Yonetici2FormViewModel vm)
    {
        var userId = User.GetUserId();

        var eval = await _db.Degerlendirmeler
            .FirstOrDefaultAsync(e => e.DegerlendirmeId == vm.DegerlendirmeId);

        if (eval == null) return NotFound();
        if (eval.Yonetici2Id != userId) return Forbid();
        
        if (eval.Durum == DegerlendirmeDurum.IKKalibrasyon || eval.Durum == DegerlendirmeDurum.Tamamlandi)
            return Forbid();
            
        if (!(eval.Durum == DegerlendirmeDurum.Yonetici2Asamasi || eval.Durum == DegerlendirmeDurum.Yonetici2Iade)) 
            return Forbid();

        eval.Yonetici2Notu = vm.Yonetici2Notu;
        eval.Durum = DegerlendirmeDurum.Yonetici1Iade;
        eval.GuncellemeTarihi = DateTime.Now;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { donemId = eval.DonemId });
    }

    private static Yonetici2FormViewModel MapToVm(string donemAd, Degerlendirme eval, bool duzenlenebilir)
    {
        return new Yonetici2FormViewModel
        {
            DonemId = eval.DonemId,
            PersonelId = eval.PersonelId,
            DegerlendirmeId = eval.DegerlendirmeId,

            DonemAd = donemAd,

            PersonelAd = eval.Personel?.AdSoyad ?? "",
            SicilNo = eval.Personel?.SicilNo ?? "",

            Gorev = eval.Personel?.Gorev ?? "",
            ProjeAdi = eval.Personel?.ProjeAdi ?? "",
            Mudurluk = eval.Personel?.Mudurluk ?? "",

            ToplamPuan = eval.ToplamPuan,
            GenelSonuc = eval.GenelSonuc ?? "",

            Yonetici1Notu = eval.Yonetici1Notu,
            Yonetici2Notu = eval.Yonetici2Notu,
            NihaiYoneticiNotu = eval.NihaiYoneticiNotu,

            GucluYonler = eval.GucluYonler,
            GelisimeAcikYonler = eval.GelisimeAcikYonler,
            GelisimOnerileri = eval.GelisimOnerileri,

            DuzenlenebilirMi = duzenlenebilir,
            RehberHtml = RehberHtmlProvider.BuildRehberHtml(),

            Sorular = eval.Detaylar
                .OrderBy(d => d.Soru!.SiraNo)
                .Select(d => new Yonetici2FormViewModel.SoruRow
                {
                    DetayId = d.DetayId,
                    SoruId = d.SoruId,
                    SiraNo = d.Soru!.SiraNo,
                    ZorunluMu = d.Soru!.ZorunluMu,
                    Baslik = d.Soru!.Kategori,
                    TemizSoruMetni = $"{d.Soru!.SoruBaslik}: {d.Soru!.SoruMetni}",

                    Yonetici1Puan = d.Yonetici1Puan,
                    Yonetici1Yorum = d.Yonetici1Yorum,

                    Yonetici2Yorum = d.Yonetici2Yorum,
                    NihaiYoneticiYorum = d.NihaiYoneticiYorum
                }).ToList()
        };
    }
}
