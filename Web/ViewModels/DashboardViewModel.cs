namespace PerformansSitesi.Web.ViewModels;

public class DashboardViewModel
{
    public int DonemId { get; set; }
    public string DonemAd { get; set; } = "";

    // Genel �statistikler (T�m Personel - Admin/IK i�in)
    public int ToplamPersonel { get; set; }
    public int DegerlendirmeBaslayan { get; set; }
    public int Kalibrasyonda { get; set; }
    public int Tamamlanan { get; set; }

    // Kullan�c�n�n Ba�l� Personeline G�re �statistikler
    public int BagliPersonelSayisi { get; set; }
    public int BagliDegerlendirmeBaslayan { get; set; }
    public int BagliKalibrasyonda { get; set; }
    public int BagliTamamlanan { get; set; }

    // Rol Bilgisi
    public string UserRole { get; set; } = "";
    public string UserName { get; set; } = "";

    public DistributionBlock Distribution { get; set; } = new();
    public DistributionBlock BagliDistribution { get; set; } = new();

    // De�erlendirme Durumuna G�re Detay
    public List<EvaluationStatistic> EvaluationStatistics { get; set; } = new();

    public class DistributionBlock
    {
        public int BeklenenAlti { get; set; }
        public int Beklenen { get; set; }
        public int BeklenenUstu { get; set; }

        public double BeklenenAltiPct { get; set; }
        public double BeklenenPct { get; set; }
        public double BeklenenUstuPct { get; set; }

        public double TargetAltiPct { get; set; } = 15;
        public double TargetPct { get; set; } = 70;
        public double TargetUstuPct { get; set; } = 15;
    }

    public class EvaluationStatistic
    {
        public string Status { get; set; } = "";
        public int Count { get; set; }
        public double Percentage { get; set; }
        public int TargetPercentage { get; set; }
    }
}
