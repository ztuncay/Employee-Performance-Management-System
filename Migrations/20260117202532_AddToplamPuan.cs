using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerformansSitesi.Migrations
{
    /// <inheritdoc />
    public partial class AddToplamPuan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ToplamPuan",
                table: "Degerlendirmeler",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToplamPuan",
                table: "Degerlendirmeler");
        }
    }
}
