using System.ComponentModel.DataAnnotations;

namespace PerformansSitesi.Web.ViewModels;

public class KullaniciPasswordViewModel
{
    public int KullaniciId { get; set; }
    public string AdSoyad { get; set; } = "";

    [Required, MinLength(4)]
    [DataType(DataType.Password)]
    public string YeniSifre { get; set; } = "";

    public string? Hata { get; set; }
}
