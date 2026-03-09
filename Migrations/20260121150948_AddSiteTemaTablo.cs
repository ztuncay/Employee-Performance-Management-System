using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerformansSitesi.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteTemaTablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteTemalari",
                columns: table => new
                {
                    TemaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemaAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    PrimaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SecondaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SuccessColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    WarningColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    DangerColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    InfoColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    LightColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    DarkColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    FontFamily = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FontSize = table.Column<int>(type: "int", nullable: false),
                    HeadingFontFamily = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NavbarPosition = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NavbarTheme = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NavbarBgColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SidebarWidth = table.Column<int>(type: "int", nullable: false),
                    ContainerSize = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CardBorderRadius = table.Column<int>(type: "int", nullable: false),
                    CardShadow = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ButtonBorderRadius = table.Column<int>(type: "int", nullable: false),
                    ButtonSize = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CustomCss = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteTemalari", x => x.TemaId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteTemalari_AktifMi",
                table: "SiteTemalari",
                column: "AktifMi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteTemalari");
        }
    }
}
