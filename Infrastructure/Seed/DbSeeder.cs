using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Domain.Enums;
using PerformansSitesi.Infrastructure.Data;

namespace PerformansSitesi.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PerformansDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Kullanici>>();

        await db.Database.MigrateAsync();

        if (!await db.Donemler.AnyAsync())
        {
            db.Donemler.Add(new Donem
            {
                Ad = "2026 Performans Degerlendirmesi",
                BaslangicTarihi = new DateTime(2026, 1, 1),
                BitisTarihi = new DateTime(2026, 1, 31),
                AktifMi = true
            });
            await db.SaveChangesAsync();
        }

        if (!await db.PerformansSorulari.AnyAsync())
        {
            const int sablonId = 1;
            var sorular = new List<PerformansSorusu>
            {
                new() 
                { 
                    SablonId = sablonId, 
                    SiraNo = 1, 
                    ZorunluMu = true,
                    Kategori = "G�REV VE SORUMLULUK DE�ERLEND�RMES�",
                    SoruBaslik = "1. �� Disiplini",
                    SoruMetni = "��yeri kurallar�na uyar, devam durumuna ve �al��ma s�relerine dikkat eder; k�l�k-k�yafet ve davran��lar�nda i�yeri kurallar�na uygun �ekilde hareket eder."
                },
                new() 
                { 
                    SablonId = sablonId, 
                    SiraNo = 2, 
                    ZorunluMu = true,
                    Kategori = "G�REV VE SORUMLULUK DE�ERLEND�RMES�",
                    SoruBaslik = "2. Sorumluluk Bilinci",
                    SoruMetni = "�al��malar�nda �irket yarar�n� g�zeterek �al��ma talimatlar�na uyar; g�revlerini verilen �er�evede, zaman�nda ve aksatmadan yerine getirir."
                },
                new() 
                { 
                    SablonId = sablonId, 
                    SiraNo = 3, 
                    ZorunluMu = true,
                    Kategori = "G�REV VE SORUMLULUK DE�ERLEND�RMES�",
                    SoruBaslik = "3. �� Bilgisi ve Becerisi",
                    SoruMetni = "��in gerektirdi�i bilgi, beceri ve deneyime sahip olup g�revlerini nezarete ihtiya� duymadan yerine getirir."
                },
                new() 
                { 
                    SablonId = sablonId, 
                    SiraNo = 4, 
                    ZorunluMu = true,
                    Kategori = "G�REV VE SORUMLULUK DE�ERLEND�RMES�",
                    SoruBaslik = "4. Verimlilik",
                    SoruMetni = "��iyle ilgili malzeme, makine, te�hizat ve edevat� itinal� ve temiz �ekilde kullan�r, zaman�nda bak�m�n� yapar ve tasarruf kurallar�na uyar."
                },
                new() 
                { 
                    SablonId = sablonId, 
                    SiraNo = 5, 
                    ZorunluMu = true,
                    Kategori = "G�REV VE SORUMLULUK DE�ERLEND�RMES�",
                    SoruBaslik = "5. Kalite Odakl�l�k",
                    SoruMetni = "Verilen i�i zaman�nda ve istenilen kalitede ger�ekle�tirerek kalite standartlar�na uygun �ekilde �al���r."
                },
                new() 
                { 
                    SablonId = sablonId, 
                    SiraNo = 6, 
                    ZorunluMu = true,
                    Kategori = "G�REV VE SORUMLULUK DE�ERLEND�RMES�",
                    SoruBaslik = "6. �� G�venli�i Bilinci",
                    SoruMetni = "�� Sa�l��� ve G�venli�i kurallar�na uyar, koruyucu ekipmanlar� eksiksiz kullan�r; �evre g�venli�i ile i�yeri d�zeni ve temizli�ine �zen g�sterir."
                },

                new() 
                { 
                    SablonId = sablonId, 
                    SiraNo = 7, 
                    ZorunluMu = true,
                    Kategori = "YETK�NL�K DE�ERLEND�RMES�",
                    SoruBaslik = "1. Tak�m �al��mas�",
                    SoruMetni = "Ekip arkada�lar�yla i� birli�i i�inde, uyumlu �al���r; i�iyle ilgili kar��l�kl� g�r�� al��veri�inde bulunur."
                },
                new() 
                { 
                    SablonId = sablonId, 
                    SiraNo = 8, 
                    ZorunluMu = true,
                    Kategori = "YETK�NL�K DE�ERLEND�RMES�",
                    SoruBaslik = "2. �leti�im Becerisi",
                    SoruMetni = "Y�neticilerle ve ekip arkada�lar�yla a��k ve net ileti�im kurar, iyi bir dinleyicidir ve empati geli�tirerek �at��madan uzak durur."
                },
                new() 
                { 
                    SablonId = sablonId, 
                    SiraNo = 9, 
                    ZorunluMu = true,
                    Kategori = "YETK�NL�K DE�ERLEND�RMES�",
                    SoruBaslik = "3. Karar Verme ve Problem ��zme",
                    SoruMetni = "Talimatlar �er�evesinde inisiyatif kullan�r, isabetli kararlar al�r, i�iyle ilgili problemleri etkin �ekilde ��zer ve gerekti�inde y�neticisinden destek ister."
                },
                new() 
                { 
                    SablonId = sablonId, 
                    SiraNo = 10, 
                    ZorunluMu = true,
                    Kategori = "YETK�NL�K DE�ERLEND�RMES�",
                    SoruBaslik = "4. ��renme ve Geli�ime A��kl�k",
                    SoruMetni = "E�itimlere kat�l�r, yeni g�rev ve y�ntemleri ��renmeye a��kt�r; geri bildirimleri dikkate alarak kendini geli�tirir ve hatalar�n� kabul ederek d�zeltmeye �al���r."
                },
                new() 
                { 
                    SablonId = sablonId, 
                    SiraNo = 11, 
                    ZorunluMu = true,
                    Kategori = "YETK�NL�K DE�ERLEND�RMES�",
                    SoruBaslik = "5. Motivasyon",
                    SoruMetni = "��e kar�� olumlu tutum sergiler ve g�revlerini isteyerek yerine getirir."
                }
            };

            db.PerformansSorulari.AddRange(sorular);
            await db.SaveChangesAsync();
        }

        if (!await db.Kullanicilar.AnyAsync())
        {
            var sysAdmin = new Kullanici { AdSoyad = "Sistem Admin", KullaniciAdi = "sysadmin", Email = "sysadmin@firma.com", Rol = Rol.SistemAdmin };
            var y1 = new Kullanici { AdSoyad = "Yonetici 1", KullaniciAdi = "yonetici1", Email = "yonetici1@firma.com", Rol = Rol.Yonetici1 };
            var y2 = new Kullanici { AdSoyad = "Yonetici 2", KullaniciAdi = "yonetici2", Email = "yonetici2@firma.com", Rol = Rol.Yonetici2 };
            var ny = new Kullanici { AdSoyad = "Bolge Muduru", KullaniciAdi = "bolge", Email = "bolge@firma.com", Rol = Rol.NihaiYonetici };
            var ik = new Kullanici { AdSoyad = "IK", KullaniciAdi = "ik", Email = "ik@firma.com", Rol = Rol.IK };
            var admin = new Kullanici { AdSoyad = "Admin", KullaniciAdi = "admin", Email = "admin@firma.com", Rol = Rol.Admin };

            sysAdmin.SifreHash = hasher.HashPassword(sysAdmin, "1234");
            y1.SifreHash = hasher.HashPassword(y1, "1234");
            y2.SifreHash = hasher.HashPassword(y2, "1234");
            ny.SifreHash = hasher.HashPassword(ny, "1234");
            ik.SifreHash = hasher.HashPassword(ik, "1234");
            admin.SifreHash = hasher.HashPassword(admin, "1234");

            db.Kullanicilar.AddRange(sysAdmin, y1, y2, ny, ik, admin);
            await db.SaveChangesAsync();

            var p1 = new Personel
            {
                SicilNo = "S001",
                AdSoyad = "Personel A",
                Gorev = "Operator",
                ProjeAdi = "YEMEKHANE",
                Mudurluk = "Tesis Yonetimi Mudurlugu",
                Yonetici1Id = y1.KullaniciId,
                Yonetici2Id = y2.KullaniciId,
                NihaiYoneticiId = ny.KullaniciId,
                AktifMi = true
            };

            db.Personeller.Add(p1);
            await db.SaveChangesAsync();
        }
        else
        {
            var sysAdminExists = await db.Kullanicilar.AnyAsync(k => k.KullaniciAdi == "sysadmin");
            if (!sysAdminExists)
            {
                var hasher2 = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Kullanici>>();
                var sysAdmin = new Kullanici 
                { 
                    AdSoyad = "Sistem Admin", 
                    KullaniciAdi = "sysadmin", 
                    Email = "*@example.com", 
                    Rol = Rol.SistemAdmin 
                };
                sysAdmin.SifreHash = hasher2.HashPassword(sysAdmin, "1234");
                db.Kullanicilar.Add(sysAdmin);
                await db.SaveChangesAsync();
            }
        }
    }
}
