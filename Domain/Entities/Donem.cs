namespace PerformansSitesi.Domain.Entities;

public class Donem
{
    public int DonemId { get; set; }
    public string Ad { get; set; } = "";
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public bool AktifMi { get; set; }
}
