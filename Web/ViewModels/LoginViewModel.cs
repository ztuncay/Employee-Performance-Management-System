using System.ComponentModel.DataAnnotations;

namespace PerformansSitesi.Web.ViewModels;

public class LoginViewModel
{
    [Required]
    public string KullaniciAdi { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    public string Sifre { get; set; } = "";

    public bool HatirlaBeni { get; set; }

    public string? Error { get; set; }
}
