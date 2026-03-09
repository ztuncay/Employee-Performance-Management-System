using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerformansSitesi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Donemler",
                columns: table => new
                {
                    DonemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donemler", x => x.DonemId);
                });

            migrationBuilder.CreateTable(
                name: "PerformansSorulari",
                columns: table => new
                {
                    SoruId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SablonId = table.Column<int>(type: "int", nullable: false),
                    SoruMetni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiraNo = table.Column<int>(type: "int", nullable: false),
                    ZorunluMu = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformansSorulari", x => x.SoruId);
                });

            migrationBuilder.CreateTable(
                name: "DegerlendirmeDetaylari",
                columns: table => new
                {
                    DetayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DegerlendirmeId = table.Column<int>(type: "int", nullable: false),
                    SoruId = table.Column<int>(type: "int", nullable: false),
                    Yonetici1Puan = table.Column<int>(type: "int", nullable: true),
                    Yonetici1Yorum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Yonetici2Yorum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NihaiYoneticiYorum = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DegerlendirmeDetaylari", x => x.DetayId);
                    table.ForeignKey(
                        name: "FK_DegerlendirmeDetaylari_PerformansSorulari_SoruId",
                        column: x => x.SoruId,
                        principalTable: "PerformansSorulari",
                        principalColumn: "SoruId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Degerlendirmeler",
                columns: table => new
                {
                    DegerlendirmeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonemId = table.Column<int>(type: "int", nullable: false),
                    PersonelId = table.Column<int>(type: "int", nullable: false),
                    SablonId = table.Column<int>(type: "int", nullable: false),
                    Yonetici1Id = table.Column<int>(type: "int", nullable: false),
                    Yonetici2Id = table.Column<int>(type: "int", nullable: false),
                    NihaiYoneticiId = table.Column<int>(type: "int", nullable: false),
                    OrtalamaPuan = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    GenelSonuc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Yonetici1Notu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Yonetici2Notu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NihaiYoneticiNotu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GucluYonler = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GelisimeAcikYonler = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GelisimOnerileri = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Degerlendirmeler", x => x.DegerlendirmeId);
                    table.ForeignKey(
                        name: "FK_Degerlendirmeler_Donemler_DonemId",
                        column: x => x.DonemId,
                        principalTable: "Donemler",
                        principalColumn: "DonemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    KullaniciId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SifreHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<int>(type: "int", nullable: false),
                    PersonelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.KullaniciId);
                });

            migrationBuilder.CreateTable(
                name: "Personeller",
                columns: table => new
                {
                    PersonelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SicilNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Departman = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Yonetici1Id = table.Column<int>(type: "int", nullable: false),
                    Yonetici2Id = table.Column<int>(type: "int", nullable: false),
                    NihaiYoneticiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personeller", x => x.PersonelId);
                    table.ForeignKey(
                        name: "FK_Personeller_Kullanicilar_NihaiYoneticiId",
                        column: x => x.NihaiYoneticiId,
                        principalTable: "Kullanicilar",
                        principalColumn: "KullaniciId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Personeller_Kullanicilar_Yonetici1Id",
                        column: x => x.Yonetici1Id,
                        principalTable: "Kullanicilar",
                        principalColumn: "KullaniciId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Personeller_Kullanicilar_Yonetici2Id",
                        column: x => x.Yonetici2Id,
                        principalTable: "Kullanicilar",
                        principalColumn: "KullaniciId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DegerlendirmeDetaylari_DegerlendirmeId_SoruId",
                table: "DegerlendirmeDetaylari",
                columns: new[] { "DegerlendirmeId", "SoruId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DegerlendirmeDetaylari_SoruId",
                table: "DegerlendirmeDetaylari",
                column: "SoruId");

            migrationBuilder.CreateIndex(
                name: "IX_Degerlendirmeler_DonemId_PersonelId",
                table: "Degerlendirmeler",
                columns: new[] { "DonemId", "PersonelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Degerlendirmeler_PersonelId",
                table: "Degerlendirmeler",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_PersonelId",
                table: "Kullanicilar",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformansSorulari_SablonId_SiraNo",
                table: "PerformansSorulari",
                columns: new[] { "SablonId", "SiraNo" });

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_NihaiYoneticiId",
                table: "Personeller",
                column: "NihaiYoneticiId");

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_SicilNo",
                table: "Personeller",
                column: "SicilNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_Yonetici1Id",
                table: "Personeller",
                column: "Yonetici1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_Yonetici2Id",
                table: "Personeller",
                column: "Yonetici2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DegerlendirmeDetaylari_Degerlendirmeler_DegerlendirmeId",
                table: "DegerlendirmeDetaylari",
                column: "DegerlendirmeId",
                principalTable: "Degerlendirmeler",
                principalColumn: "DegerlendirmeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Degerlendirmeler_Personeller_PersonelId",
                table: "Degerlendirmeler",
                column: "PersonelId",
                principalTable: "Personeller",
                principalColumn: "PersonelId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Kullanicilar_Personeller_PersonelId",
                table: "Kullanicilar",
                column: "PersonelId",
                principalTable: "Personeller",
                principalColumn: "PersonelId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kullanicilar_Personeller_PersonelId",
                table: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "DegerlendirmeDetaylari");

            migrationBuilder.DropTable(
                name: "Degerlendirmeler");

            migrationBuilder.DropTable(
                name: "PerformansSorulari");

            migrationBuilder.DropTable(
                name: "Donemler");

            migrationBuilder.DropTable(
                name: "Personeller");

            migrationBuilder.DropTable(
                name: "Kullanicilar");
        }
    }
}
