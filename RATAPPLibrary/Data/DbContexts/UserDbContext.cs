using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RATAPPLibrary.Data.DbContexts
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Credentials> Credentials { get; set; }
        public DbSet<AccountType> AccountTypes { get; set; }
        public DbSet<Individual> Individuals { get; set; }
        public DbSet<Breeder> Breeders { get; set; }
        public DbSet<Club> Clubs { get; set; } // Clubs should be of type 'Club'
        public DbSet<BreederClub> BreederClubs { get; set; } // BreederClub should be a junction table for many-to-many

        // Constructor for Dependency Injection (for uts)
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureUser(modelBuilder);
            ConfigureCredentials(modelBuilder);
            ConfigureAccountType(modelBuilder);
            ConfigureIndividual(modelBuilder);
            ConfigureBreeder(modelBuilder);
            ConfigureClub(modelBuilder);
            ConfigureBreederClub(modelBuilder);
        }

        // Configure the User entity
        private void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Individual)
                .WithOne() // Each User has one Individual
                .HasForeignKey<User>(u => u.IndividualId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.AccountType)
                .WithMany() // An AccountType can have many Users
                .HasForeignKey(u => u.AccountTypeId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Credentials)
                .WithOne() // Each User has one Credentials
                .HasForeignKey<User>(u => u.CredentialsId);
        }

        // Configure the Credentials entity
        private void ConfigureCredentials(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Credentials>()
                .Property(c => c.Username)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Credentials>()
                .Property(c => c.Password)
                .IsRequired(); // Ensure PasswordHash is required
        }

        // Configure the AccountType entity
        private void ConfigureAccountType(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountType>()
                .Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(50);
        }

        // Configure the Individual entity
        private void ConfigureIndividual(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Individual>()
                .Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Individual>()
                .Property(i => i.Phone)
                .HasMaxLength(15);

            modelBuilder.Entity<Individual>()
                .Property(i => i.Email)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Individual>()
                .Property(i => i.Location)
                .IsRequired()
                .HasMaxLength(255);
        }

        // Configure the Breeder entity
        private void ConfigureBreeder(ModelBuilder modelBuilder)
        {
            // Configure Breeder and User relationship (One-to-One)
            modelBuilder.Entity<Breeder>()
                .HasOne(b => b.User)  // Breeder has one User
                .WithOne()  // User has one Breeder
                .HasForeignKey<Breeder>(b => b.UserId);  // Foreign key in Breeder is UserId

            // Configure Breeder properties
            modelBuilder.Entity<Breeder>()
                .Property(b => b.LogoPath)
                .HasMaxLength(255)  // Assuming this is a path to the logo image
                .IsRequired(false);  // Set to false if LogoPath is optional
        }

        // Configure the Club entity
        private void ConfigureClub(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Club>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100); // Ensure Club Name is required and limited to 100 characters
        }

        // Configure the BreederClub entity (junction table for many-to-many relationship)
        private void ConfigureBreederClub(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BreederClub>()
                .HasKey(bc => new { bc.BreederId, bc.ClubId }); // Composite key for BreederClub (many-to-many)

            modelBuilder.Entity<BreederClub>()
                .HasOne(bc => bc.Breeder)
                .WithMany(b => b.BreederClubs) // Each Breeder can have many BreederClubs
                .HasForeignKey(bc => bc.BreederId);

            modelBuilder.Entity<BreederClub>()
                .HasOne(bc => bc.Club)
                .WithMany(c => c.BreederClubs) // Each Club can have many BreederClubs
                .HasForeignKey(bc => bc.ClubId);
        }
    }
}
