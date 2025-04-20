using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class reAddLineage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lineages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnimalId = table.Column<int>(type: "int", nullable: false),
                    AncestorId = table.Column<int>(type: "int", nullable: false),
                    Generation = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lineages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lineages_Animal_AncestorId",
                        column: x => x.AncestorId,
                        principalTable: "Animal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Lineages_Animal_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animal",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lineages_AncestorId",
                table: "Lineages",
                column: "AncestorId");

            migrationBuilder.CreateIndex(
                name: "IX_Lineages_AnimalId",
                table: "Lineages",
                column: "AnimalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
