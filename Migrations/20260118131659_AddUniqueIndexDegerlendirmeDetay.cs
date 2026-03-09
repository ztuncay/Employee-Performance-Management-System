using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerformansSitesi.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexDegerlendirmeDetay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DegerlendirmeDetaylari_DegerlendirmeId",
                table: "DegerlendirmeDetaylari");

            migrationBuilder.CreateIndex(
                name: "IX_DegerlendirmeDetaylari_DegerlendirmeId_SoruId",
                table: "DegerlendirmeDetaylari",
                columns: new[] { "DegerlendirmeId", "SoruId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DegerlendirmeDetaylari_DegerlendirmeId_SoruId",
                table: "DegerlendirmeDetaylari");

            migrationBuilder.CreateIndex(
                name: "IX_DegerlendirmeDetaylari_DegerlendirmeId",
                table: "DegerlendirmeDetaylari",
                column: "DegerlendirmeId");
        }
    }
}
