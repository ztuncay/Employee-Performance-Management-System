using System.ComponentModel.DataAnnotations;

namespace PerformansSitesi.Web.ViewModels;

public class PerformansSoruEditViewModel
{
    public int SoruId { get; set; }

    [Required]
    public int SablonId { get; set; } = 1;

    [Required]
    public int SiraNo { get; set; }

    public bool ZorunluMu { get; set; }

    [Required(ErrorMessage = "Kategori se�ilmelidir")]
    public string Kategori { get; set; } = "";

    [Required(ErrorMessage = "Soru ba�l��� gereklidir")]
    public string SoruBaslik { get; set; } = "";

    [Required(ErrorMessage = "Soru metni gereklidir")]
    public string SoruMetni { get; set; } = "";

    public string? Hata { get; set; }
}
