using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateGeneticsDatabase : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "TraitType");
        }
    }
}
