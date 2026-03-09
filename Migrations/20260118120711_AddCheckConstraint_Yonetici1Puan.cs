using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerformansSitesi.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraint_Yonetici1Puan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Degerlendirmeler_Donemler_DonemId",
                table: "Degerlendirmeler");

            migrationBuilder.DropForeignKey(
                name: "FK_Degerlendirmeler_Personeller_PersonelId",
                table: "Degerlendirmeler");

            migrationBuilder.DropForeignKey(
                name: "FK_Kullanicilar_Personeller_PersonelId",
                table: "Kullanicilar");

            migrationBuilder.DropIndex(
                name: "IX_PerformansSorulari_SablonId_SiraNo",
                table: "PerformansSorulari");

            migrationBuilder.DropIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar");

            migrationBuilder.DropIndex(
                name: "IX_DegerlendirmeDetaylari_DegerlendirmeId_SoruId",
                table: "DegerlendirmeDetaylari");

            migrationBuilder.DropColumn(
                name: "OrtalamaPuan",
                table: "Degerlendirmeler");

            migrationBuilder.AlterColumn<string>(
                name: "SoruMetni",
                table: "PerformansSorulari",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "KullaniciAdi",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "AdSoyad",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "AktifMi",
                table: "Donemler",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Ad",
                table: "Donemler",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OlusturmaTarihi",
                table: "Degerlendirmeler",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UserRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Path = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    QueryString = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerformansSorulari_SiraNo",
                table: "PerformansSorulari",
                column: "SiraNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DegerlendirmeDetaylari_DegerlendirmeId",
                table: "DegerlendirmeDetaylari",
                column: "DegerlendirmeId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DegerlendirmeDetay_Yonetici1Puan",
                table: "DegerlendirmeDetaylari",
                sql: "[Yonetici1Puan] IS NULL OR ([Yonetici1Puan] BETWEEN 1 AND 3)");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EventType",
                table: "AuditLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserRole",
                table: "AuditLogs",
                column: "UserRole");

            migrationBuilder.AddForeignKey(
                name: "FK_Degerlendirmeler_Donemler_DonemId",
                table: "Degerlendirmeler",
                column: "DonemId",
                principalTable: "Donemler",
                principalColumn: "DonemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Degerlendirmeler_Personeller_PersonelId",
                table: "Degerlendirmeler",
                column: "PersonelId",
                principalTable: "Personeller",
                principalColumn: "PersonelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Kullanicilar_Personeller_PersonelId",
                table: "Kullanicilar",
                column: "PersonelId",
                principalTable: "Personeller",
                principalColumn: "PersonelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Degerlendirmeler_Donemler_DonemId",
                table: "Degerlendirmeler");

            migrationBuilder.DropForeignKey(
                name: "FK_Degerlendirmeler_Personeller_PersonelId",
                table: "Degerlendirmeler");

            migrationBuilder.DropForeignKey(
                name: "FK_Kullanicilar_Personeller_PersonelId",
                table: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_PerformansSorulari_SiraNo",
                table: "PerformansSorulari");

            migrationBuilder.DropIndex(
                name: "IX_DegerlendirmeDetaylari_DegerlendirmeId",
                table: "DegerlendirmeDetaylari");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DegerlendirmeDetay_Yonetici1Puan",
                table: "DegerlendirmeDetaylari");

            migrationBuilder.AlterColumn<string>(
                name: "SoruMetni",
                table: "PerformansSorulari",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(800)",
                oldMaxLength: 800);

            migrationBuilder.AlterColumn<string>(
                name: "KullaniciAdi",
                table: "Kullanicilar",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Kullanicilar",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AdSoyad",
                table: "Kullanicilar",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "AktifMi",
                table: "Donemler",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Ad",
                table: "Donemler",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OlusturmaTarihi",
                table: "Degerlendirmeler",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<decimal>(
                name: "OrtalamaPuan",
                table: "Degerlendirmeler",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformansSorulari_SablonId_SiraNo",
                table: "PerformansSorulari",
                columns: new[] { "SablonId", "SiraNo" });

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DegerlendirmeDetaylari_DegerlendirmeId_SoruId",
                table: "DegerlendirmeDetaylari",
                columns: new[] { "DegerlendirmeId", "SoruId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Degerlendirmeler_Donemler_DonemId",
                table: "Degerlendirmeler",
                column: "DonemId",
                principalTable: "Donemler",
                principalColumn: "DonemId",
                onDelete: ReferentialAction.Restrict);

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
    }
}
