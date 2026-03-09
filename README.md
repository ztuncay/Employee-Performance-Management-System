# PerformansSitesi Performans Degerlendirme Sistemi

**Versiyon:** 2.0  
**Framework:** .NET 8.0  
**Platform:** ASP.NET Core MVC/Razor Pages  
**Son Guncelleme:** Ocak 2026

---

## GENEL BAKIS

PerformansSitesi icin gelistirilmis kapsamli, cok asamali performans degerlendirme sistemi.

### TEMEL OZELLIKLER

- **Cok Asamali Degerlendirme:** Yonetici 1 → Yonetici 2 → Nihai Yonetici → IK
- **Otomatik Puan Hesaplama:** Gorev ve yetkinlik bazli puanlama
- **Excel Ice/Disa Aktarma:** Toplu kullanici ve personel islemleri
- **Rol Bazli Yetkilendirme:** 6 farkli kullanici rolu
- **Tema Yonetimi:** Ozellestirilebilir gorunum ve logo
- **Kapsamli Raporlama:** Detayli ve ozet Excel raporlari
- **Guvenlik:** Audit log, sifre politikalari, KVKK uyumu
- **Donem Yonetimi:** Esnek donem tanimlama ve gecisler

---

## TEKNOLOJI STACK

### Backend (115 C# Dosyasi)
- **.NET 8.0** - Ana framework
- **ASP.NET Core MVC/Razor Pages** - Web framework
- **Entity Framework Core** - ORM
- **SQL Server** - Veritabani

### Frontend (58 Razor Views + 75 JavaScript)
- **Bootstrap 5** - UI Framework
- **jQuery** - JavaScript kutuphanesi
- **Marked.js** - Markdown parser (CDN)
- **Bootstrap Icons** - Icon seti
- **Custom CSS** - Ozellesmis stiller

### Guvenlik
- Cookie Authentication
- HTTPS/SSL zorunlu
- HSTS aktif
- Anti-forgery tokens
- Rate limiting
- Audit logging

---

## SCRIPT POLITIKASI

**ONEMLI:** Bu proje script kullanmaz!

###   KULLANILMAYAN TEKNOLOJILER
-   PowerShell (.ps1) - 0 dosya
-   Bash (.sh) - 0 dosya
-   Python (.py) - 0 dosya (YASAKLANMIS)
-   SQL Scripts (.sql) - 0 dosya

###   ALTERNATIF YONTEMLER
- **Deployment:** Visual Studio Publish
- **Database Migration:** Entity Framework Core Tools
- **Yapılandirma:** appsettings.json

---

## PROJE YAPISI

```
PerformansSitesi/
├── Controllers/          # MVC Controllers (17 dosya)
├── Views/                # Razor Views (58 dosya)
├── Application/          # Services & Business Logic
│   └── Services/         # KilavuzService, ThemeService vb.
├── Domain/              # Entities & Enums
│   ├── Entities/        # Degerlendirme, Personel, Kullanici vb.
│   └── Enums/           # Rol, DegerlendirmeDurumu vb.
├── Infrastructure/      # Data Access
│   ├── Data/            # DbContext
│   ├── Interceptors/    # SaveChanges interceptors
│   └── Seed/            # Varsayilan data
├── Web/                 # ViewModels & Helpers
│   ├── ViewModels/
│   ├── Helpers/
│   └── Filters/         # Action filters
├── wwwroot/             # Static files (CSS, JS, images)
├── Docs/                # Dokumantasyon (25 MD dosyasi)
└── Properties/          # Publish Profiles
    └── PublishProfiles/ # IIS, FolderProfile
```

---

## HIZLI BASLANGIC

### On Gereksinimler
- **Visual Studio 2022** veya ustü
- **.NET 8 SDK**
- **SQL Server 2019+**

### 1. Projeyi Ac
```bash
# Projeyi Visual Studio ile ac
PerformansSitesi.sln
```

### 2. Veritabani Olustur

Visual Studio **Package Manager Console**:
```powershell
Update-Database
```

Veya **dotnet CLI**:
```bash
dotnet ef database update
```

### 3. Calistir
- Visual Studio'da **F5** basin
- Veya:
```bash
dotnet run
```

### 4. Giris Yap

Varsayilan kullanicilar (otomatik olusturulur):
- **admin** / **Admin123!** (Admin rolu)
- **ik** / **Ik123!** (IK rolu)

---

## DEPLOYMENT (PRODUCTION)

### Visual Studio ile IIS'e Deploy

1. **Solution Explorer**'da projeye sag tikla
2. **Publish** sec
3. **IISProfile** publish profile'i sec
4. **Publish** dugmesine tikla

### Detayli Deployment Kilavuzu

Bkz: `Docs/DEPLOYMENT_GUIDE_VS.md`

### Onemli Notlar

