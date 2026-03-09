using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Infrastructure.Data;
using PerformansSitesi.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PerformansSitesi.Controllers;

public class KvkkController : Controller
{
    private readonly PerformansDbContext _db;
    private readonly ILogger<KvkkController> _logger;

    public KvkkController(PerformansDbContext db, ILogger<KvkkController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AydinlatmaMetni()
    {
        return View();
    }

    [Authorize]
    [HttpGet]
    public IActionResult DeleteMyDataConfirm()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMyData()
    {
        var userId = User.GetUserId();
        var userName = User.Identity?.Name;

        _logger.LogWarning("KVKK Data Deletion Request: User {UserId} ({UserName})", userId, userName);

        var degerlendirmeler = await _db.Degerlendirmeler
            .Where(d => d.PersonelId == userId)
            .ToListAsync();

        foreach (var deg in degerlendirmeler)
        {
            deg.Yonetici1Notu = "[VER� SAH�B� TALEB�YLE ANON�MLE�T�R�LD�]";
            deg.Yonetici2Notu = "[VER� SAH�B� TALEB�YLE ANON�MLE�T�R�LD�]";
            deg.NihaiYoneticiNotu = "[VER� SAH�B� TALEB�YLE ANON�MLE�T�R�LD�]";
            deg.GucluYonler = null;
            deg.GelisimeAcikYonler = null;
            deg.GelisimOnerileri = null;
        }

        var personel = await _db.Personeller.FindAsync(userId);
        if (personel != null)
        {
            personel.AdSoyad = $"Silinmi� Kullan�c� #{userId}";
            personel.SicilNo = $"DELETED-{userId}";
            personel.AktifMi = false;
            personel.PasifTarihi = DateTime.Now;
            personel.PasifNedeni = "KVKK Madde 11/d - Veri Sahibi Silme Talebi";
        }

        var kullanici = await _db.Kullanicilar.FindAsync(userId);
        if (kullanici != null)
        {
            kullanici.AdSoyad = $"Silinmi� Kullan�c� #{userId}";
            kullanici.Email = $"deleted{userId}@anonymized.local";
            kullanici.KullaniciAdi = $"deleted_{userId}";
            kullanici.SifreHash = "[DELETED]";
        }

        _db.AuditLogs.Add(new AuditLog
        {
            EventType = "KVKK_Data_Deletion",
            UserId = userId,
            UserName = userName,
            Note = "Veri sahibi talebiyle ki�isel veriler anonimle�tirildi (KVKK Madde 11/d)",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync();

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("DataDeleted");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult DataDeleted()
    {
        return View();
    }
}
