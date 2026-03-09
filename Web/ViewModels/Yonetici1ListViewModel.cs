namespace PerformansSitesi.Web.ViewModels;

public class Yonetici1ListViewModel
{
    public int DonemId { get; set; }
    public List<DonemItem> Donemler { get; set; } = new();

    public string? Arama { get; set; }

    public int DegerlendirilenSayisi { get; set; }
    public int ToplamPersonelSayisi { get; set; }

    public List<Row> Rows { get; set; } = new();

    public class DonemItem
    {
        public int DonemId { get; set; }
        public string Ad { get; set; } = "";
    }

    public class Row
    {
        public int PersonelId { get; set; }
        public string PersonelAd { get; set; } = "";
        public string SicilNo { get; set; } = "";

        public string Gorev { get; set; } = "";
        public string ProjeAdi { get; set; } = "";
        public string Mudurluk { get; set; } = "";

        public int? ToplamPuan { get; set; }
        public string? GenelSonuc { get; set; }

        public int Durum { get; set; }
        public string DurumMetni { get; set; } = "";
        public string DurumCssClass { get; set; } = "";

        public int? DegerlendirmeId { get; set; }
    }
}
