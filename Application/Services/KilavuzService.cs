namespace PerformansSitesi.Application.Services;

/// <summary>
/// Rol bazlı kullanım kılavuzu HTML içeriği sağlar
/// </summary>
public class KilavuzService
{
    public string GetKilavuzHtmlByRole(string role)
    {
        return role switch
        {
            "Admin" => GetAdminKilavuz(),
            "IK" => GetIKKilavuz(),
            "Yonetici1" => GetYonetici1Kilavuz(),
            "Yonetici2" => GetYonetici2Kilavuz(),
            "NihaiYonetici" => GetNihaiYoneticiKilavuz(),
            _ => GetGenelKilavuz()
        };
    }

    private string GetAdminKilavuz()
    {
        return @"
<div class='kilavuz-content'>
    <h2>Admin Kullanım Kılavuzu</h2>
    
    <h3>Sistem Yönetimi</h3>
    <p>Admin olarak sistemin tüm yönetim fonksiyonlarına erişiminiz bulunmaktadır.</p>
    
    <h4>Kullanıcı Yönetimi</h4>
    <ul>
        <li>Yeni kullanıcı ekleme, düzenleme ve silme</li>
        <li>Kullanıcı rollerini yönetme</li>
        <li>Excel ile toplu kullanıcı yükleme</li>
        <li>Kullanıcı şifrelerini sıfırlama</li>
    </ul>
    
    <h4>Personel Yönetimi</h4>
    <ul>
        <li>Personel bilgilerini ekleme ve güncelleme</li>
        <li>Excel ile toplu personel import</li>
        <li>Personel aktif/pasif durumlarını yönetme</li>
        <li>Yönetici atamalarını yapılandırma</li>
    </ul>
    
    <h4>Dönem Yönetimi</h4>
    <ul>
        <li>Yeni değerlendirme dönemi oluşturma</li>
        <li>Dönem başlangıç ve bitiş tarihlerini ayarlama</li>
        <li>Aktif dönemi belirleme</li>
    </ul>
    
    <h4>Performans Soruları</h4>
    <ul>
        <li>Değerlendirme sorularını oluşturma ve düzenleme</li>
        <li>Soru kategorilerini (Görev/Yetkinlik) yönetme</li>
        <li>Soruları aktif/pasif yapma</li>
    </ul>
    
    <h4>Tema Yönetimi</h4>
    <ul>
        <li>Site renklerini özelleştirme</li>
        <li>Logo ve başlık ayarları</li>
        <li>Varsayılan tema oluşturma</li>
    </ul>
    
    <h4>Raporlar ve Denetim</h4>
    <ul>
        <li>Sistem loglarını görüntüleme</li>
        <li>Kullanıcı aktivitelerini izleme</li>
        <li>Performans raporlarını inceleme</li>
        <li>Değişiklik geçmişini takip etme</li>
    </ul>
</div>
";
    }

    private string GetIKKilavuz()
    {
        return @"
<div class='kilavuz-content'>
    <h2>İK Kullanım Kılavuzu</h2>
    
    <h3>İK Değerlendirme Süreci</h3>
    <p>İK olarak çalışan performans değerlendirmelerini gözden geçirme ve kalibrasyon yapma yetkisine sahipsiniz.</p>
    
    <h4>Değerlendirmeleri İnceleme</h4>
    <ul>
        <li>Tüm personel değerlendirmelerini görüntüleme</li>
        <li>Değerlendirme aşamalarını takip etme</li>
        <li>Yönetici değerlendirmelerini kontrol etme</li>
    </ul>
    
    <h4>Kalibrasyon İşlemleri</h4>
    <ul>
        <li>Nihai değerlendirmeleri onaylama veya geri gönderme</li>
        <li>Değerlendirme notlarını kalibrasyon için düzenleme</li>
        <li>Hedef dağılımı ile uyumluluğu kontrol etme</li>
    </ul>
    
    <h4>Hedef Dağılım</h4>
    <ul>
        <li><strong>Beklenen Altı:</strong> %15</li>
        <li><strong>Beklenen:</strong> %70</li>
        <li><strong>Beklenen Üstü:</strong> %15</li>
    </ul>
    
    <h4>Personel Yönetimi</h4>
    <ul>
        <li>Personel listelerini görüntüleme</li>
        <li>Personel bilgilerini güncelleme</li>
        <li>Excel ile toplu personel import</li>
    </ul>
    
    <h4>Raporlama</h4>
    <ul>
        <li>Değerlendirme raporlarını oluşturma</li>
        <li>İstatistiksel analizler yapma</li>
        <li>Dağılım grafikleri inceleme</li>
    </ul>
</div>
";
    }

