using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class animalImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "imageUrl",
                table: "Animal",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imageUrl",
                table: "Animal");
        }
    }
}
