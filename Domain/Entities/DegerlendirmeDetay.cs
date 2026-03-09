using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PerformansSitesi.Domain.Entities;

public class DegerlendirmeDetay
{
    [Key]
    public int DetayId { get; set; }   // ✅ Primary Key

    public int DegerlendirmeId { get; set; }
    public Degerlendirme? Degerlendirme { get; set; }

    public int SoruId { get; set; }
    public PerformansSorusu? Soru { get; set; }

    public int? Yonetici1Puan { get; set; }

    public string? Yonetici1Yorum { get; set; }
    public string? Yonetici2Yorum { get; set; }
    public string? NihaiYoneticiYorum { get; set; }
}
