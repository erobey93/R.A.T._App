using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class notSure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animal_Stock_StockId",
                table: "Animal");

            migrationBuilder.DropForeignKey(
                name: "FK_AnimalRecords_Animal_AnimalId",
                table: "AnimalRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnimalRecords",
                table: "AnimalRecords");

            migrationBuilder.RenameTable(
                name: "AnimalRecords",
                newName: "AnimalRecord");

            migrationBuilder.RenameIndex(
                name: "IX_AnimalRecords_AnimalId",
                table: "AnimalRecord",
                newName: "IX_AnimalRecord_AnimalId");

            migrationBuilder.AlterColumn<int>(
                name: "StockId",
                table: "Animal",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnimalRecord",
                table: "AnimalRecord",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Animal_Stock_StockId",
                table: "Animal",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalRecord_Animal_AnimalId",
                table: "AnimalRecord",
                column: "AnimalId",
                principalTable: "Animal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animal_Stock_StockId",
                table: "Animal");

            migrationBuilder.DropForeignKey(
                name: "FK_AnimalRecord_Animal_AnimalId",
                table: "AnimalRecord");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnimalRecord",
                table: "AnimalRecord");

            migrationBuilder.RenameTable(
                name: "AnimalRecord",
                newName: "AnimalRecords");

            migrationBuilder.RenameIndex(
                name: "IX_AnimalRecord_AnimalId",
                table: "AnimalRecords",
                newName: "IX_AnimalRecords_AnimalId");

            migrationBuilder.AlterColumn<int>(
                name: "StockId",
                table: "Animal",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnimalRecords",
                table: "AnimalRecords",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Animal_Stock_StockId",
                table: "Animal",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnimalRecords_Animal_AnimalId",
                table: "AnimalRecords",
                column: "AnimalId",
                principalTable: "Animal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
