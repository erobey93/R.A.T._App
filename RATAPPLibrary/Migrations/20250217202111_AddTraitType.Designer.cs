﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RATAPPLibrary.Data.DbContexts;

#nullable disable

namespace RATAPPLibrary.Migrations
{
    [DbContext(typeof(RatAppDbContext))]
    [Migration("20250217202111_AddTraitType")]
    partial class AddTraitType
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AnimalLitter", b =>
                {
                    b.Property<int>("AnimalsId")
                        .HasColumnType("int");

                    b.Property<int>("LittersId")
                        .HasColumnType("int");

                    b.HasKey("AnimalsId", "LittersId");

                    b.HasIndex("LittersId");

                    b.ToTable("AnimalLitter");
                });

            modelBuilder.Entity("LitterBreeder", b =>
                {
                    b.Property<int>("BreederId")
                        .HasColumnType("int");

                    b.Property<int>("LitterId")
                        .HasColumnType("int");

                    b.HasKey("BreederId", "LitterId");

                    b.HasIndex("LitterId");

                    b.ToTable("LitterBreeder");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.AccountType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("AccountTypes");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Animal", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Age")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateOfDeath")
                        .HasColumnType("datetime2");

                    b.Property<int>("LineId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Sex")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<int?>("StockId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("LineId");

                    b.HasIndex("StockId");

                    b.ToTable("Animals");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.AnimalRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AnimalId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AnimalId");

                    b.ToTable("AnimalRecord");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Breeder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("LogoPath")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Breeders");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.BreederClub", b =>
                {
                    b.Property<int>("BreederId")
                        .HasColumnType("int");

                    b.Property<int>("ClubId")
                        .HasColumnType("int");

                    b.HasKey("BreederId", "ClubId");

                    b.HasIndex("ClubId");

                    b.ToTable("BreederClubs");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Club", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Clubs");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Line", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("StockId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("StockId");

                    b.ToTable("Lines");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Litter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("NumFemale")
                        .HasColumnType("int");

                    b.Property<int?>("NumMale")
                        .HasColumnType("int");

                    b.Property<int?>("NumPups")
                        .HasColumnType("int");

                    b.Property<int>("PairId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PairId");

                    b.ToTable("Litters");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Pairing", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<int>("DamId")
                        .HasColumnType("int");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.Property<int>("SireId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DamId");

                    b.HasIndex("ProjectId");

                    b.HasIndex("SireId");

                    b.ToTable("Pairings");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<int>("LineId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("LineId")
                        .IsUnique();

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Stock", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<int>("BreederId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SpeciesId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BreederId");

                    b.HasIndex("SpeciesId");

                    b.ToTable("Stocks");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Credentials", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Credentials");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Genetics.AnimalTrait", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AnimalId")
                        .HasColumnType("int");

                    b.Property<int>("TraitId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AnimalId");

                    b.HasIndex("TraitId");

                    b.ToTable("AnimalTrait");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Genetics.Trait", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CommonName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Genotype")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("TraitTypeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TraitTypeId");

                    b.ToTable("Trait");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Genetics.TraitType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("TraitType");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Individual", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.HasKey("Id");

                    b.ToTable("Individuals");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Species", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CommonName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScientificName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Species");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountTypeId")
                        .HasColumnType("int");

                    b.Property<int>("CredentialsId")
                        .HasColumnType("int");

                    b.Property<string>("ImagePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("IndividualId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AccountTypeId");

                    b.HasIndex("CredentialsId")
                        .IsUnique();

                    b.HasIndex("IndividualId")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("AnimalLitter", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.Animal", null)
                        .WithMany()
                        .HasForeignKey("AnimalsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RATAPPLibrary.Data.Models.Breeding.Litter", null)
                        .WithMany()
                        .HasForeignKey("LittersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("LitterBreeder", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.Breeding.Breeder", null)
                        .WithMany()
                        .HasForeignKey("BreederId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RATAPPLibrary.Data.Models.Breeding.Litter", null)
                        .WithMany()
                        .HasForeignKey("LitterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Animal", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.Breeding.Line", "Line")
                        .WithMany("Animals")
                        .HasForeignKey("LineId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("RATAPPLibrary.Data.Models.Breeding.Stock", null)
                        .WithMany("Animals")
                        .HasForeignKey("StockId");

                    b.Navigation("Line");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.AnimalRecord", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.Animal", "Animal")
                        .WithMany()
                        .HasForeignKey("AnimalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Animal");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Breeder", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.User", "User")
                        .WithOne()
                        .HasForeignKey("RATAPPLibrary.Data.Models.Breeding.Breeder", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.BreederClub", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.Breeding.Breeder", "Breeder")
                        .WithMany("BreederClubs")
                        .HasForeignKey("BreederId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RATAPPLibrary.Data.Models.Breeding.Club", "Club")
                        .WithMany("BreederClubs")
                        .HasForeignKey("ClubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Breeder");

                    b.Navigation("Club");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Line", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.Breeding.Stock", "Stock")
                        .WithMany()
                        .HasForeignKey("StockId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Litter", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.Breeding.Pairing", "Pair")
                        .WithMany()
                        .HasForeignKey("PairId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Pair");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Pairing", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.Animal", "Dam")
                        .WithMany()
                        .HasForeignKey("DamId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("RATAPPLibrary.Data.Models.Breeding.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RATAPPLibrary.Data.Models.Animal", "Sire")
                        .WithMany()
                        .HasForeignKey("SireId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Dam");

                    b.Navigation("Project");

                    b.Navigation("Sire");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Project", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.Breeding.Line", "Line")
                        .WithOne()
                        .HasForeignKey("RATAPPLibrary.Data.Models.Breeding.Project", "LineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Line");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Stock", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.Breeding.Breeder", "Breeder")
                        .WithMany()
                        .HasForeignKey("BreederId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("RATAPPLibrary.Data.Models.Species", "Species")
                        .WithMany()
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("RATAPPLibrary.Data.Models.Species", null)
                        .WithMany("Stocks")
                        .HasForeignKey("SpeciesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Breeder");

                    b.Navigation("Species");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Genetics.AnimalTrait", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.Animal", "Animal")
                        .WithMany()
                        .HasForeignKey("AnimalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RATAPPLibrary.Data.Models.Genetics.Trait", "Trait")
                        .WithMany("AnimalTraits")
                        .HasForeignKey("TraitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Animal");

                    b.Navigation("Trait");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Genetics.Trait", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.Genetics.TraitType", "TraitType")
                        .WithMany("Traits")
                        .HasForeignKey("TraitTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TraitType");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.User", b =>
                {
                    b.HasOne("RATAPPLibrary.Data.Models.AccountType", "AccountType")
                        .WithMany()
                        .HasForeignKey("AccountTypeId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("RATAPPLibrary.Data.Models.Credentials", "Credentials")
                        .WithOne()
                        .HasForeignKey("RATAPPLibrary.Data.Models.User", "CredentialsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RATAPPLibrary.Data.Models.Individual", "Individual")
                        .WithOne()
                        .HasForeignKey("RATAPPLibrary.Data.Models.User", "IndividualId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AccountType");

                    b.Navigation("Credentials");

                    b.Navigation("Individual");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Breeder", b =>
                {
                    b.Navigation("BreederClubs");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Club", b =>
                {
                    b.Navigation("BreederClubs");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Line", b =>
                {
                    b.Navigation("Animals");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Breeding.Stock", b =>
                {
                    b.Navigation("Animals");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Genetics.Trait", b =>
                {
                    b.Navigation("AnimalTraits");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Genetics.TraitType", b =>
                {
                    b.Navigation("Traits");
                });

            modelBuilder.Entity("RATAPPLibrary.Data.Models.Species", b =>
                {
                    b.Navigation("Stocks");
                });
#pragma warning restore 612, 618
        }
    }
}