    private string GetYonetici1Kilavuz()
    {
        return @"
<div class='kilavuz-content'>
    <h2>1. Yönetici Kullanım Kılavuzu</h2>
    
    <h3>Değerlendirme Süreci</h3>
    <p>Doğrudan sorumluluğunuzdaki çalışanların performans değerlendirmelerini yapmakla sorumlusunuz.</p>
    
    <h4>Değerlendirme Adımları</h4>
    <ol>
        <li><strong>Liste Görüntüleme:</strong> Size atanmış çalışanları görüntüleyin</li>
        <li><strong>Değerlendirme Formu:</strong> Her soru için puanlama yapın</li>
        <li><strong>Yorum Ekleme:</strong> Gerekli açıklamaları yazın</li>
        <li><strong>Kaydetme:</strong> Değerlendirmeyi kaydedin</li>
    </ol>
    
    <h4>Puanlama Skalası</h4>
    <table style='width: 100%; border-collapse: collapse; margin: 15px 0;'>
        <tr style='border-bottom: 2px solid #ddd; background-color: #f5f5f5;'>
            <th style='padding: 10px; text-align: left;'>Seviye</th>
            <th style='padding: 10px; text-align: left;'>Açıklama</th>
        </tr>
        <tr style='border-bottom: 1px solid #ddd;'>
            <td style='padding: 10px;'><strong>Beklenen Altı (1)</strong></td>
            <td style='padding: 10px;'>Görev ve sorumlulukların çoğunu yerine getirememektedir</td>
        </tr>
        <tr style='border-bottom: 1px solid #ddd;'>
            <td style='padding: 10px;'><strong>Beklenen (2)</strong></td>
            <td style='padding: 10px;'>Görev ve sorumlulukları istenilen düzeyde yerine getirmektedir</td>
        </tr>
        <tr>
            <td style='padding: 10px;'><strong>Beklenen Üstü (3)</strong></td>
            <td style='padding: 10px;'>Görev ve sorumlulukları istenilen düzeyin çok üzerinde yerine getirmektedir</td>
        </tr>
    </table>
    
    <h4>Görev ve Yetkinlik Soruları</h4>
    <ul>
        <li><strong>Görev Soruları:</strong> Kişinin işini ne kadar iyi yaptığını değerlendirir</li>
        <li><strong>Yetkinlik Soruları:</strong> Kişinin davranışsal özelliklerini değerlendirir</li>
    </ul>
    
    <h4>Önemli Notlar</h4>
    <ul>
        <li>Değerlendirmeleriniz objektif ve adil olmalıdır</li>
        <li>Hedef dağılıma mümkün olduğunca uyulmalıdır</li>
        <li>Her değerlendirme için detaylı yorum yazılmalıdır</li>
        <li>Değerlendirmeler süreç içinde üst yönetime iletilecektir</li>
    </ul>
</div>
";
    }

    private string GetYonetici2Kilavuz()
    {
        return @"
<div class='kilavuz-content'>
    <h2>2. Yönetici Kullanım Kılavuzu</h2>
    
    <h3>Değerlendirme Süreci</h3>
    <p>1. Yöneticinin yaptığı değerlendirmeleri gözden geçirip onaylama veya düzeltme yapma yetkisine sahipsiniz.</p>
    
    <h4>Görevleriniz</h4>
    <ol>
        <li><strong>İnceleme:</strong> 1. Yönetici değerlendirmelerini kontrol edin</li>
        <li><strong>Değerlendirme:</strong> Kendi görüşlerinizi ekleyin</li>
        <li><strong>Kalibrasyon:</strong> Değerlendirmeleri dengeleyerek adil dağılım sağlayın</li>
        <li><strong>Onay:</strong> Değerlendirmeleri onaylayıp bir üst seviyeye gönderin</li>
    </ol>
    
    <h4>Puanlama Rehberi</h4>
    <table style='width: 100%; border-collapse: collapse; margin: 15px 0;'>
        <tr style='border-bottom: 2px solid #ddd; background-color: #f5f5f5;'>
            <th style='padding: 10px; text-align: left;'>Seviye</th>
            <th style='padding: 10px; text-align: left;'>Açıklama</th>
        </tr>
        <tr style='border-bottom: 1px solid #ddd;'>
            <td style='padding: 10px;'><strong>Beklenen Altı (1)</strong></td>
            <td style='padding: 10px;'>Yetkinlik gelişimi için iyileştirme planı oluşturulmalıdır</td>
        </tr>
        <tr style='border-bottom: 1px solid #ddd;'>
            <td style='padding: 10px;'><strong>Beklenen (2)</strong></td>
            <td style='padding: 10px;'>Rolü için gerekli yetkinliklere sahip ve bunları etkili kullanır</td>
        </tr>
        <tr>
            <td style='padding: 10px;'><strong>Beklenen Üstü (3)</strong></td>
            <td style='padding: 10px;'>Yetkinliklerini stratejik bir şekilde kullanarak katma değer yaratır</td>
        </tr>
    </table>
    
    <h4>Hedef Dağılım</h4>
    <p>Değerlendirmelerinizde aşağıdaki dağılımı hedefleyin:</p>
    <ul>
        <li><strong>Beklenen Altı:</strong> %15</li>
        <li><strong>Beklenen:</strong> %70</li>
        <li><strong>Beklenen Üstü:</strong> %15</li>
    </ul>
    
    <h4>İpuçları</h4>
    <ul>
        <li>1. Yönetici değerlendirmelerini dikkatlice okuyun</li>
        <li>Farklı görüş varsa açıklayıcı yorum yazın</li>
        <li>Ekip içinde adil bir dağılım sağlayın</li>
        <li>Değerlendirmelerinizi verilerle destekleyin</li>
    </ul>
</div>
";
    }

