using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class stockfuckyou : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animals_Lines_LineId",
                table: "Animals");

            migrationBuilder.DropForeignKey(
                name: "FK_Lines_Stocks_StockId",
                table: "Lines");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Breeders_BreederId",
                table: "Stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Species_Id",
                table: "Stocks");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Stocks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Lines",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Animals_Lines_LineId",
                table: "Animals",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lines_Stocks_StockId",
                table: "Lines",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Breeders_BreederId",
                table: "Stocks",
                column: "BreederId",
                principalTable: "Breeders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animals_Lines_LineId",
                table: "Animals");

            migrationBuilder.DropForeignKey(
                name: "FK_Lines_Stocks_StockId",
                table: "Lines");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Breeders_BreederId",
                table: "Stocks");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Stocks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Lines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Animals_Lines_LineId",
                table: "Animals",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lines_Stocks_StockId",
                table: "Lines",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Breeders_BreederId",
                table: "Stocks",
                column: "BreederId",
                principalTable: "Breeders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Species_Id",
                table: "Stocks",
                column: "Id",
                principalTable: "Species",
                principalColumn: "Id");
        }
    }
}
