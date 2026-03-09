namespace PerformansSitesi.Application.Services;

public static class DurumHelper
{
    public static string DurumMetni(int durum) => durum switch
    {
        0 => "Değerlendirme Yok",
        1 => "1. Yönetici Aşamasında",
        2 => "2. Yönetici Aşamasında",
        3 => "Nihai Yönetici Aşamasında",
        4 => "İK / Kalibrasyon Aşamasında",
        5 => "Tamamlandı",
        6 => "1. Yöneticiye İade",
        7 => "2. Yöneticiye İade",
        8 => "Nihai Yöneticiye İade",
        _ => "Bilinmiyor"
    };

    public static string DurumCssClass(int durum) => durum switch
    {
        0 => "primary",           // Değerlendirme Yok - Mavi
        1 => "danger",            // 1. Yönetici Aşamasında - Kırmızı (Bekleyen)
        2 => "danger",            // 2. Yönetici Aşamasında - Kırmızı (Bekleyen)
        3 => "danger",            // Nihai Yönetici Aşamasında - Kırmızı (Bekleyen)
        4 => "dark",              // İK / Kalibrasyon Aşamasında - Siyah
        5 => "success",           // Tamamlandı - Yeşil
        6 => "danger",            // 1. Yöneticiye İade - Kırmızı
        7 => "danger",            // 2. Yöneticiye İade - Kırmızı
        8 => "danger",            // Nihai Yöneticiye İade - Kırmızı
        _ => "primary"            // Bilinmiyor - Mavi
    };
}
