using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Application.Services;
using PerformansSitesi.Domain.Enums;
using PerformansSitesi.Infrastructure.Data;
using PerformansSitesi.Web.ViewModels;

namespace PerformansSitesi.Controllers;

[Authorize(Roles = "IK,Admin,SistemAdmin")]
public class IKController : Controller
{
    private readonly PerformansDbContext _db;

    public IKController(PerformansDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int? donemId, 
        string? arama,
        int? durumFiltre,
        string? genelSonucFiltre,
        int? minPuan,
        int? maxPuan,
        string? gorevFiltre,
        string? projeFiltre)
    {
        var donemler = await _db.Donemler
            .AsNoTracking()
            .OrderByDescending(x => x.AktifMi)
            .ThenByDescending(x => x.BaslangicTarihi)
            .ToListAsync();

        if (donemler.Count == 0) return Content("Dönem yok. önce dönem ekleyin.");

        var selectedDonemId = donemId
            ?? donemler.FirstOrDefault(x => x.AktifMi)?.DonemId
            ?? donemler.First().DonemId;

        var personelQuery = _db.Personeller
            .AsNoTracking()
            .Include(p => p.Yonetici1)
            .Include(p => p.Yonetici2)
            .Include(p => p.NihaiYonetici)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();
            personelQuery = personelQuery.Where(p =>
                p.AdSoyad.Contains(arama) ||
                p.SicilNo.Contains(arama) ||
                p.Mudurluk.Contains(arama) ||
                p.ProjeAdi.Contains(arama) ||
                p.Gorev.Contains(arama) ||
                (p.Yonetici1 != null && p.Yonetici1.AdSoyad.Contains(arama)) ||
                (p.Yonetici2 != null && p.Yonetici2.AdSoyad.Contains(arama)) ||
                (p.NihaiYonetici != null && p.NihaiYonetici.AdSoyad.Contains(arama)));
        }

        if (!string.IsNullOrWhiteSpace(gorevFiltre))
        {
            personelQuery = personelQuery.Where(p => p.Gorev == gorevFiltre);
        }

        if (!string.IsNullOrWhiteSpace(projeFiltre))
        {
            personelQuery = personelQuery.Where(p => p.ProjeAdi == projeFiltre);
        }

        var personeller = await personelQuery.OrderBy(p => p.AdSoyad).ToListAsync();
        var personelIds = personeller.Select(p => p.PersonelId).ToList();

        var evalDict = await _db.Degerlendirmeler
            .AsNoTracking()
            .Where(e => e.DonemId == selectedDonemId && personelIds.Contains(e.PersonelId))
            .ToDictionaryAsync(e => e.PersonelId);

        var rows = new List<IKListViewModel.Row>();

        foreach (var p in personeller)
        {
            evalDict.TryGetValue(p.PersonelId, out var e);
            var durum = e == null ? 0 : (int)e.Durum;

            rows.Add(new IKListViewModel.Row
            {
                PersonelId = p.PersonelId,
                PersonelAd = p.AdSoyad,
                SicilNo = p.SicilNo,

                Gorev = p.Gorev,
                ProjeAdi = p.ProjeAdi,
                Mudurluk = p.Mudurluk,

                Yonetici1Ad = p.Yonetici1?.AdSoyad ?? "",
                Yonetici2Ad = p.Yonetici2?.AdSoyad ?? "",
                NihaiYoneticiAd = p.NihaiYonetici?.AdSoyad ?? "",

                ToplamPuan = e?.ToplamPuan,
                GenelSonuc = e?.GenelSonuc,

                Durum = durum,
                DurumMetni = DurumHelper.DurumMetni(durum),
                DurumCssClass = DurumHelper.DurumCssClass(durum),

                DegerlendirmeId = e?.DegerlendirmeId
            });
        }

        // Durum filtresini uygula
        if (durumFiltre.HasValue)
        {
            rows = rows.Where(r => r.Durum == durumFiltre.Value).ToList();
        }

        // Genel sonuç filtresini uygula
        if (!string.IsNullOrWhiteSpace(genelSonucFiltre))
        {
            rows = rows.Where(r => r.GenelSonuc == genelSonucFiltre).ToList();
        }

