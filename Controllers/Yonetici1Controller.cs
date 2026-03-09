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

[Authorize(Roles = "Yonetici1")]
public class Yonetici1Controller : Controller
{
    private readonly PerformansDbContext _db;

    public Yonetici1Controller(PerformansDbContext db)
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

        if (!donemler.Any())
            return View(new Yonetici1ListViewModel { Donemler = new() });

        var selectedDonemId = donemId
            ?? donemler.FirstOrDefault(x => x.AktifMi)?.DonemId
            ?? donemler.First().DonemId;

        var personelQuery = _db.Personeller
            .AsNoTracking()
            .Where(p => p.Yonetici1Id == userId);

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();
            personelQuery = personelQuery.Where(p =>
                p.AdSoyad.Contains(arama) ||
                p.SicilNo.Contains(arama) ||
                p.Mudurluk.Contains(arama) ||
                p.ProjeAdi.Contains(arama));
        }

        var personeller = await personelQuery.OrderBy(p => p.AdSoyad).ToListAsync();
        var personelIds = personeller.Select(p => p.PersonelId).ToList();

        var evals = await _db.Degerlendirmeler
            .AsNoTracking()
            .Where(e => e.DonemId == selectedDonemId && personelIds.Contains(e.PersonelId))
            .ToListAsync();

        var rows = new List<Yonetici1ListViewModel.Row>();

        foreach (var p in personeller)
        {
            var e = evals.FirstOrDefault(x => x.PersonelId == p.PersonelId);
            var durum = e == null ? 0 : (int)e.Durum;

            rows.Add(new Yonetici1ListViewModel.Row
            {
                PersonelId = p.PersonelId,
                PersonelAd = p.AdSoyad,
                SicilNo = p.SicilNo,

                Gorev = p.Gorev ?? "",
                ProjeAdi = p.ProjeAdi ?? "",
                Mudurluk = p.Mudurluk ?? "",

                ToplamPuan = e?.ToplamPuan,
                GenelSonuc = e?.GenelSonuc,

                Durum = durum,
                DurumMetni = DurumHelper.DurumMetni(durum),
                DurumCssClass = DurumHelper.DurumCssClass(durum),

                DegerlendirmeId = e?.DegerlendirmeId
            });
        }

        var vm = new Yonetici1ListViewModel
        {
            DonemId = selectedDonemId,
            Donemler = donemler.Select(d => new Yonetici1ListViewModel.DonemItem { DonemId = d.DonemId, Ad = d.Ad }).ToList(),
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

        var donem = await _db.Donemler.AsNoTracking().FirstOrDefaultAsync(d => d.DonemId == donemId);
        if (donem == null) return NotFound();

        var personel = await _db.Personeller.AsNoTracking().FirstOrDefaultAsync(p => p.PersonelId == personelId);
        if (personel == null) return NotFound();
        if (personel.Yonetici1Id != userId) return Forbid();

        var eval = await _db.Degerlendirmeler
            .Include(e => e.Personel)
            .Include(e => e.Detaylar).ThenInclude(d => d.Soru)
            .FirstOrDefaultAsync(e => e.DonemId == donemId && e.PersonelId == personelId);

        if (eval == null)
        {
            eval = new Degerlendirme
            {
                DonemId = donemId,
                PersonelId = personelId,
                SablonId = 1,
                Yonetici1Id = personel.Yonetici1Id,
                Yonetici2Id = personel.Yonetici2Id,
                NihaiYoneticiId = personel.NihaiYoneticiId,
                Durum = DegerlendirmeDurum.Yonetici1Asamasi,
                OlusturmaTarihi = DateTime.Now
            };

            var sorular = await _db.PerformansSorulari
                .AsNoTracking()
                .Where(s => s.SablonId == 1)
                .OrderBy(s => s.SiraNo)
                .ToListAsync();

            foreach (var s in sorular)
            {
                eval.Detaylar.Add(new DegerlendirmeDetay
                {
                    SoruId = s.SoruId,
                    Yonetici1Puan = null,
                    Yonetici1Yorum = ""
                });
            }

            _db.Degerlendirmeler.Add(eval);
            await _db.SaveChangesAsync();

            eval = await _db.Degerlendirmeler
                .Include(e => e.Personel)
                .Include(e => e.Detaylar).ThenInclude(d => d.Soru)
                .FirstAsync(e => e.DegerlendirmeId == eval.DegerlendirmeId);
        }

        var duzenlenebilir =
            eval.Durum == DegerlendirmeDurum.Yonetici1Asamasi ||
            eval.Durum == DegerlendirmeDurum.Yonetici1Iade;

        if (eval.Durum == DegerlendirmeDurum.IKKalibrasyon || eval.Durum == DegerlendirmeDurum.Tamamlandi)
        {
            duzenlenebilir = false;
        }

        return View(MapToVm(donem.Ad, eval, duzenlenebilir));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDraft(Yonetici1FormViewModel vm)
    {
        var (eval, forbid) = await LoadEvalForY1(vm.DegerlendirmeId);
        if (forbid != null) return forbid;

        if (eval.Durum == DegerlendirmeDurum.IKKalibrasyon || eval.Durum == DegerlendirmeDurum.Tamamlandi)
            return Forbid();

        if (!(eval.Durum == DegerlendirmeDurum.Yonetici1Asamasi || eval.Durum == DegerlendirmeDurum.Yonetici1Iade))
            return Forbid();

        ApplyVmToEval(vm, eval);

        eval.Durum = DegerlendirmeDurum.Yonetici1Asamasi;
        eval.GuncellemeTarihi = DateTime.Now;

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Form), new { donemId = eval.DonemId, personelId = eval.PersonelId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitToYonetici2(Yonetici1FormViewModel vm)
    {
        var (eval, forbid) = await LoadEvalForY1(vm.DegerlendirmeId);
        if (forbid != null) return forbid;

        if (eval.Durum == DegerlendirmeDurum.IKKalibrasyon || eval.Durum == DegerlendirmeDurum.Tamamlandi)
            return Forbid();

        if (!(eval.Durum == DegerlendirmeDurum.Yonetici1Asamasi || eval.Durum == DegerlendirmeDurum.Yonetici1Iade))
            return Forbid();

        ApplyVmToEval(vm, eval);

        var zorunluEksik = eval.Detaylar.Any(d => d.Soru!.ZorunluMu && !d.Yonetici1Puan.HasValue);
        if (zorunluEksik)
        {
            ModelState.AddModelError("", "Zorunlu sorular�n tamam�nda puan se�meden 2. Y�neticiye g�nderemezsiniz.");
            var donem = await _db.Donemler.AsNoTracking().FirstAsync(x => x.DonemId == eval.DonemId);
            return View("Form", MapToVm(donem.Ad, eval, true));
        }

        eval.Durum = DegerlendirmeDurum.Yonetici2Asamasi;
        eval.GuncellemeTarihi = DateTime.Now;

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { donemId = eval.DonemId });
    }

    private async Task<(Degerlendirme eval, IActionResult? forbid)> LoadEvalForY1(int evalId)
    {
        var userId = User.GetUserId();

        var eval = await _db.Degerlendirmeler
            .Include(e => e.Personel)
            .Include(e => e.Detaylar).ThenInclude(d => d.Soru)
            .FirstOrDefaultAsync(e => e.DegerlendirmeId == evalId);

        if (eval == null) return (null!, NotFound());
        if (eval.Yonetici1Id != userId) return (null!, Forbid());

        return (eval, null);
    }

    private void ApplyVmToEval(Yonetici1FormViewModel vm, Degerlendirme eval)
    {
        eval.Yonetici1Notu = vm.Yonetici1Notu;
        eval.GucluYonler = vm.GucluYonler;
        eval.GelisimeAcikYonler = vm.GelisimeAcikYonler;
        eval.GelisimOnerileri = vm.GelisimOnerileri;

        var dict = eval.Detaylar.ToDictionary(x => x.DetayId);
        foreach (var r in vm.Sorular)
        {
            if (!dict.TryGetValue(r.DetayId, out var d)) continue;

            d.Yonetici1Puan = r.Yonetici1Puan;
            d.Yonetici1Yorum = r.Yonetici1Yorum;
        }

        var (toplam, sonuc) = PerformanceCalculation.CalculateTotal(eval.Detaylar.Select(x => x.Yonetici1Puan));
        eval.ToplamPuan = toplam;
        eval.GenelSonuc = sonuc;
    }

    private static Yonetici1FormViewModel MapToVm(string donemAd, Degerlendirme eval, bool duzenlenebilir)
    {
        return new Yonetici1FormViewModel
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
                .Select(d => new Yonetici1FormViewModel.SoruRow
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
