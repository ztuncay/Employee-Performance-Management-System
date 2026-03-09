namespace PerformansSitesi.Web.ViewModels;

public class IKFormViewModel
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

    public string Yonetici1Ad { get; set; } = "";
    public string Yonetici2Ad { get; set; } = "";
    public string NihaiYoneticiAd { get; set; } = "";

    public int? ToplamPuan { get; set; }
    public string GenelSonuc { get; set; } = "";

    public int Durum { get; set; }
    public string DurumMetni { get; set; } = "";
    public bool KalibrasyondaMi { get; set; }

    public string? Yonetici1Notu { get; set; }
    public string? Yonetici2Notu { get; set; }
    public string? NihaiYoneticiNotu { get; set; }

    public string? GucluYonler { get; set; }
    public string? GelisimeAcikYonler { get; set; }
    public string? GelisimOnerileri { get; set; }

    public string RehberHtml { get; set; } = "";

    public List<SoruRow> Sorular { get; set; } = new();

    public class SoruRow
    {
        public int SiraNo { get; set; }
        public bool ZorunluMu { get; set; }
        public string Baslik { get; set; } = "";
        public string TemizSoruMetni { get; set; } = "";

        public int? Yonetici1Puan { get; set; }
        public string? Yonetici1Yorum { get; set; }
    }
}
