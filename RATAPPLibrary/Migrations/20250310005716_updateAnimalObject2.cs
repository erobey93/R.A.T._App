using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateAnimalObject2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "damId",
                table: "Animal");

            migrationBuilder.DropColumn(
                name: "sireId",
                table: "Animal");

            migrationBuilder.AddColumn<int>(
                name: "weight",
                table: "Animal",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "weight",
                table: "Animal");

            migrationBuilder.AddColumn<int>(
                name: "damId",
                table: "Animal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sireId",
                table: "Animal",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
