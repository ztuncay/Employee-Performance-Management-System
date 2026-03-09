using PerformansSitesi.Domain.Enums;

namespace PerformansSitesi.Domain.Entities;

public class Kullanici
{
    public int KullaniciId { get; set; }
    public string AdSoyad { get; set; } = "";
    public string Email { get; set; } = "";
    public string KullaniciAdi { get; set; } = "";
    public string SifreHash { get; set; } = "";
    public Rol Rol { get; set; }

    public int? PersonelId { get; set; }
    public Personel? Personel { get; set; }

    public int FailedLoginCount { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
}
