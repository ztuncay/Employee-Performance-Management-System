using System.ComponentModel.DataAnnotations;

namespace PerformansSitesi.Web.ViewModels;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Mevcut şifre zorunludur")]
    [Display(Name = "Mevcut şifre")]
    [DataType(DataType.Password)]
    public string MevcutSifre { get; set; } = "";

    [Required(ErrorMessage = "Yeni şifre zorunludur")]
    [Display(Name = "Yeni şifre")]
    [DataType(DataType.Password)]
    [MinLength(4, ErrorMessage = "şifre en az 4 karakter olmalıdır")]
    public string YeniSifre { get; set; } = "";

    [Required(ErrorMessage = "şifre onayı zorunludur")]
    [Display(Name = "Yeni şifre (Tekrar)")]
    [DataType(DataType.Password)]
    [Compare("YeniSifre", ErrorMessage = "Şifreler eşleşmiyor")]
    public string YeniSifreTekrar { get; set; } = "";

    public string? Error { get; set; }
}
