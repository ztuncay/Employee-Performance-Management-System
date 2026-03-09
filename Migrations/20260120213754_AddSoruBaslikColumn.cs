using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerformansSitesi.Migrations
{
    /// <inheritdoc />
    public partial class AddSoruBaslikColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SoruBaslik",
                table: "PerformansSorulari",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoruBaslik",
                table: "PerformansSorulari");
        }
    }
}
