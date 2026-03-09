using System.ComponentModel.DataAnnotations;

namespace PerformansSitesi.Domain.Entities;

public class PerformansSorusu
{
    [Key]
    public int SoruId { get; set; }

    public int SablonId { get; set; }
    public int SiraNo { get; set; }
    
    /// <summary>
    /// Kategori/Grup ba�l��� (�rn: "G�REV VE SORUMLULUK DE�ERLEND�RMES�")
    /// </summary>
    public string Kategori { get; set; } = "";
    
    /// <summary>
    /// Soru ba�l��� (�rn: "1. �� Disiplini")
    /// </summary>
    public string SoruBaslik { get; set; } = "";
    
    /// <summary>
    /// Soru a��klamas�/detay�
    /// </summary>
    public string SoruMetni { get; set; } = "";

    public bool ZorunluMu { get; set; } = true;
}