        // Puan filtrelerini uygula
        if (minPuan.HasValue)
        {
            rows = rows.Where(r => r.ToplamPuan.HasValue && r.ToplamPuan.Value >= minPuan.Value).ToList();
        }

        if (maxPuan.HasValue)
        {
            rows = rows.Where(r => r.ToplamPuan.HasValue && r.ToplamPuan.Value <= maxPuan.Value).ToList();
        }

        // Filtre seçenekleri için listeleri oluştur
        var allPersoneller = await _db.Personeller
            .AsNoTracking()
            .ToListAsync();

        var gorevListesi = allPersoneller
            .Where(p => !string.IsNullOrWhiteSpace(p.Gorev))
            .Select(p => p.Gorev)
            .Distinct()
            .OrderBy(g => g)
            .ToList();

        var projeListesi = allPersoneller
            .Where(p => !string.IsNullOrWhiteSpace(p.ProjeAdi))
            .Select(p => p.ProjeAdi)
            .Distinct()
            .OrderBy(pr => pr)
            .ToList();

        var vm = new IKListViewModel
        {
            DonemId = selectedDonemId,
            Donemler = donemler.Select(d => new IKListViewModel.DonemItem { DonemId = d.DonemId, Ad = d.Ad }).ToList(),
            Arama = arama,
            DurumFiltre = durumFiltre,
            GenelSonucFiltre = genelSonucFiltre,
            MinPuan = minPuan,
            MaxPuan = maxPuan,
            GorevFiltre = gorevFiltre,
            ProjeFiltre = projeFiltre,
            Rows = rows,
            ToplamPersonelSayisi = rows.Count,
            DegerlendirilenSayisi = rows.Count(r => r.Durum != 0),
            KalibrasyondakiSayisi = rows.Count(r => r.Durum == 4),
            GorevListesi = gorevListesi,
            ProjeListesi = projeListesi
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ViewForm(int degerlendirmeId)
    {
        var eval = await _db.Degerlendirmeler
            .Include(e => e.Donem)
            .Include(e => e.Personel).ThenInclude(p => p!.Yonetici1)
            .Include(e => e.Personel).ThenInclude(p => p!.Yonetici2)
            .Include(e => e.Personel).ThenInclude(p => p!.NihaiYonetici)
            .Include(e => e.Detaylar).ThenInclude(d => d.Soru)
            .FirstOrDefaultAsync(e => e.DegerlendirmeId == degerlendirmeId);

        if (eval == null) return NotFound();

        return View(MapToVm(eval));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnToYonetici1(int degerlendirmeId)
    {
        var eval = await _db.Degerlendirmeler.FirstOrDefaultAsync(e => e.DegerlendirmeId == degerlendirmeId);
        if (eval == null) return NotFound();

        if (eval.Durum != DegerlendirmeDurum.IKKalibrasyon) return Forbid();

        eval.Durum = DegerlendirmeDurum.Yonetici1Iade;
        eval.GuncellemeTarihi = DateTime.Now;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { donemId = eval.DonemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnToYonetici2(int degerlendirmeId)
    {
        var eval = await _db.Degerlendirmeler.FirstOrDefaultAsync(e => e.DegerlendirmeId == degerlendirmeId);
        if (eval == null) return NotFound();

        if (eval.Durum != DegerlendirmeDurum.IKKalibrasyon) return Forbid();

        eval.Durum = DegerlendirmeDurum.Yonetici2Iade;
        eval.GuncellemeTarihi = DateTime.Now;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { donemId = eval.DonemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnToNihai(int degerlendirmeId)
    {
        var eval = await _db.Degerlendirmeler.FirstOrDefaultAsync(e => e.DegerlendirmeId == degerlendirmeId);
        if (eval == null) return NotFound();

        if (eval.Durum != DegerlendirmeDurum.IKKalibrasyon) return Forbid();

        eval.Durum = DegerlendirmeDurum.NihaiYoneticiIade;
        eval.GuncellemeTarihi = DateTime.Now;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { donemId = eval.DonemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int degerlendirmeId)
    {
        var eval = await _db.Degerlendirmeler.FirstOrDefaultAsync(e => e.DegerlendirmeId == degerlendirmeId);
        if (eval == null) return NotFound();

        if (eval.Durum != DegerlendirmeDurum.IKKalibrasyon) return Forbid();

        eval.Durum = DegerlendirmeDurum.Tamamlandi;
        eval.GuncellemeTarihi = DateTime.Now;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { donemId = eval.DonemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkComplete(int donemId, List<int> degerlendirmeIds)
    {
        if (degerlendirmeIds == null || degerlendirmeIds.Count == 0)
        {
            TempData["Error"] = "Hiçbir kayıt seçilmedi.";
            return RedirectToAction(nameof(Index), new { donemId });
        }

        var evaluations = await _db.Degerlendirmeler
            .Where(e => degerlendirmeIds.Contains(e.DegerlendirmeId) && e.Durum == DegerlendirmeDurum.IKKalibrasyon)
            .ToListAsync();

        if (evaluations.Count == 0)
        {
            TempData["Error"] = "Kalibrasyon aşamasında uygun kayıt bulunamadı.";
            return RedirectToAction(nameof(Index), new { donemId });
        }

        foreach (var eval in evaluations)
        {
            eval.Durum = DegerlendirmeDurum.Tamamlandi;
            eval.GuncellemeTarihi = DateTime.Now;
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = $"{evaluations.Count} adet değerlendirme tamamlandı statüsüne alındı.";
        return RedirectToAction(nameof(Index), new { donemId });
    }

    private static IKFormViewModel MapToVm(Domain.Entities.Degerlendirme eval)
    {
        string Clean(string s) => s.Contains("]") ? s[(s.IndexOf(']') + 1)..].Trim() : s.Trim();
        string Group(string s)
        {
            if (!s.StartsWith("[")) return "";
            var end = s.IndexOf(']');
            return end > 0 ? s[1..end] : "";
        }

        var durumInt = (int)eval.Durum;

        return new IKFormViewModel
        {
            DonemId = eval.DonemId,
            PersonelId = eval.PersonelId,
            DegerlendirmeId = eval.DegerlendirmeId,

            DonemAd = eval.Donem?.Ad ?? "",
            PersonelAd = eval.Personel?.AdSoyad ?? "",
            SicilNo = eval.Personel?.SicilNo ?? "",

            Gorev = eval.Personel?.Gorev ?? "",
            ProjeAdi = eval.Personel?.ProjeAdi ?? "",
            Mudurluk = eval.Personel?.Mudurluk ?? "",

            Yonetici1Ad = eval.Personel?.Yonetici1?.AdSoyad ?? "",
            Yonetici2Ad = eval.Personel?.Yonetici2?.AdSoyad ?? "",
            NihaiYoneticiAd = eval.Personel?.NihaiYonetici?.AdSoyad ?? "",

            ToplamPuan = eval.ToplamPuan,
            GenelSonuc = eval.GenelSonuc ?? "",

            Durum = durumInt,
            DurumMetni = DurumHelper.DurumMetni(durumInt),
            KalibrasyondaMi = eval.Durum == DegerlendirmeDurum.IKKalibrasyon,

            Yonetici1Notu = eval.Yonetici1Notu,
            Yonetici2Notu = eval.Yonetici2Notu,
            NihaiYoneticiNotu = eval.NihaiYoneticiNotu,

            GucluYonler = eval.GucluYonler,
            GelisimeAcikYonler = eval.GelisimeAcikYonler,
            GelisimOnerileri = eval.GelisimOnerileri,

            Sorular = eval.Detaylar
                .OrderBy(d => d.Soru!.SiraNo)
                .Select(d => new IKFormViewModel.SoruRow
                {
                    SiraNo = d.Soru!.SiraNo,
                    ZorunluMu = d.Soru!.ZorunluMu,
                    Baslik = d.Soru!.Kategori,
                    TemizSoruMetni = $"{d.Soru!.SoruBaslik}: {d.Soru!.SoruMetni}",
                    Yonetici1Puan = d.Yonetici1Puan,
                    Yonetici1Yorum = d.Yonetici1Yorum
                }).ToList()
        };
    }
}