- **appsettings.Production.json** manuel yapilandirilmali
- **SSL sertifikasi** gerekli (Let's Encrypt veya ticari)
- **IIS Application Pool:** .NET CLR version = "No Managed Code"
- **Environment Variable:** ASPNETCORE_ENVIRONMENT = Production

---

## ROLLER VE YETKILER

| Rol | Yetki | Gorevler |
|-----|-------|----------|
| **SistemAdmin** | ★★★★★ | Tum sistem yonetimi, tema, database |
| **Admin** | ★★★★☆ | Kullanici, personel, donem, soru yonetimi |
| **IK** | ★★★☆☆ | Surec yonetimi, kalibrasyon, raporlama |
| **NihaiYonetici** | ★★☆☆☆ | Son onay, stratejik degerlendirme |
| **Yonetici2** | ★★☆☆☆ | Ikinci onay, kalibrasyon |
| **Yonetici1** | ★☆☆☆☆ | Ilk degerlendirme, personel puanlama |

---

## DOKUMANTASYON

### Teknik Dokumantasyon
- **Deployment Kilavuzu:** `Docs/DEPLOYMENT_GUIDE_VS.md`
- **Guvenlik Raporu:** `Docs/SECURITY_GUIDE.md`
- **Migration Kilavuzu:** `Docs/EF_MIGRATION_GUIDE.md`
- **Production Config:** `Docs/PRODUCTION_CONFIG_GUIDE.md`

### Kullanim Kilavuzlari (Rol Bazli)
- **Genel:** `Docs/KULLANIM_KILAVUZU.md`
- **Admin:** `Docs/ADMIN_KILAVUZU.md`
- **Sistem Admin:** `Docs/SISTEM_ADMIN_KILAVUZU.md`
- **IK:** `Docs/IK_KILAVUZU.md`
- **Yonetici 1:** `Docs/YONETICI1_KILAVUZU.md`
- **Yonetici 2:** `Docs/YONETICI2_KILAVUZU.md`
- **Nihai Yonetici:** `Docs/NIHAI_YONETICI_KILAVUZU.md`

---

## KULLANIM KILAVUZU SAYFASI

Sistem icinde **dinamik kilavuz sayfasi** vardir:
- Ana menueden: **"Kullanim Kilavuzu"** butonuna tikla
- Kullanici dropdown'indan: **"Kullanim Kilavuzu"** secenegine tikla
- **Otomatik olarak** kullanicinin rolune gore ilgili kilavuzu gosterir

**Teknik Detay:**
- **Controller:** `Controllers/KilavuzController.cs`
- **Service:** `Application/Services/KilavuzService.cs`
- **View:** `Views/Kilavuz/Index.cshtml`
- **Markdown Parser:** Marked.js (client-side rendering)

---

## ISTATISTIKLER

### Kod Tabanı
- **C# Dosyalari:** 115
- **Razor Views:** 58
- **JavaScript:** 75 (cogu Bootstrap kutuphaneleri)
- **Markdown:** 25 (dokumantasyon)

### Kullanilmayan
- **PowerShell Scripts:** 0
- **Bash Scripts:** 0
- **Python Scripts:** 0
- **SQL Scripts:** 0

**Not:** Tum islemler .NET ve Visual Studio ile yapilir.

---

## VERITABANI SEMASI

### Ana Tablolar
- **Kullanicilar** - Sistem kullanicilari
- **Personeller** - Degerlendirilen calisanlar
- **Donemler** - Degerlendirme donemleri
- **PerformansSorular** - Degerlendirme sorulari
- **Degerlendirmeler** - Performans degerlendirmeleri
- **DegerlendirmeCevaplar** - Soru bazli cevaplar
- **SiteTema** - Tema ayarlari
- **AuditLogs** - Islem kayitlari

---

## GUVENLIK

### Uygulanan Guvenlik Tedbirleri
-   HTTPS zorunlu
-   Cookie authentication (30dk timeout)
-   Anti-forgery tokens
-   Rate limiting (100 req/dk)
-   Audit logging
-   Password hashing (bcrypt)
-   HSTS aktif
-   XSS protection headers
-   CSRF protection

### KVKK Uyumu
- Kullanici onaylari
- Veri saklama politikalari
- Erisim loglari
- Gizlilik politikasi modali

---

## PERFORMANS

### Optimizasyonlar
- EF Core AsNoTracking queries
- Memory cache kullanimi
- Lazy loading devre disi
- Index'ler veritabaninda tanimli
- Static file caching

---

## GELISTIRME

### Branch Stratejisi
- **main** - Production
- **develop** - Gelistirme
- **feature/** - Yeni ozellikler

### Commit Mesaj Formati
```
[Tip]: Kisa aciklama

Detayli aciklama (opsiyonel)
```

**Tipler:** feat, fix, docs, style, refactor, test, chore

---

**Gelistirici:** Zeynep Tuncay

**SON NOT:** Bu proje **tamamen .NET/C# stack** uzerine kurulu, **script kullanmayan**, modern bir ASP.NET Core web uygulamasidir. Tum islemler Visual Studio ve Entity Framework Tools ile yapilir.

