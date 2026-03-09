using System.ComponentModel.DataAnnotations;
using PerformansSitesi.Domain.Entities;

namespace PerformansSitesi.Web.ViewModels;

public class PersonelFormViewModel
{
    public int PersonelId { get; set; }

    [Required(ErrorMessage = "Sicil No zorunludur")]
    [Display(Name = "Sicil No")]
    public string SicilNo { get; set; } = "";

    [Required(ErrorMessage = "Personel Adı Soyadı zorunludur")]
    [Display(Name = "Personel Adı Soyadı")]
    public string AdSoyad { get; set; } = "";

    [Display(Name = "Görevi")]
    public string? Gorev { get; set; }

    [Display(Name = "Proje Adı")]
    public string? ProjeAdi { get; set; }

    [Display(Name = "Müdürlük")]
    public string? Mudurluk { get; set; }

    [Display(Name = "Lokasyon")]
    public string? Lokasyon { get; set; }

    [Display(Name = "İşe Giriş Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? IseGirisTarihi { get; set; }

    [Display(Name = "İşten Çıkış Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? IstenCikisTarihi { get; set; }

    [Display(Name = "1. Yönetici")]
    public int? Yonetici1Id { get; set; }

    [Display(Name = "2. Yönetici")]
    public int? Yonetici2Id { get; set; }

    [Display(Name = "Bölge Müdürü")]
    public int? NihaiYoneticiId { get; set; }

    public List<Kullanici> AllYoneticiler { get; set; } = new();

    public string? Error { get; set; }
}
