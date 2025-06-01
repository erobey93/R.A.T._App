using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class eagerLoadChromsomePair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chromosomes_Species_SpeciesId",
                table: "Chromosomes");

            migrationBuilder.DropForeignKey(
                name: "FK_Genotypes_Animal_AnimalId",
                table: "Genotypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Genotypes_ChromosomePairs_ChromosomePairId",
                table: "Genotypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Genotypes_Trait_TraitId",
                table: "Genotypes");

            migrationBuilder.DropIndex(
                name: "IX_Genotypes_AnimalId_ChromosomePairId",
                table: "Genotypes");

            migrationBuilder.DropIndex(
                name: "IX_Chromosomes_SpeciesId_Number_Arm_Region_Band",
                table: "Chromosomes");

            migrationBuilder.AlterColumn<string>(
                name: "Band",
                table: "Chromosomes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genotypes_AnimalId",
                table: "Genotypes",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_Chromosomes_SpeciesId",
                table: "Chromosomes",
                column: "SpeciesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chromosomes_Species_SpeciesId",
                table: "Chromosomes",
                column: "SpeciesId",
                principalTable: "Species",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Genotypes_Animal_AnimalId",
                table: "Genotypes",
                column: "AnimalId",
                principalTable: "Animal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Genotypes_ChromosomePairs_ChromosomePairId",
                table: "Genotypes",
                column: "ChromosomePairId",
                principalTable: "ChromosomePairs",
                principalColumn: "PairId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Genotypes_Trait_TraitId",
                table: "Genotypes",
                column: "TraitId",
                principalTable: "Trait",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chromosomes_Species_SpeciesId",
                table: "Chromosomes");

            migrationBuilder.DropForeignKey(
                name: "FK_Genotypes_Animal_AnimalId",
                table: "Genotypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Genotypes_ChromosomePairs_ChromosomePairId",
                table: "Genotypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Genotypes_Trait_TraitId",
                table: "Genotypes");

            migrationBuilder.DropIndex(
                name: "IX_Genotypes_AnimalId",
                table: "Genotypes");

            migrationBuilder.DropIndex(
                name: "IX_Chromosomes_SpeciesId",
                table: "Chromosomes");

            migrationBuilder.AlterColumn<string>(
                name: "Band",
                table: "Chromosomes",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genotypes_AnimalId_ChromosomePairId",
                table: "Genotypes",
                columns: new[] { "AnimalId", "ChromosomePairId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chromosomes_SpeciesId_Number_Arm_Region_Band",
                table: "Chromosomes",
                columns: new[] { "SpeciesId", "Number", "Arm", "Region", "Band" },
                unique: true,
                filter: "[Arm] IS NOT NULL AND [Region] IS NOT NULL AND [Band] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Chromosomes_Species_SpeciesId",
                table: "Chromosomes",
                column: "SpeciesId",
                principalTable: "Species",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Genotypes_Animal_AnimalId",
                table: "Genotypes",
                column: "AnimalId",
                principalTable: "Animal",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Genotypes_ChromosomePairs_ChromosomePairId",
                table: "Genotypes",
                column: "ChromosomePairId",
                principalTable: "ChromosomePairs",
                principalColumn: "PairId");

            migrationBuilder.AddForeignKey(
                name: "FK_Genotypes_Trait_TraitId",
                table: "Genotypes",
                column: "TraitId",
                principalTable: "Trait",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
