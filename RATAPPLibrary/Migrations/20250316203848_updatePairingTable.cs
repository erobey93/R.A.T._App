using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class updatePairingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PairingEndDate",
                table: "Pairing",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PairingStartDate",
                table: "Pairing",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pairingId",
                table: "Pairing",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PairingEndDate",
                table: "Pairing");

            migrationBuilder.DropColumn(
                name: "PairingStartDate",
                table: "Pairing");

            migrationBuilder.DropColumn(
                name: "pairingId",
                table: "Pairing");
        }
    }
}
