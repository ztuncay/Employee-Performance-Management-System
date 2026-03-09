using System.ComponentModel.DataAnnotations;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Domain.Enums;

namespace PerformansSitesi.Web.ViewModels;

public class KullaniciEditViewModel
{
    public int KullaniciId { get; set; }

    [Required(ErrorMessage = "Ad Soyad zorunludur")]
    [MaxLength(200)]
    [Display(Name = "Ad Soyad")]
    public string AdSoyad { get; set; } = "";

    [MaxLength(200)]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Kullanıcı Adı zorunludur")]
    [MaxLength(100)]
    [Display(Name = "Kullanıcı Adı")]
    public string KullaniciAdi { get; set; } = "";

    [Required(ErrorMessage = "Rol seçimi zorunludur")]
    [Display(Name = "Rol")]
    public Rol Rol { get; set; }

    [Display(Name = "Bağlı Personel")]
    public int? PersonelId { get; set; }

    public List<Personel> AllPersoneller { get; set; } = new();

    public string? Hata { get; set; }
}