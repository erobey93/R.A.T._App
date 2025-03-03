using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateAnimalObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnimalId1",
                table: "AnimalTrait",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "damId",
                table: "Animal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "registrationNumber",
                table: "Animal",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sireId",
                table: "Animal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AnimalTrait_AnimalId1",
                table: "AnimalTrait",
                column: "AnimalId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalTrait_Animal_AnimalId1",
                table: "AnimalTrait",
                column: "AnimalId1",
                principalTable: "Animal",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalTrait_Animal_AnimalId1",
                table: "AnimalTrait");

            migrationBuilder.DropIndex(
                name: "IX_AnimalTrait_AnimalId1",
                table: "AnimalTrait");

            migrationBuilder.DropColumn(
                name: "AnimalId1",
                table: "AnimalTrait");

            migrationBuilder.DropColumn(
                name: "damId",
                table: "Animal");

            migrationBuilder.DropColumn(
                name: "registrationNumber",
                table: "Animal");

            migrationBuilder.DropColumn(
                name: "sireId",
                table: "Animal");
        }
    }
}
