using System.ComponentModel.DataAnnotations;

namespace PerformansSitesi.Domain.Entities;

/// <summary>
/// Site tema ve tasarım ayarları
/// </summary>
public class SiteTema
{
    [Key]
    public int TemaId { get; set; }

    /// <summary>
    /// Tema adı
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TemaAdi { get; set; } = "";

    /// <summary>
    /// Aktif tema mı?
    /// </summary>
    public bool AktifMi { get; set; }

    /// <summary>
    /// Ana renk (Primary Color) - HEX format (#FF5733)
    /// </summary>
    [MaxLength(7)]
    public string PrimaryColor { get; set; } = "#0d6efd";

    /// <summary>
    /// İkincil renk (Secondary Color)
    /// </summary>
    [MaxLength(7)]
    public string SecondaryColor { get; set; } = "#6c757d";

    /// <summary>
    /// Başarı rengi (Success Color)
    /// </summary>
    [MaxLength(7)]
    public string SuccessColor { get; set; } = "#198754";

    /// <summary>
    /// Uyarı rengi (Warning Color)
    /// </summary>
    [MaxLength(7)]
    public string WarningColor { get; set; } = "#ffc107";

    /// <summary>
    /// Hata rengi (Danger Color)
    /// </summary>
    [MaxLength(7)]
    public string DangerColor { get; set; } = "#dc3545";

    /// <summary>
    /// Bilgi rengi (Info Color)
    /// </summary>
    [MaxLength(7)]
    public string InfoColor { get; set; } = "#0dcaf0";

    /// <summary>
    /// Açık renk (Light Color)
    /// </summary>
    [MaxLength(7)]
    public string LightColor { get; set; } = "#f8f9fa";

    /// <summary>
    /// Koyu renk (Dark Color)
    /// </summary>
    [MaxLength(7)]
    public string DarkColor { get; set; } = "#212529";

    /// <summary>
    /// Ana yazı tipi (Font Family)
    /// </summary>
    [MaxLength(200)]
    public string FontFamily { get; set; } = "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif";

    /// <summary>
    /// Yazı boyutu (px)
    /// </summary>
    public int FontSize { get; set; } = 14;

    /// <summary>
    /// Başlık yazı tipi
    /// </summary>
    [MaxLength(200)]
    public string HeadingFontFamily { get; set; } = "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif";

    /// <summary>
    /// Navbar pozisyonu (top, fixed-top, sticky-top)
    /// </summary>
    [MaxLength(20)]
    public string NavbarPosition { get; set; } = "sticky-top";

    /// <summary>
    /// Navbar rengi (light, dark)
    /// </summary>
    [MaxLength(10)]
    public string NavbarTheme { get; set; } = "dark";

    /// <summary>
    /// Navbar arka plan rengi
    /// </summary>
    [MaxLength(7)]
    public string NavbarBgColor { get; set; } = "#212529";

    /// <summary>
    /// Sidebar geni�li�i (px)
    /// </summary>
    public int SidebarWidth { get; set; } = 250;

    /// <summary>
    /// Container geni�li�i (fluid, xl, lg, md)
    /// </summary>
    [MaxLength(20)]
    public string ContainerSize { get; set; } = "fluid";

    /// <summary>
    /// Card border radius (px)
    /// </summary>
    public int CardBorderRadius { get; set; } = 8;

    /// <summary>
    /// Card shadow (none, sm, md, lg)
    /// </summary>
    [MaxLength(10)]
    public string CardShadow { get; set; } = "sm";

    /// <summary>
    /// Button border radius (px)
    /// </summary>
    public int ButtonBorderRadius { get; set; } = 4;

    /// <summary>
    /// Button boyutu (sm, md, lg)
    /// </summary>
    [MaxLength(10)]
    public string ButtonSize { get; set; } = "md";

    /// <summary>
    /// �zel CSS kodlar�
    /// </summary>
    public string? CustomCss { get; set; }

    /// <summary>
    /// Tema a��klamas�
    /// </summary>
    [MaxLength(500)]
    public string? Aciklama { get; set; }

    /// <summary>
    /// Olu�turulma tarihi
    /// </summary>
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

    /// <summary>
    /// Son g�ncellenme tarihi
    /// </summary>
    public DateTime? GuncellenmeTarihi { get; set; }
}
