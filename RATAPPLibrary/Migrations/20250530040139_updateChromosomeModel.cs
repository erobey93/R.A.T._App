using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateChromosomeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Arm",
                table: "Chromosomes",
                type: "nvarchar(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Band",
                table: "Chromosomes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Region",
                table: "Chromosomes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Arm",
                table: "Chromosomes");

            migrationBuilder.DropColumn(
                name: "Band",
                table: "Chromosomes");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Chromosomes");
        }
    }
}
