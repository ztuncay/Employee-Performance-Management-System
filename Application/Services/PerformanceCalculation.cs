namespace PerformansSitesi.Application.Services;

public static class PerformanceCalculation
{
    public static int? MapToPoint(int? secim)
    {
        if (!secim.HasValue) return null;
        return secim.Value switch
        {
            1 => 3,
            2 => 7,
            3 => 11,
            _ => null
        };
    }

    /// <summary>
    /// Puan aralıkları:
    /// - Beklenen Altı: 33-76 (%15)
    /// - Beklenen: 77-110 (%70)
    /// - Beklenen Üstü: 111-121 (%15)
    /// </summary>
    public static (int? toplam, string genelSonuc) CalculateTotal(IEnumerable<int?> secimler)
    {
        var pts = secimler
            .Select(MapToPoint)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();

        if (pts.Count == 0)
            return (null, "");

        var toplam = pts.Sum();

        var sonuc =
            toplam <= 76 ? "Beklenen Altı" :
            toplam <= 110 ? "Beklenen" :
            "Beklenen Üstü";

        return (toplam, sonuc);
    }
}