    private string GetNihaiYoneticiKilavuz()
    {
        return @"
<div class='kilavuz-content'>
    <h2>Nihai Yönetici Kullanım Kılavuzu</h2>
    
    <h3>Nihai Onay Süreci</h3>
    <p>Tüm değerlendirme sürecinin son aşamasındasınız. Değerlendirmeleri son kez gözden geçirip nihai onay verme yetkisine sahipsiniz.</p>
    
    <h4>Sorumluluklarınız</h4>
    <ol>
        <li><strong>Kapsamlı İnceleme:</strong> Alt yönetici değerlendirmelerini detaylı inceleyin</li>
        <li><strong>Stratejik Değerlendirme:</strong> Organizasyonel hedeflerle uyumu kontrol edin</li>
        <li><strong>Nihai Karar:</strong> Değerlendirmeleri onaylayın veya düzeltme talep edin</li>
        <li><strong>Kalibrasyon:</strong> Genel dağılımın hedef oranlarla uyumunu sağlayın</li>
    </ol>
    
    <h4>Puanlama Sistemi</h4>
    <table style='width: 100%; border-collapse: collapse; margin: 15px 0;'>
        <tr style='border-bottom: 2px solid #ddd; background-color: #f5f5f5;'>
            <th style='padding: 10px; text-align: left;'>Seviye</th>
            <th style='padding: 10px; text-align: left;'>Hedef %</th>
            <th style='padding: 10px; text-align: left;'>Açıklama</th>
        </tr>
        <tr style='border-bottom: 1px solid #ddd;'>
            <td style='padding: 10px;'><strong>Beklenen Altı (1)</strong></td>
            <td style='padding: 10px;'>%15</td>
            <td style='padding: 10px;'>Gelişim planı gerektiren çalışanlar</td>
        </tr>
        <tr style='border-bottom: 1px solid #ddd;'>
            <td style='padding: 10px;'><strong>Beklenen (2)</strong></td>
            <td style='padding: 10px;'>%70</td>
            <td style='padding: 10px;'>Standart performans gösteren çalışanlar</td>
        </tr>
        <tr>
            <td style='padding: 10px;'><strong>Beklenen Üstü (3)</strong></td>
            <td style='padding: 10px;'>%15</td>
            <td style='padding: 10px;'>Üstün performans gösteren çalışanlar</td>
        </tr>
    </table>
    
    <h4>Kalibrasyon Kontrolleri</h4>
    <ul>
        <li>Genel dağılımı hedef oranlarla karşılaştırın</li>
        <li>Departmanlar arası dengeyi gözlemleyin</li>
        <li>Aşırı pozitif veya negatif değerlendirmeleri inceleyin</li>
        <li>Objektifliği ve adil değerlendirmeyi sağlayın</li>
    </ul>
    
    <h4>Karar Verme Kriterleri</h4>
    <ul>
        <li><strong>Onaylama:</strong> Değerlendirme objektif ve dengeli ise</li>
        <li><strong>Düzeltme İsteği:</strong> Ek açıklama veya revizyon gerekiyorsa</li>
        <li><strong>Kalibrasyon:</strong> Hedef dağılıma uymak için ayarlama yapılması gerekiyorsa</li>
    </ul>
    
    <h4>Raporlama</h4>
    <p>Nihai onay sonrasında:</p>
    <ul>
        <li>Değerlendirme sonuçları İK'ya iletilir</li>
        <li>Genel performans raporları oluşturulur</li>
        <li>Gelişim planları hazırlanır</li>
        <li>Ödüllendirme ve terfi süreçleri başlatılabilir</li>
    </ul>
</div>
";
    }

