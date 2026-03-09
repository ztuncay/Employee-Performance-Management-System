using PerformansSitesi.Domain.Entities;

namespace PerformansSitesi.Domain.Entities;

public class Personel
{
    public int PersonelId { get; set; }

    public string AdSoyad { get; set; } = "";
    public string SicilNo { get; set; } = "";

    public string Gorev { get; set; } = "";
    public string ProjeAdi { get; set; } = "";
    public string Mudurluk { get; set; } = "";
    public string? Lokasyon { get; set; }

    /// <summary>
    /// Personelin i�e giri� tarihi
    /// </summary>
    public DateTime? IseGirisTarihi { get; set; }

    /// <summary>
    /// Personelin i�ten ��k�� tarihi (varsa)
    /// </summary>
    public DateTime? IstenCikisTarihi { get; set; }

    public int? Yonetici1Id { get; set; }
    public int? Yonetici2Id { get; set; }
    public int? NihaiYoneticiId { get; set; }

    public bool AktifMi { get; set; } = true;
    public DateTime? PasifTarihi { get; set; }
    public string? PasifNedeni { get; set; }

    public Kullanici? Yonetici1 { get; set; }
    public Kullanici? Yonetici2 { get; set; }
    public Kullanici? NihaiYonetici { get; set; }
}
