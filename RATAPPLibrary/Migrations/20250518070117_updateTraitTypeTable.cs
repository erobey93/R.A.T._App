using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateTraitTypeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "TraitType",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BreedingCalculations",
                columns: table => new
                {
                    CalculationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PairingId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreedingCalculations", x => x.CalculationId);
                    table.ForeignKey(
                        name: "FK_BreedingCalculations_Pairing_PairingId",
                        column: x => x.PairingId,
                        principalTable: "Pairing",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Chromosomes",
                columns: table => new
                {
                    ChromosomeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    SpeciesId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chromosomes", x => x.ChromosomeId);
                    table.ForeignKey(
                        name: "FK_Chromosomes_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PossibleOffspring",
                columns: table => new
                {
                    OffspringId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalculationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Probability = table.Column<float>(type: "real", nullable: false),
                    Phenotype = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GenotypeDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaternalAlleles = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaternalAlleles = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PossibleOffspring", x => x.OffspringId);
                    table.ForeignKey(
                        name: "FK_PossibleOffspring_BreedingCalculations_CalculationId",
                        column: x => x.CalculationId,
                        principalTable: "BreedingCalculations",
                        principalColumn: "CalculationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChromosomePairs",
                columns: table => new
                {
                    PairId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaternalChromosomeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaternalChromosomeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InheritancePattern = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChromosomePairs", x => x.PairId);
                    table.ForeignKey(
                        name: "FK_ChromosomePairs_Chromosomes_MaternalChromosomeId",
                        column: x => x.MaternalChromosomeId,
                        principalTable: "Chromosomes",
                        principalColumn: "ChromosomeId");
                    table.ForeignKey(
                        name: "FK_ChromosomePairs_Chromosomes_PaternalChromosomeId",
                        column: x => x.PaternalChromosomeId,
                        principalTable: "Chromosomes",
                        principalColumn: "ChromosomeId");
                });

            migrationBuilder.CreateTable(
                name: "Genes",
                columns: table => new
                {
                    GeneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChromosomePairId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CommonName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ExpressionAge = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Penetrance = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Expressivity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequiresMonitoring = table.Column<bool>(type: "bit", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ImpactLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genes", x => x.GeneId);
                    table.ForeignKey(
                        name: "FK_Genes_ChromosomePairs_ChromosomePairId",
                        column: x => x.ChromosomePairId,
                        principalTable: "ChromosomePairs",
                        principalColumn: "PairId");
                });

            migrationBuilder.CreateTable(
                name: "Genotypes",
                columns: table => new
                {
                    GenotypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnimalId = table.Column<int>(type: "int", nullable: false),
                    ChromosomePairId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genotypes", x => x.GenotypeId);
                    table.ForeignKey(
                        name: "FK_Genotypes_Animal_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Genotypes_ChromosomePairs_ChromosomePairId",
                        column: x => x.ChromosomePairId,
                        principalTable: "ChromosomePairs",
                        principalColumn: "PairId");
                });

            migrationBuilder.CreateTable(
                name: "Alleles",
                columns: table => new
                {
                    AlleleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Phenotype = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsWildType = table.Column<bool>(type: "bit", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ManagementNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alleles", x => x.AlleleId);
                    table.ForeignKey(
                        name: "FK_Alleles_Genes_GeneId",
                        column: x => x.GeneId,
                        principalTable: "Genes",
                        principalColumn: "GeneId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alleles_GeneId_Symbol",
                table: "Alleles",
                columns: new[] { "GeneId", "Symbol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BreedingCalculations_PairingId",
                table: "BreedingCalculations",
                column: "PairingId");

            migrationBuilder.CreateIndex(
                name: "IX_ChromosomePairs_MaternalChromosomeId",
                table: "ChromosomePairs",
                column: "MaternalChromosomeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChromosomePairs_PaternalChromosomeId",
                table: "ChromosomePairs",
                column: "PaternalChromosomeId");

            migrationBuilder.CreateIndex(
                name: "IX_Chromosomes_SpeciesId_Number",
                table: "Chromosomes",
                columns: new[] { "SpeciesId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genes_ChromosomePairId_Position",
                table: "Genes",
                columns: new[] { "ChromosomePairId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genotypes_AnimalId_ChromosomePairId",
                table: "Genotypes",
                columns: new[] { "AnimalId", "ChromosomePairId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genotypes_ChromosomePairId",
                table: "Genotypes",
                column: "ChromosomePairId");

            migrationBuilder.CreateIndex(
                name: "IX_PossibleOffspring_CalculationId",
                table: "PossibleOffspring",
                column: "CalculationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alleles");

            migrationBuilder.DropTable(
                name: "Genotypes");

            migrationBuilder.DropTable(
                name: "PossibleOffspring");

            migrationBuilder.DropTable(
                name: "Genes");

            migrationBuilder.DropTable(
                name: "BreedingCalculations");

            migrationBuilder.DropTable(
                name: "ChromosomePairs");

            migrationBuilder.DropTable(
                name: "Chromosomes");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TraitType");
        }
    }
}