    private string GetGenelKilavuz()
    {
        return @"
<div class='kilavuz-content'>
    <h2>Performans Değerlendirme Sistemi Kullanım Kılavuzu</h2>
    
    <h3>Sistem Hakkında</h3>
    <p>Bu sistem, çalışan performans değerlendirmelerinin adil, objektif ve standart bir şekilde yapılmasını sağlar.</p>
    
    <h4>Genel Süreç</h4>
    <ol>
        <li><strong>1. Yönetici Değerlendirmesi:</strong> Doğrudan yönetici değerlendirme yapar</li>
        <li><strong>2. Yönetici Onayı:</strong> İkinci seviye yönetici kontrol ve onay yapar</li>
        <li><strong>Nihai Yönetici Onayı:</strong> Üst düzey yönetici nihai onayı verir</li>
        <li><strong>İK Kalibrasyonu:</strong> İK departmanı kalibrasyon ve son kontrolü yapar</li>
    </ol>
    
    <h4>Değerlendirme Kriterleri</h4>
    <ul>
        <li><strong>Görev ve Sorumluluklar:</strong> İşin ne kadar iyi yapıldığını ölçer</li>
        <li><strong>Yetkinlikler:</strong> Davranışsal ve teknik becerileri değerlendirir</li>
    </ul>
    
    <h4>Başarı Skalası</h4>
    <table style='width: 100%; border-collapse: collapse; margin: 15px 0;'>
        <tr style='border-bottom: 2px solid #ddd; background-color: #f5f5f5;'>
            <th style='padding: 10px; text-align: left; width: 30%;'>Seviye</th>
            <th style='padding: 10px; text-align: left;'>Tanım</th>
        </tr>
        <tr style='border-bottom: 1px solid #ddd;'>
            <td style='padding: 10px;'><strong>Beklenen Altı</strong></td>
            <td style='padding: 10px;'>Performans hedeflerin altında, gelişim alanları belirgin</td>
        </tr>
        <tr style='border-bottom: 1px solid #ddd;'>
            <td style='padding: 10px;'><strong>Beklenen</strong></td>
            <td style='padding: 10px;'>Beklentileri karşılayan, standart performans</td>
        </tr>
        <tr>
            <td style='padding: 10px;'><strong>Beklenen Üstü</strong></td>
            <td style='padding: 10px;'>Beklentilerin üzerinde, mükemmel performans</td>
        </tr>
    </table>
    
    <h4>Hedef Dağılım</h4>
    <p>Adil ve dengeli değerlendirme için hedef dağılım:</p>
    <ul>
        <li>%15 - Beklenen Altı</li>
        <li>%70 - Beklenen</li>
        <li>%15 - Beklenen Üstü</li>
    </ul>
    
    <h4>Sistem Kullanımı</h4>
    <ul>
        <li>Rolünüze özel menülerden ilgili bölümlere erişebilirsiniz</li>
        <li>Değerlendirmeler dönem bazlı yapılır</li>
        <li>Her aşamada detaylı yorum ve açıklama yazılması önemlidir</li>
        <li>Değerlendirmeler gizli tutulur ve sadece yetkili kişiler görebilir</li>
    </ul>
    
    <h4>Destek</h4>
    <p>Sorularınız için İK departmanı ile iletişime geçebilirsiniz.</p>
</div>

<style>
.kilavuz-content {
    padding: 20px;
    line-height: 1.6;
}

.kilavuz-content h2 {
    color: #2c3e50;
    border-bottom: 3px solid #3498db;
    padding-bottom: 10px;
    margin-bottom: 20px;
}

.kilavuz-content h3 {
    color: #34495e;
    margin-top: 25px;
    margin-bottom: 15px;
}

.kilavuz-content h4 {
    color: #555;
    margin-top: 20px;
    margin-bottom: 10px;
}

.kilavuz-content ul, .kilavuz-content ol {
    margin-left: 20px;
    margin-bottom: 15px;
}

.kilavuz-content li {
    margin-bottom: 8px;
}

.kilavuz-content table {
    background-color: white;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.kilavuz-content p {
    margin-bottom: 12px;
    color: #555;
}
</style>
";
    }
}
