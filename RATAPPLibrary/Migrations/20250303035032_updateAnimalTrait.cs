using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateAnimalTrait : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnimalTrait_Animal_AnimalId1",
                table: "AnimalTrait");

            migrationBuilder.DropForeignKey(
                name: "FK_AnimalTrait_Trait_TraitId1",
                table: "AnimalTrait");

            migrationBuilder.DropIndex(
                name: "IX_AnimalTrait_AnimalId1",
                table: "AnimalTrait");

            migrationBuilder.DropIndex(
                name: "IX_AnimalTrait_TraitId1",
                table: "AnimalTrait");

            migrationBuilder.DropColumn(
                name: "AnimalId1",
                table: "AnimalTrait");

            migrationBuilder.DropColumn(
                name: "TraitId1",
                table: "AnimalTrait");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnimalId1",
                table: "AnimalTrait",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TraitId1",
                table: "AnimalTrait",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnimalTrait_AnimalId1",
                table: "AnimalTrait",
                column: "AnimalId1");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalTrait_TraitId1",
                table: "AnimalTrait",
                column: "TraitId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalTrait_Animal_AnimalId1",
                table: "AnimalTrait",
                column: "AnimalId1",
                principalTable: "Animal",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalTrait_Trait_TraitId1",
                table: "AnimalTrait",
                column: "TraitId1",
                principalTable: "Trait",
                principalColumn: "Id");
        }
    }
}
