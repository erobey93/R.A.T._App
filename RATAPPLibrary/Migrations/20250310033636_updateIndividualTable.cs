using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updateIndividualTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Individuals",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Individuals",
                newName: "State");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Individuals",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Individuals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Individuals");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Individuals");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "Individuals",
                newName: "Location");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Individuals",
                newName: "Name");
        }
    }
}
