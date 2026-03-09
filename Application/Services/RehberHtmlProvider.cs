namespace PerformansSitesi.Application.Services;

public static class RehberHtmlProvider
{
    public static string BuildRehberHtml()
    {
        return @"
<h3>Performans Değerlendirme Rehberi</h3>

<h4>Görev ve Sorumluluk Başarı Skalası</h4>
<table style='width: 100%; border-collapse: collapse;'>
    <tr style='border-bottom: 1px solid #ddd;'>
        <td style='padding: 8px; width: 20%;'><strong>Beklenen Altı</strong></td>
        <td style='padding: 8px;'>Kişi kendisine verilen görev ve sorumlulukların çoğunu büyük ölçüde yerine getirememektedir. Gelişim alanları belirgin olup, destekleyici eğitim ve mentorluk gereklidir.</td>
    </tr>
    <tr style='border-bottom: 1px solid #ddd;'>
        <td style='padding: 8px; width: 20%;'><strong>Beklenen</strong></td>
        <td style='padding: 8px;'>Kişi kendisine verilen görev ve sorumlulukları istenilen düzeyde, zamanında ve kaliteli bir şekilde yerine getirmektedir. Ekip hedeflerine katkı sağlamaktadır.</td>
    </tr>
    <tr style='border-bottom: 1px solid #ddd;'>
        <td style='padding: 8px; width: 20%;'><strong>Beklenen Üstü</strong></td>
        <td style='padding: 8px;'>Kişi kendisine verilen görev ve sorumlulukları tamamıyla, istenilen düzeyin çok üzerinde yerine getirmektedir. Proaktif yaklaşım sergiler ve örnek teşkil eder.</td>
    </tr>
</table>

<br>

<h4>Yetkinlik Başarı Skalası</h4>
<table style='width: 100%; border-collapse: collapse;'>
    <tr style='border-bottom: 1px solid #ddd;'>
        <td style='padding: 8px; width: 20%;'><strong>Beklenen Altı</strong></td>
        <td style='padding: 8px;'>Kişi bu davranış için beklenenlerin çoğunu büyük ölçüde yerine getirememektedir. Yetkinlik gelişimi için iyileştirme planı oluşturulmalıdır.</td>
    </tr>
    <tr style='border-bottom: 1px solid #ddd;'>
        <td style='padding: 8px; width: 20%;'><strong>Beklenen</strong></td>
        <td style='padding: 8px;'>Kişi bu davranış için beklenenleri, istenilen düzeyde yerine getirmektedir. Rolü için gerekli yetkinliklere sahiptir ve bunları etkili kullanır.</td>
    </tr>
    <tr style='border-bottom: 1px solid #ddd;'>
        <td style='padding: 8px; width: 20%;'><strong>Beklenen Üstü</strong></td>
        <td style='padding: 8px;'>Kişi bu davranış için beklenenleri tamamıyla, istenilen düzeyin çok üzerinde yerine getirmektedir. Yetkinliklerini stratejik bir şekilde kullanarak katma değer yaratır.</td>
    </tr>
</table>

<br>

<h4>Kalibrasyon Aşaması</h4>
<p>İK ve yöneticiler tarafından tüm değerlendirmeler incelenip, hedef dağılımla uyumluluğu kontrol edilmektedir.</p>

<h4>Hedef Dağılım</h4>
<ul>
    <li><strong>Beklenen Altı:</strong> %15</li>
    <li><strong>Beklenen:</strong> %70</li>
    <li><strong>Beklenen Üstü:</strong> %15</li>
</ul>

<hr style='margin-top: 15px;'>

<p style='font-size: 12px; color: #666;'>
<strong>Not:</strong> Hedef dağılım, tüm değerlendirmeler arasında objektif bir dengeyi sağlamak için tasarlanmıştır. 
Yöneticilerin bu dağılıma mümkün olduğunca uyması tavsiye edilir.
</p>
";
    }
}
