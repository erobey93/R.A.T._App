using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateTrait : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animal_Stock_StockId",
                table: "Animal");

            migrationBuilder.AddColumn<int>(
                name: "SpeciesID",
                table: "Trait",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SpeciesId",
                table: "AnimalTrait",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "StockId",
                table: "Animal",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Trait_SpeciesID",
                table: "Trait",
                column: "SpeciesID");

            migrationBuilder.AddForeignKey(
                name: "FK_Animal_Stock_StockId",
                table: "Animal",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Trait_Species_SpeciesID",
                table: "Trait",
                column: "SpeciesID",
                principalTable: "Species",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animal_Stock_StockId",
                table: "Animal");

            migrationBuilder.DropForeignKey(
                name: "FK_Trait_Species_SpeciesID",
                table: "Trait");

            migrationBuilder.DropIndex(
                name: "IX_Trait_SpeciesID",
                table: "Trait");

            migrationBuilder.DropColumn(
                name: "SpeciesID",
                table: "Trait");

            migrationBuilder.DropColumn(
                name: "SpeciesId",
                table: "AnimalTrait");

            migrationBuilder.AlterColumn<int>(
                name: "StockId",
                table: "Animal",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Animal_Stock_StockId",
                table: "Animal",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
