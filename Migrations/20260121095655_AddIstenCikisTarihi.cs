using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerformansSitesi.Migrations
{
    /// <inheritdoc />
    public partial class AddIstenCikisTarihi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "IstenCikisTarihi",
                table: "Personeller",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IstenCikisTarihi",
                table: "Personeller");
        }
    }
}
