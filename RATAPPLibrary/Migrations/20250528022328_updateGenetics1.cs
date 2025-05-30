using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateGenetics1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TraitId",
                table: "Genotypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "InheritancePattern",
                table: "Alleles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Genotypes_TraitId",
                table: "Genotypes",
                column: "TraitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Genotypes_Trait_TraitId",
                table: "Genotypes",
                column: "TraitId",
                principalTable: "Trait",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Genotypes_Trait_TraitId",
                table: "Genotypes");

            migrationBuilder.DropIndex(
                name: "IX_Genotypes_TraitId",
                table: "Genotypes");

            migrationBuilder.DropColumn(
                name: "TraitId",
                table: "Genotypes");

            migrationBuilder.DropColumn(
                name: "InheritancePattern",
                table: "Alleles");
        }
    }
}
