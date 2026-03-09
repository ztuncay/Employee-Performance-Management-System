using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Infrastructure.Data;
using PerformansSitesi.Web.Extensions;
using PerformansSitesi.Web.ViewModels;
using System.Security.Claims;

namespace PerformansSitesi.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly PerformansDbContext _db;

    public DashboardController(PerformansDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? kullaniciAdi, int? donemId)
    {
        var selectedDonemId = donemId
            ?? await _db.Donemler
                .OrderByDescending(x => x.AktifMi)
                .ThenByDescending(x => x.BaslangicTarihi)
                .Select(x => x.DonemId)
                .FirstAsync();

        var donem = await _db.Donemler.AsNoTracking().FirstAsync(d => d.DonemId == selectedDonemId);

        var toplamPersonel = await _db.Personeller.AsNoTracking().CountAsync();

        var evals = await _db.Degerlendirmeler
            .AsNoTracking()
            .Where(e => e.DonemId == selectedDonemId)
            .Select(e => new { Durum = (int)e.Durum, e.GenelSonuc })
            .ToListAsync();

        var baslayan = evals.Count;
        var kalibrasyonda = evals.Count(x => x.Durum == 4);
        var tamamlanan = evals.Count(x => x.Durum == 5);

        int CountSonuc(string s) => evals.Count(x => (x.GenelSonuc ?? "") == s);

        var alti = CountSonuc("Beklenen Altı");
        var beklenen = CountSonuc("Beklenen");
        var ustu = CountSonuc("Beklenen Üstü");
        var totalScored = alti + beklenen + ustu;

        double Pct(int n) => totalScored == 0 ? 0 : Math.Round((n * 100.0) / totalScored, 2);

        var userId = User.GetUserId();
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "";

        var bagliPersonelIds = new List<int>();
        var bagliDegerlendirmeler = new List<dynamic>();

        if (userRole == "Yonetici1")
        {
            bagliPersonelIds = await _db.Personeller
                .AsNoTracking()
                .Where(p => p.Yonetici1Id == userId)
                .Select(p => p.PersonelId)
                .ToListAsync();
        }
        else if (userRole == "Yonetici2")
        {
            bagliPersonelIds = await _db.Personeller
                .AsNoTracking()
                .Where(p => p.Yonetici2Id == userId)
                .Select(p => p.PersonelId)
                .ToListAsync();
        }
        else if (userRole == "NihaiYonetici")
        {
            bagliPersonelIds = await _db.Personeller
                .AsNoTracking()
                .Where(p => p.NihaiYoneticiId == userId)
                .Select(p => p.PersonelId)
                .ToListAsync();
        }

        if (bagliPersonelIds.Count > 0)
        {
            bagliDegerlendirmeler = await _db.Degerlendirmeler
                .AsNoTracking()
                .Where(e => e.DonemId == selectedDonemId && bagliPersonelIds.Contains(e.PersonelId))
                .Select(e => new { Durum = (int)e.Durum, e.GenelSonuc })
                .Cast<dynamic>()
                .ToListAsync();
        }

        var bagliBaslayan = bagliDegerlendirmeler.Count;
        var bagliKalibrasyonda = bagliDegerlendirmeler.Count(x => (int)x.Durum == 4);
        var bagliTamamlanan = bagliDegerlendirmeler.Count(x => (int)x.Durum == 5);

        int CountBagliSonuc(string s) => bagliDegerlendirmeler.Count(x => (x.GenelSonuc ?? "") == s);

        var bagliAlti = CountBagliSonuc("Beklenen Altı");
        var bagliBeklenen = CountBagliSonuc("Beklenen");
        var bagliUstu = CountBagliSonuc("Beklenen Üstü");
        var bagliTotalScored = bagliAlti + bagliBeklenen + bagliUstu;

        double BagliPct(int n) => bagliTotalScored == 0 ? 0 : Math.Round((n * 100.0) / bagliTotalScored, 2);

        var vm = new DashboardViewModel
        {
            DonemId = donem.DonemId,
            DonemAd = donem.Ad,
            ToplamPersonel = toplamPersonel,
            DegerlendirmeBaslayan = baslayan,
            Kalibrasyonda = kalibrasyonda,
            Tamamlanan = tamamlanan,
            Distribution = new DashboardViewModel.DistributionBlock
            {
                BeklenenAlti = alti,
                Beklenen = beklenen,
                BeklenenUstu = ustu,
                BeklenenAltiPct = Pct(alti),
                BeklenenPct = Pct(beklenen),
                BeklenenUstuPct = Pct(ustu)
            },
            BagliPersonelSayisi = bagliPersonelIds.Count,
            BagliDegerlendirmeBaslayan = bagliBaslayan,
            BagliKalibrasyonda = bagliKalibrasyonda,
            BagliTamamlanan = bagliTamamlanan,
            BagliDistribution = new DashboardViewModel.DistributionBlock
            {
                BeklenenAlti = bagliAlti,
                Beklenen = bagliBeklenen,
                BeklenenUstu = bagliUstu,
                BeklenenAltiPct = BagliPct(bagliAlti),
                BeklenenPct = BagliPct(bagliBeklenen),
                BeklenenUstuPct = BagliPct(bagliUstu)
            },
            UserRole = userRole,
            UserName = userName
        };

        return View(vm);
    }
}
