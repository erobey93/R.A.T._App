using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateGenetics2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GenericGenotype",
                columns: table => new
                {
                    GenotypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChromosomePairId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TraitId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenericGenotype", x => x.GenotypeId);
                    table.ForeignKey(
                        name: "FK_GenericGenotype_ChromosomePairs_ChromosomePairId",
                        column: x => x.ChromosomePairId,
                        principalTable: "ChromosomePairs",
                        principalColumn: "PairId");
                    table.ForeignKey(
                        name: "FK_GenericGenotype_Trait_TraitId",
                        column: x => x.TraitId,
                        principalTable: "Trait",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GenericGenotype_ChromosomePairId_TraitId",
                table: "GenericGenotype",
                columns: new[] { "ChromosomePairId", "TraitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GenericGenotype_TraitId",
                table: "GenericGenotype",
                column: "TraitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenericGenotype");
        }
    }
}
