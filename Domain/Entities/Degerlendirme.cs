using PerformansSitesi.Domain.Enums;

namespace PerformansSitesi.Domain.Entities;

public class Degerlendirme
{
    public int DegerlendirmeId { get; set; }

    public int DonemId { get; set; }
    public Donem? Donem { get; set; }

    public int PersonelId { get; set; }
    public Personel? Personel { get; set; }

    public int SablonId { get; set; } = 1;

    public int? Yonetici1Id { get; set; }
    public int? Yonetici2Id { get; set; }
    public int? NihaiYoneticiId { get; set; }

    public int? ToplamPuan { get; set; } // 33-121
    public string? GenelSonuc { get; set; } 

    public DegerlendirmeDurum Durum { get; set; }

    public DateTime OlusturmaTarihi { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }

    public string? Yonetici1Notu { get; set; }
    public string? Yonetici2Notu { get; set; }
    public string? NihaiYoneticiNotu { get; set; }

    public string? GucluYonler { get; set; }
    public string? GelisimeAcikYonler { get; set; }
    public string? GelisimOnerileri { get; set; }

    public List<DegerlendirmeDetay> Detaylar { get; set; } = new();
}
