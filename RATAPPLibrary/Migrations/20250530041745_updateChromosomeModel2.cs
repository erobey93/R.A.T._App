using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateChromosomeModel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chromosomes_SpeciesId_Number",
                table: "Chromosomes");

            migrationBuilder.CreateIndex(
                name: "IX_Chromosomes_SpeciesId_Number_Arm_Region_Band",
                table: "Chromosomes",
                columns: new[] { "SpeciesId", "Number", "Arm", "Region", "Band" },
                unique: true,
                filter: "[Arm] IS NOT NULL AND [Region] IS NOT NULL AND [Band] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chromosomes_SpeciesId_Number_Arm_Region_Band",
                table: "Chromosomes");

            migrationBuilder.CreateIndex(
                name: "IX_Chromosomes_SpeciesId_Number",
                table: "Chromosomes",
                columns: new[] { "SpeciesId", "Number" },
                unique: true);
        }
    }
}
