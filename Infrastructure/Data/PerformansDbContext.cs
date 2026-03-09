using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Domain.Entities;

namespace PerformansSitesi.Infrastructure.Data;

public class PerformansDbContext : DbContext
{
    public PerformansDbContext(DbContextOptions<PerformansDbContext> options) : base(options) { }

    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<Personel> Personeller => Set<Personel>();
    public DbSet<Donem> Donemler => Set<Donem>();
    public DbSet<PerformansSorusu> PerformansSorulari => Set<PerformansSorusu>();
    public DbSet<Degerlendirme> Degerlendirmeler => Set<Degerlendirme>();
    public DbSet<DegerlendirmeDetay> DegerlendirmeDetaylari => Set<DegerlendirmeDetay>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SiteTema> SiteTemalari => Set<SiteTema>();
    public DbSet<ChangeLog> ChangeLogs => Set<ChangeLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DegerlendirmeDetay>(b =>
        {
            b.HasKey(x => x.DetayId);
            b.HasOne(x => x.Degerlendirme).WithMany(d => d.Detaylar).HasForeignKey(x => x.DegerlendirmeId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Soru).WithMany().HasForeignKey(x => x.SoruId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.DegerlendirmeId, x.SoruId }).IsUnique();
            b.Property(x => x.Yonetici1Yorum).HasColumnType("nvarchar(max)");
            b.Property(x => x.Yonetici2Yorum).HasColumnType("nvarchar(max)");
            b.Property(x => x.NihaiYoneticiYorum).HasColumnType("nvarchar(max)");
            b.HasCheckConstraint("CK_DegerlendirmeDetay_Yonetici1Puan", "[Yonetici1Puan] IS NULL OR ([Yonetici1Puan] BETWEEN 1 AND 3)");
        });

        modelBuilder.Entity<Personel>(b =>
        {
            b.HasIndex(x => x.SicilNo).IsUnique();
            b.Property(x => x.AdSoyad).HasMaxLength(200);
            b.Property(x => x.SicilNo).HasMaxLength(50);
            b.Property(x => x.Gorev).HasMaxLength(200);
            b.Property(x => x.ProjeAdi).HasMaxLength(200);
            b.Property(x => x.Mudurluk).HasMaxLength(200);
            b.HasOne(x => x.Yonetici1).WithMany().HasForeignKey(x => x.Yonetici1Id).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Yonetici2).WithMany().HasForeignKey(x => x.Yonetici2Id).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.NihaiYonetici).WithMany().HasForeignKey(x => x.NihaiYoneticiId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Degerlendirme>(b =>
        {
            b.HasIndex(x => new { x.DonemId, x.PersonelId }).IsUnique();
            b.Property(x => x.GenelSonuc).HasMaxLength(50);
        });

        modelBuilder.Entity<AuditLog>(b =>
        {
            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => x.EventType);
            b.HasIndex(x => x.UserRole);
            b.Property(x => x.EventType).HasMaxLength(80);
            b.Property(x => x.UserRole).HasMaxLength(50);
            b.Property(x => x.UserName).HasMaxLength(200);
            b.Property(x => x.Path).HasMaxLength(300);
            b.Property(x => x.IpAddress).HasMaxLength(50);
            b.Property(x => x.Method).HasMaxLength(10);
            b.Property(x => x.QueryString).HasMaxLength(1000);
            b.Property(x => x.UserAgent).HasMaxLength(500);
            b.Property(x => x.Note).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<PerformansSorusu>(b =>
        {
            b.HasKey(x => x.SoruId);
            b.Property(x => x.SoruMetni).HasMaxLength(800);
            b.HasIndex(x => x.SiraNo).IsUnique();
        });

        modelBuilder.Entity<SiteTema>(b =>
        {
            b.HasKey(x => x.TemaId);
            b.HasIndex(x => x.AktifMi);
        });

        modelBuilder.Entity<ChangeLog>(b =>
        {
            b.HasKey(x => x.ChangeId);
            b.HasIndex(x => x.ChangedAt);
            b.HasIndex(x => x.Category);
            b.Property(x => x.Category).HasMaxLength(50);
            b.Property(x => x.Module).HasMaxLength(200);
            b.Property(x => x.Description).HasColumnType("nvarchar(max)");
            b.Property(x => x.OldValue).HasColumnType("nvarchar(max)");
            b.Property(x => x.NewValue).HasColumnType("nvarchar(max)");
        });
    }
}
