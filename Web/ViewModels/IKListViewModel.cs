namespace PerformansSitesi.Web.ViewModels;

public class IKListViewModel
{
    public int DonemId { get; set; }
    public List<DonemItem> Donemler { get; set; } = new();

    // Filtre parametreleri
    public string? Arama { get; set; }
    public int? DurumFiltre { get; set; }
    public string? GenelSonucFiltre { get; set; }
    public int? MinPuan { get; set; }
    public int? MaxPuan { get; set; }
    public string? GorevFiltre { get; set; }
    public string? ProjeFiltre { get; set; }

    public int DegerlendirilenSayisi { get; set; }
    public int ToplamPersonelSayisi { get; set; }
    public int KalibrasyondakiSayisi { get; set; }

    public List<Row> Rows { get; set; } = new();
    
    
    
    // Filtre i�in se�enekler
    public List<string> GenelSonucListesi { get; set; } = new()
    {
        "Beklenen Alt�",
        "Beklenen",
        "Beklenen �st�"
    };
    public List<string> GorevListesi { get; set; } = new();
    public List<string> ProjeListesi { get; set; } = new();

    public class DonemItem
    {
        public int DonemId { get; set; }
        public string Ad { get; set; } = "";
    }

    public class Row
    {
        public int PersonelId { get; set; }
        public string SicilNo { get; set; } = "";
        public string PersonelAd { get; set; } = "";

        public string Gorev { get; set; } = "";
        public string ProjeAdi { get; set; } = "";
        public string Mudurluk { get; set; } = "";

        public string Yonetici1Ad { get; set; } = "";
        public string Yonetici2Ad { get; set; } = "";
        public string NihaiYoneticiAd { get; set; } = "";

        public int? ToplamPuan { get; set; }
        public string? GenelSonuc { get; set; }

        public int Durum { get; set; }
        public string DurumMetni { get; set; } = "";
        public string DurumCssClass { get; set; } = "";

        public int? DegerlendirmeId { get; set; }
        
        // Kalibrasyon a�amas�nda m�?
        public bool IsKalibrasyon => Durum == 4;
    }
}
