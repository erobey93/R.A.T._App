using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Club",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Club", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Credentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Individuals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Individuals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Species",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommonName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScientificName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Species", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TraitType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraitType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CredentialsId = table.Column<int>(type: "int", nullable: false),
                    AccountTypeId = table.Column<int>(type: "int", nullable: false),
                    IndividualId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_AccountTypes_AccountTypeId",
                        column: x => x.AccountTypeId,
                        principalTable: "AccountTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Credentials_CredentialsId",
                        column: x => x.CredentialsId,
                        principalTable: "Credentials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trait",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraitTypeId = table.Column<int>(type: "int", nullable: false),
                    Genotype = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CommonName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SpeciesID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trait", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trait_Species_SpeciesID",
                        column: x => x.SpeciesID,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trait_TraitType_TraitTypeId",
                        column: x => x.TraitTypeId,
                        principalTable: "TraitType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Breeder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breeder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Breeder_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BreederClub",
                columns: table => new
                {
                    BreederId = table.Column<int>(type: "int", nullable: false),
                    ClubId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreederClub", x => new { x.BreederId, x.ClubId });
                    table.ForeignKey(
                        name: "FK_BreederClub_Breeder_BreederId",
                        column: x => x.BreederId,
                        principalTable: "Breeder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BreederClub_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stock",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    BreederId = table.Column<int>(type: "int", nullable: false),
                    SpeciesId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stock_Breeder_BreederId",
                        column: x => x.BreederId,
                        principalTable: "Breeder",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Stock_Species_Id",
                        column: x => x.Id,
                        principalTable: "Species",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Stock_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Line",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Line", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Line_Stock_StockId",
                        column: x => x.StockId,
                        principalTable: "Stock",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Animal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineId = table.Column<int>(type: "int", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfDeath = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StockId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Animal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Animal_Line_LineId",
                        column: x => x.LineId,
                        principalTable: "Line",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Animal_Stock_StockId",
                        column: x => x.StockId,
                        principalTable: "Stock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LineId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Line_LineId",
                        column: x => x.LineId,
                        principalTable: "Line",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnimalRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnimalId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimalRecord_Animal_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnimalTrait",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnimalId = table.Column<int>(type: "int", nullable: false),
                    TraitId = table.Column<int>(type: "int", nullable: false),
                    TraitId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalTrait", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimalTrait_Animal_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AnimalTrait_Trait_TraitId",
                        column: x => x.TraitId,
                        principalTable: "Trait",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AnimalTrait_Trait_TraitId1",
                        column: x => x.TraitId1,
                        principalTable: "Trait",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pairing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SireId = table.Column<int>(type: "int", nullable: false),
                    DamId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pairing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pairing_Animal_DamId",
                        column: x => x.DamId,
                        principalTable: "Animal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pairing_Animal_SireId",
                        column: x => x.SireId,
                        principalTable: "Animal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pairing_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Litter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PairId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumPups = table.Column<int>(type: "int", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumFemale = table.Column<int>(type: "int", nullable: true),
                    NumMale = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Litter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Litter_Pairing_PairId",
                        column: x => x.PairId,
                        principalTable: "Pairing",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnimalLitter",
                columns: table => new
                {
                    AnimalsId = table.Column<int>(type: "int", nullable: false),
                    LittersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalLitter", x => new { x.AnimalsId, x.LittersId });
                    table.ForeignKey(
                        name: "FK_AnimalLitter_Animal_AnimalsId",
                        column: x => x.AnimalsId,
                        principalTable: "Animal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnimalLitter_Litter_LittersId",
                        column: x => x.LittersId,
                        principalTable: "Litter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LitterBreeder",
                columns: table => new
                {
                    BreederId = table.Column<int>(type: "int", nullable: false),
                    LitterId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LitterBreeder", x => new { x.BreederId, x.LitterId });
                    table.ForeignKey(
                        name: "FK_LitterBreeder_Breeder_BreederId",
                        column: x => x.BreederId,
                        principalTable: "Breeder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LitterBreeder_Litter_LitterId",
                        column: x => x.LitterId,
                        principalTable: "Litter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Animal_LineId",
                table: "Animal",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_Animal_StockId",
                table: "Animal",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalLitter_LittersId",
                table: "AnimalLitter",
                column: "LittersId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalRecord_AnimalId",
                table: "AnimalRecord",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalTrait_AnimalId",
                table: "AnimalTrait",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalTrait_TraitId",
                table: "AnimalTrait",
                column: "TraitId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimalTrait_TraitId1",
                table: "AnimalTrait",
                column: "TraitId1");

            migrationBuilder.CreateIndex(
                name: "IX_Breeder_UserId",
                table: "Breeder",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BreederClub_ClubId",
                table: "BreederClub",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Line_StockId",
                table: "Line",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_Litter_PairId",
                table: "Litter",
                column: "PairId");

            migrationBuilder.CreateIndex(
                name: "IX_LitterBreeder_LitterId",
                table: "LitterBreeder",
                column: "LitterId");

            migrationBuilder.CreateIndex(
                name: "IX_Pairing_DamId",
                table: "Pairing",
                column: "DamId");

            migrationBuilder.CreateIndex(
                name: "IX_Pairing_ProjectId",
                table: "Pairing",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Pairing_SireId",
                table: "Pairing",
                column: "SireId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_LineId",
                table: "Project",
                column: "LineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stock_BreederId",
                table: "Stock",
                column: "BreederId");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_SpeciesId",
                table: "Stock",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_Trait_SpeciesID",
                table: "Trait",
                column: "SpeciesID");

            migrationBuilder.CreateIndex(
                name: "IX_Trait_TraitTypeId",
                table: "Trait",
                column: "TraitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AccountTypeId",
                table: "Users",
                column: "AccountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CredentialsId",
                table: "Users",
                column: "CredentialsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IndividualId",
                table: "Users",
                column: "IndividualId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnimalLitter");

            migrationBuilder.DropTable(
                name: "AnimalRecord");

            migrationBuilder.DropTable(
                name: "AnimalTrait");

            migrationBuilder.DropTable(
                name: "BreederClub");

            migrationBuilder.DropTable(
                name: "LitterBreeder");

            migrationBuilder.DropTable(
                name: "Trait");

            migrationBuilder.DropTable(
                name: "Club");

            migrationBuilder.DropTable(
                name: "Litter");

            migrationBuilder.DropTable(
                name: "TraitType");

            migrationBuilder.DropTable(
                name: "Pairing");

            migrationBuilder.DropTable(
                name: "Animal");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "Line");

            migrationBuilder.DropTable(
                name: "Stock");

            migrationBuilder.DropTable(
                name: "Breeder");

            migrationBuilder.DropTable(
                name: "Species");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AccountTypes");

            migrationBuilder.DropTable(
                name: "Credentials");

            migrationBuilder.DropTable(
                name: "Individuals");
        }
    }
}
