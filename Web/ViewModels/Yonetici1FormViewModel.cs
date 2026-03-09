namespace PerformansSitesi.Web.ViewModels;

public class Yonetici1FormViewModel
{
    public int DonemId { get; set; }
    public int PersonelId { get; set; }
    public int DegerlendirmeId { get; set; }

    public string DonemAd { get; set; } = "";
    public string PersonelAd { get; set; } = "";
    public string SicilNo { get; set; } = "";

    public string Gorev { get; set; } = "";
    public string ProjeAdi { get; set; } = "";
    public string Mudurluk { get; set; } = "";

    public int? ToplamPuan { get; set; }
    public string GenelSonuc { get; set; } = "";

    public bool DuzenlenebilirMi { get; set; }
    public string RehberHtml { get; set; } = "";

    public string? Yonetici1Notu { get; set; }
    public string? Yonetici2Notu { get; set; }
    public string? NihaiYoneticiNotu { get; set; }

    public string? GucluYonler { get; set; }
    public string? GelisimeAcikYonler { get; set; }
    public string? GelisimOnerileri { get; set; }

    public List<SoruRow> Sorular { get; set; } = new();

    public class SoruRow
    {
        public int DetayId { get; set; }
        public int SoruId { get; set; }
        public int SiraNo { get; set; }
        public bool ZorunluMu { get; set; }

        public string Baslik { get; set; } = "";
        public string TemizSoruMetni { get; set; } = "";

        public int? Yonetici1Puan { get; set; } // 1/2/3
        public string? Yonetici1Yorum { get; set; }

        public string? Yonetici2Yorum { get; set; }
        public string? NihaiYoneticiYorum { get; set; }
    }
}
