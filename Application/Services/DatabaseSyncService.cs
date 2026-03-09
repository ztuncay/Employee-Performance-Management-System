using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Infrastructure.Data;

namespace PerformansSitesi.Application.Services;

/// <summary>
/// Veritaban� otomatik senkronizasyon ve de�i�iklik takip servisi
/// Her de�i�iklik otomatik olarak hem kodda hem veritaban�nda g�ncellenir
/// </summary>
public class DatabaseSyncService
{
    private readonly PerformansDbContext _db;
    private readonly ILogger<DatabaseSyncService> _logger;

    public DatabaseSyncService(PerformansDbContext db, ILogger<DatabaseSyncService> logger)
    {
        _db = db;
        _logger = logger;
    }

    #region Kullan�c� ��lemleri

    /// <summary>
    /// Kullan�c� ekle - Hem kodda hem veritaban�nda
    /// </summary>
    public async Task<(bool Success, string Message, Kullanici? User)> AddUserAsync(Kullanici user)
    {
        try
        {
            var exists = await _db.Kullanicilar.AnyAsync(k => k.KullaniciAdi == user.KullaniciAdi);
            if (exists)
            {
                return (false, "Bu kullan�c� ad� zaten kullan�l�yor.", null);
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                var emailExists = await _db.Kullanicilar.AnyAsync(k => k.Email == user.Email);
                if (emailExists)
                {
                    return (false, "Bu email zaten kullan�l�yor.", null);
                }
            }

            _db.Kullanicilar.Add(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Kullan�c� eklendi: {user.KullaniciAdi} (ID: {user.KullaniciId})");

            return (true, "Kullan�c� ba�ar�yla eklendi.", user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Kullan�c� eklenirken hata: {user.KullaniciAdi}");
            return (false, $"Hata: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Kullan�c� g�ncelle - Hem kodda hem veritaban�nda
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateUserAsync(Kullanici updatedUser)
    {
        try
        {
            var user = await _db.Kullanicilar.FindAsync(updatedUser.KullaniciId);
            if (user == null)
            {
                return (false, "Kullan�c� bulunamad�.");
            }

            user.AdSoyad = updatedUser.AdSoyad;
            user.Email = updatedUser.Email;
            user.KullaniciAdi = updatedUser.KullaniciAdi;
            user.Rol = updatedUser.Rol;
            user.PersonelId = updatedUser.PersonelId;

            if (!string.IsNullOrEmpty(updatedUser.SifreHash) && user.SifreHash != updatedUser.SifreHash)
            {
                user.SifreHash = updatedUser.SifreHash;
                _logger.LogInformation($"Kullan�c� �ifresi de�i�tirildi: {user.KullaniciAdi}");
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation($"Kullan�c� g�ncellendi: {user.KullaniciAdi} (ID: {user.KullaniciId})");

            return (true, "Kullan�c� ba�ar�yla g�ncellendi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Kullan�c� g�ncellenirken hata: ID {updatedUser.KullaniciId}");
            return (false, $"Hata: {ex.Message}");
        }
    }

    /// <summary>
    /// Kullan�c� sil - Hem kodda hem veritaban�nda
    /// </summary>
    public async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
    {
        try
        {
            var user = await _db.Kullanicilar.FindAsync(userId);
            if (user == null)
            {
                return (false, "Kullan�c� bulunamad�.");
            }

            var hasPersonel = await _db.Personeller.AnyAsync(p =>
                p.Yonetici1Id == userId || p.Yonetici2Id == userId || p.NihaiYoneticiId == userId);

            if (hasPersonel)
            {
                return (false, "Bu kullan�c� personel kay�tlar�nda kullan�l�yor. Silme i�lemi iptal edildi.");
            }

            _db.Kullanicilar.Remove(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Kullan�c� silindi: {user.KullaniciAdi} (ID: {user.KullaniciId})");

            return (true, "Kullan�c� ba�ar�yla silindi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Kullan�c� silinirken hata: ID {userId}");
            return (false, $"Hata: {ex.Message}");
        }
    }

    #endregion

    #region Personel ��lemleri

    /// <summary>
    /// Personel ekle - Hem kodda hem veritaban�nda
    /// </summary>
    public async Task<(bool Success, string Message, Personel? Personel)> AddPersonelAsync(Personel personel)
    {
        try
        {
            var exists = await _db.Personeller.AnyAsync(p => p.SicilNo == personel.SicilNo);
            if (exists)
            {
                return (false, "Bu sicil numaras� zaten kullan�l�yor.", null);
            }

            _db.Personeller.Add(personel);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Personel eklendi: {personel.AdSoyad} (Sicil: {personel.SicilNo})");

            return (true, "Personel ba�ar�yla eklendi.", personel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Personel eklenirken hata: {personel.AdSoyad}");
            return (false, $"Hata: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Personel g�ncelle - Hem kodda hem veritaban�nda
    /// </summary>
    public async Task<(bool Success, string Message)> UpdatePersonelAsync(Personel updatedPersonel)
    {
        try
        {
            var personel = await _db.Personeller.FindAsync(updatedPersonel.PersonelId);
            if (personel == null)
            {
                return (false, "Personel bulunamad�.");
            }

            personel.AdSoyad = updatedPersonel.AdSoyad;
            personel.SicilNo = updatedPersonel.SicilNo;
            personel.Gorev = updatedPersonel.Gorev;
            personel.ProjeAdi = updatedPersonel.ProjeAdi;
            personel.Mudurluk = updatedPersonel.Mudurluk;
            personel.IseGirisTarihi = updatedPersonel.IseGirisTarihi;
            personel.IstenCikisTarihi = updatedPersonel.IstenCikisTarihi;
            personel.Yonetici1Id = updatedPersonel.Yonetici1Id;
            personel.Yonetici2Id = updatedPersonel.Yonetici2Id;
            personel.NihaiYoneticiId = updatedPersonel.NihaiYoneticiId;
            personel.AktifMi = updatedPersonel.AktifMi;
            personel.PasifTarihi = updatedPersonel.PasifTarihi;
            personel.PasifNedeni = updatedPersonel.PasifNedeni;

            await _db.SaveChangesAsync();

            _logger.LogInformation($"Personel g�ncellendi: {personel.AdSoyad} (ID: {personel.PersonelId})");

            return (true, "Personel ba�ar�yla g�ncellendi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Personel g�ncellenirken hata: ID {updatedPersonel.PersonelId}");
            return (false, $"Hata: {ex.Message}");
        }
    }

    #endregion

    #region Soru ��lemleri

    /// <summary>
    /// Soru ekle - Hem kodda hem veritaban�nda
    /// </summary>
    public async Task<(bool Success, string Message, PerformansSorusu? Soru)> AddSoruAsync(PerformansSorusu soru)
    {
        try
        {
            var exists = await _db.PerformansSorulari.AnyAsync(s => s.SiraNo == soru.SiraNo);
            if (exists)
            {
                return (false, "Bu s�ra numaras� zaten kullan�l�yor.", null);
            }

            _db.PerformansSorulari.Add(soru);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Soru eklendi: {soru.SoruBaslik} (S�ra: {soru.SiraNo})");

            return (true, "Soru ba�ar�yla eklendi.", soru);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Soru eklenirken hata: {soru.SoruBaslik}");
            return (false, $"Hata: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Soru g�ncelle - Hem kodda hem veritaban�nda
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateSoruAsync(PerformansSorusu updatedSoru)
    {
        try
        {
            var soru = await _db.PerformansSorulari.FindAsync(updatedSoru.SoruId);
            if (soru == null)
            {
                return (false, "Soru bulunamad�.");
            }

            soru.SiraNo = updatedSoru.SiraNo;
            soru.Kategori = updatedSoru.Kategori;
            soru.SoruBaslik = updatedSoru.SoruBaslik;
            soru.SoruMetni = updatedSoru.SoruMetni;

            await _db.SaveChangesAsync();

            _logger.LogInformation($"Soru g�ncellendi: {soru.SoruBaslik} (ID: {soru.SoruId})");

            return (true, "Soru ba�ar�yla g�ncellendi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Soru g�ncellenirken hata: ID {updatedSoru.SoruId}");
            return (false, $"Hata: {ex.Message}");
        }
    }

    #endregion

    #region Tema ��lemleri

    /// <summary>
    /// Tema ekle - Hem kodda hem veritaban�nda
    /// </summary>
    public async Task<(bool Success, string Message, SiteTema? Tema)> AddTemaAsync(SiteTema tema)
    {
        try
        {
            tema.OlusturulmaTarihi = DateTime.Now;

            var mevcutTemaVar = await _db.SiteTemalari.AnyAsync();
            if (!mevcutTemaVar)
            {
                tema.AktifMi = true;
            }

            _db.SiteTemalari.Add(tema);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Tema eklendi: {tema.TemaAdi} (ID: {tema.TemaId})");

            return (true, "Tema ba�ar�yla eklendi.", tema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Tema eklenirken hata: {tema.TemaAdi}");
            return (false, $"Hata: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Tema g�ncelle - Hem kodda hem veritaban�nda
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateTemaAsync(SiteTema updatedTema)
    {
        try
        {
            var tema = await _db.SiteTemalari.FindAsync(updatedTema.TemaId);
            if (tema == null)
            {
                return (false, "Tema bulunamad�.");
            }

            tema.TemaAdi = updatedTema.TemaAdi;
            tema.PrimaryColor = updatedTema.PrimaryColor;
            tema.SecondaryColor = updatedTema.SecondaryColor;
            tema.SuccessColor = updatedTema.SuccessColor;
            tema.WarningColor = updatedTema.WarningColor;
            tema.DangerColor = updatedTema.DangerColor;
            tema.InfoColor = updatedTema.InfoColor;
            tema.LightColor = updatedTema.LightColor;
            tema.DarkColor = updatedTema.DarkColor;
            tema.FontFamily = updatedTema.FontFamily;
            tema.FontSize = updatedTema.FontSize;
            tema.HeadingFontFamily = updatedTema.HeadingFontFamily;
            tema.NavbarPosition = updatedTema.NavbarPosition;
            tema.NavbarTheme = updatedTema.NavbarTheme;
            tema.NavbarBgColor = updatedTema.NavbarBgColor;
            tema.SidebarWidth = updatedTema.SidebarWidth;
            tema.ContainerSize = updatedTema.ContainerSize;
            tema.CardBorderRadius = updatedTema.CardBorderRadius;
            tema.CardShadow = updatedTema.CardShadow;
            tema.ButtonBorderRadius = updatedTema.ButtonBorderRadius;
            tema.ButtonSize = updatedTema.ButtonSize;
            tema.CustomCss = updatedTema.CustomCss;
            tema.Aciklama = updatedTema.Aciklama;
            tema.GuncellenmeTarihi = DateTime.Now;

            await _db.SaveChangesAsync();

            _logger.LogInformation($"Tema g�ncellendi: {tema.TemaAdi} (ID: {tema.TemaId})");

            return (true, "Tema ba�ar�yla g�ncellendi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Tema g�ncellenirken hata: ID {updatedTema.TemaId}");
            return (false, $"Hata: {ex.Message}");
        }
    }

    /// <summary>
    /// Tema aktif et - Di�er temalar� pasif yap
    /// </summary>
    public async Task<(bool Success, string Message)> ActivateTemaAsync(int temaId)
    {
        try
        {
            var tema = await _db.SiteTemalari.FindAsync(temaId);
            if (tema == null)
            {
                return (false, "Tema bulunamad�.");
            }

            var tumTemalar = await _db.SiteTemalari.ToListAsync();
            foreach (var t in tumTemalar)
            {
                t.AktifMi = false;
            }

            tema.AktifMi = true;
            tema.GuncellenmeTarihi = DateTime.Now;

            await _db.SaveChangesAsync();

            _logger.LogInformation($"Tema aktif edildi: {tema.TemaAdi} (ID: {tema.TemaId})");

            return (true, $"'{tema.TemaAdi}' temas� aktif edildi. Site tasar�m� g�ncellendi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Tema aktif edilirken hata: ID {temaId}");
            return (false, $"Hata: {ex.Message}");
        }
    }

    #endregion

    #region Toplu Senkronizasyon

    /// <summary>
    /// T�m pending de�i�iklikleri veritaban�na kaydet
    /// </summary>
    public async Task<(bool Success, string Message, int ChangeCount)> SaveAllChangesAsync()
    {
        try
        {
            var changeCount = await _db.SaveChangesAsync();

            _logger.LogInformation($"Toplu senkronizasyon tamamland�: {changeCount} de�i�iklik kaydedildi.");

            return (true, $"{changeCount} de�i�iklik ba�ar�yla kaydedildi.", changeCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu senkronizasyon hatas�");
            return (false, $"Hata: {ex.Message}", 0);
        }
    }

    /// <summary>
    /// Veritaban� de�i�ikliklerini geri al
    /// </summary>
    public void RollbackChanges()
    {
        var entries = _db.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged)
            .ToList();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Modified:
                case EntityState.Deleted:
                    entry.Reload();
                    break;
            }
        }

        _logger.LogWarning("Veritaban� de�i�iklikleri geri al�nd�.");
    }

    #endregion

    #region De�i�iklik Takibi

    /// <summary>
    /// Pending de�i�iklikleri kontrol et
    /// </summary>
    public List<string> GetPendingChanges()
    {
        var changes = new List<string>();

        var entries = _db.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged);

        foreach (var entry in entries)
        {
            var entityName = entry.Entity.GetType().Name;
            var state = entry.State.ToString();

            changes.Add($"{entityName}: {state}");
        }

        return changes;
    }

    /// <summary>
    /// Veritaban� ba�lant�s�n� test et
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            await _db.Database.CanConnectAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Veritaban� ba�lant� testi ba�ar�s�z");
            return false;
        }
    }

    #endregion
}
