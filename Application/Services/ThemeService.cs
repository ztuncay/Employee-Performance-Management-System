using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Infrastructure.Data;

namespace PerformansSitesi.Application.Services;

/// <summary>
/// Site tema yönetimi servisi
/// </summary>
public class ThemeService
{
    private readonly PerformansDbContext _db;

    public ThemeService(PerformansDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Aktif temayı getir
    /// </summary>
    public async Task<SiteTema?> GetAktifTemaAsync()
    {
        return await _db.SiteTemalari
            .FirstOrDefaultAsync(t => t.AktifMi);
    }

    /// <summary>
    /// Tüm temaları listele
    /// </summary>
    public async Task<List<SiteTema>> GetAllTemasAsync()
    {
        return await _db.SiteTemalari
            .OrderByDescending(t => t.AktifMi)
            .ThenByDescending(t => t.OlusturulmaTarihi)
            .ToListAsync();
    }

    /// <summary>
    /// Tema detayını getir
    /// </summary>
    public async Task<SiteTema?> GetTemaByIdAsync(int temaId)
    {
        return await _db.SiteTemalari.FindAsync(temaId);
    }

    /// <summary>
    /// Yeni tema oluştur
    /// </summary>
    public async Task<SiteTema> CreateTemaAsync(SiteTema tema)
    {
        tema.OlusturulmaTarihi = DateTime.Now;
        
        var mevcutTemaVar = await _db.SiteTemalari.AnyAsync();
        if (!mevcutTemaVar)
        {
            tema.AktifMi = true;
        }

        _db.SiteTemalari.Add(tema);
        await _db.SaveChangesAsync();

        return tema;
    }

    /// <summary>
    /// Temayı güncelle
    /// </summary>
    public async Task<bool> UpdateTemaAsync(SiteTema tema)
    {
        var mevcutTema = await _db.SiteTemalari.FindAsync(tema.TemaId);
        if (mevcutTema == null) return false;

        mevcutTema.TemaAdi = tema.TemaAdi;
        mevcutTema.PrimaryColor = tema.PrimaryColor;
        mevcutTema.SecondaryColor = tema.SecondaryColor;
        mevcutTema.SuccessColor = tema.SuccessColor;
        mevcutTema.WarningColor = tema.WarningColor;
        mevcutTema.DangerColor = tema.DangerColor;
        mevcutTema.InfoColor = tema.InfoColor;
        mevcutTema.LightColor = tema.LightColor;
        mevcutTema.DarkColor = tema.DarkColor;
        mevcutTema.FontFamily = tema.FontFamily;
        mevcutTema.FontSize = tema.FontSize;
        mevcutTema.HeadingFontFamily = tema.HeadingFontFamily;
        mevcutTema.NavbarPosition = tema.NavbarPosition;
        mevcutTema.NavbarTheme = tema.NavbarTheme;
        mevcutTema.NavbarBgColor = tema.NavbarBgColor;
        mevcutTema.SidebarWidth = tema.SidebarWidth;
        mevcutTema.ContainerSize = tema.ContainerSize;
        mevcutTema.CardBorderRadius = tema.CardBorderRadius;
        mevcutTema.CardShadow = tema.CardShadow;
        mevcutTema.ButtonBorderRadius = tema.ButtonBorderRadius;
        mevcutTema.ButtonSize = tema.ButtonSize;
        mevcutTema.CustomCss = tema.CustomCss;
        mevcutTema.Aciklama = tema.Aciklama;
        mevcutTema.GuncellenmeTarihi = DateTime.Now;

        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Temayı aktif et (diğerlerini pasif yap)
    /// </summary>
    public async Task<bool> ActivateTemaAsync(int temaId)
    {
        var tema = await _db.SiteTemalari.FindAsync(temaId);
        if (tema == null) return false;

        var tumTemalar = await _db.SiteTemalari.ToListAsync();
        foreach (var t in tumTemalar)
        {
            t.AktifMi = false;
        }

        tema.AktifMi = true;
        tema.GuncellenmeTarihi = DateTime.Now;

        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Temayı sil
    /// </summary>
    public async Task<bool> DeleteTemaAsync(int temaId)
    {
        var tema = await _db.SiteTemalari.FindAsync(temaId);
        if (tema == null) return false;

        if (tema.AktifMi)
        {
            throw new InvalidOperationException("Aktif tema silinemez. Önce başka bir temayı aktif edin.");
        }

        _db.SiteTemalari.Remove(tema);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// CSS variables olu�tur
    /// </summary>
    public string GenerateCssVariables(SiteTema tema)
    {
        return $@"
:root {{
    /* Renkler */
    --bs-primary: {tema.PrimaryColor};
    --bs-secondary: {tema.SecondaryColor};
    --bs-success: {tema.SuccessColor};
    --bs-warning: {tema.WarningColor};
    --bs-danger: {tema.DangerColor};
    --bs-info: {tema.InfoColor};
    --bs-light: {tema.LightColor};
    --bs-dark: {tema.DarkColor};

    /* Yazılar */
    --bs-body-font-family: {tema.FontFamily};
    --bs-body-font-size: {tema.FontSize}px;
    --bs-heading-font-family: {tema.HeadingFontFamily};

    /* Layout */
    --navbar-bg-color: {tema.NavbarBgColor};
    --sidebar-width: {tema.SidebarWidth}px;

    /* Kartlar */
    --bs-card-border-radius: {tema.CardBorderRadius}px;
    
    /* Butonlar */
    --bs-btn-border-radius: {tema.ButtonBorderRadius}px;
}}

/* Navbar */
.navbar {{
    background-color: var(--navbar-bg-color) !important;
}}

/* Kartlar */
.card {{
    border-radius: var(--bs-card-border-radius);
    box-shadow: var(--bs-card-shadow-{tema.CardShadow});
}}

/* Butonlar */
.btn {{
    border-radius: var(--bs-btn-border-radius);
}}

.btn-primary {{
    background-color: var(--bs-primary);
    border-color: var(--bs-primary);
}}

.btn-primary:hover {{
    background-color: color-mix(in srgb, var(--bs-primary) 85%, black);
    border-color: color-mix(in srgb, var(--bs-primary) 85%, black);
}}

/* Link renkleri */
a {{
    color: var(--bs-primary);
}}

a:hover {{
    color: color-mix(in srgb, var(--bs-primary) 85%, black);
}}

/* Badge renkleri */
.badge.bg-primary {{
    background-color: var(--bs-primary) !important;
}}

.badge.bg-success {{
    background-color: var(--bs-success) !important;
}}

.badge.bg-warning {{
    background-color: var(--bs-warning) !important;
}}

.badge.bg-danger {{
    background-color: var(--bs-danger) !important;
}}

.badge.bg-info {{
    background-color: var(--bs-info) !important;
}}

/* Alert renkleri */
.alert-primary {{
    background-color: color-mix(in srgb, var(--bs-primary) 10%, white);
    border-color: color-mix(in srgb, var(--bs-primary) 25%, white);
    color: color-mix(in srgb, var(--bs-primary) 85%, black);
}}

.alert-success {{
    background-color: color-mix(in srgb, var(--bs-success) 10%, white);
    border-color: color-mix(in srgb, var(--bs-success) 25%, white);
    color: color-mix(in srgb, var(--bs-success) 85%, black);
}}

.alert-danger {{
    background-color: color-mix(in srgb, var(--bs-danger) 10%, white);
    border-color: color-mix(in srgb, var(--bs-danger) 25%, white);
    color: color-mix(in srgb, var(--bs-danger) 85%, black);
}}

/* Özel CSS */
{tema.CustomCss ?? ""}
";
    }

    /// <summary>
    /// Varsayılan tema oluştur
    /// </summary>
    public async Task<SiteTema> CreateDefaultTemaAsync()
    {
        var defaultTema = new SiteTema
        {
            TemaAdi = "Varsayılan (Bootstrap)",
            AktifMi = true,
            Aciklama = "Standart Bootstrap 5 teması",
            PrimaryColor = "#0d6efd",
            SecondaryColor = "#6c757d",
            SuccessColor = "#198754",
            WarningColor = "#ffc107",
            DangerColor = "#dc3545",
            InfoColor = "#0dcaf0",
            LightColor = "#f8f9fa",
            DarkColor = "#212529",
            FontFamily = "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif",
            FontSize = 14,
            HeadingFontFamily = "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif",
            NavbarPosition = "sticky-top",
            NavbarTheme = "dark",
            NavbarBgColor = "#212529",
            SidebarWidth = 250,
            ContainerSize = "fluid",
            CardBorderRadius = 8,
            CardShadow = "sm",
            ButtonBorderRadius = 4,
            ButtonSize = "md"
        };

        return await CreateTemaAsync(defaultTema);
    }
}
