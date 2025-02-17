using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Data.Models;

namespace RATAPPLibrary.Data.DbContexts
{
    public class AnimalDbContext : DbContext
    {
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Line> Lines { get; set; }
        public DbSet<Litter> Litters { get; set; }
        public DbSet<Species> Species { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Pairing> Pairings { get; set; }

        // Constructor for Dependency Injection (recommended)
        public AnimalDbContext(DbContextOptions<AnimalDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureAnimal(modelBuilder);
            ConfigureStock(modelBuilder);
            ConfigureLine(modelBuilder);
            ConfigureLitter(modelBuilder);
            ConfigureSpecies(modelBuilder);
            ConfigureProject(modelBuilder);
            ConfigurePairing(modelBuilder);
            ConfigureAnimalRecord(modelBuilder);
        }

        // Configure the Animal entity
        private void ConfigureAnimal(ModelBuilder modelBuilder)
        {
            // Primary Key
            modelBuilder.Entity<Animal>()
                .HasKey(a => a.Id); // `Id` is the primary key

            // Define `Sex` field as a required property with a max length of 10
            modelBuilder.Entity<Animal>()
                .Property(a => a.Sex)
                .IsRequired()
                .HasMaxLength(10); // Assuming 'Sex' is a string, limited to 10 characters

            // Define `Name` field as a required property with a max length of 100
            modelBuilder.Entity<Animal>()
                .Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100); // Assuming Name is a string, limited to 100 characters

            // Define `DateOfBirth` as a required property
            modelBuilder.Entity<Animal>()
                .Property(a => a.DateOfBirth)
                .IsRequired();

            // Define `DateOfDeath` as an optional (nullable) property
            modelBuilder.Entity<Animal>()
                .Property(a => a.DateOfDeath)
                .IsRequired(false);  // Nullable property for date of death

            // Define `Age` as a required property
            modelBuilder.Entity<Animal>()
                .Property(a => a.Age)
                .IsRequired();

            // Define the relationship between `Animal` and `Line` (Many-to-One)
            modelBuilder.Entity<Animal>()
                .HasOne(a => a.Line) // Each Animal belongs to one Line
                .WithMany(l => l.Animals) // A Line can have many Animals
                .HasForeignKey(a => a.LineId); // Foreign key in Animal is LineId

        }

        // Configure the Stock entity
        private void ConfigureStock(ModelBuilder modelBuilder)
        {
            // Configure the relationship between Stock and Breeder
            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Breeder) // A Stock belongs to one Breeder
                .WithMany() // A Breeder can have many Stocks
                .HasForeignKey(s => s.BreederId); // Foreign Key in Stock is BreederId

            // Configure the relationship between Stock and Species
            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Species) // A Stock belongs to one Species
                .WithMany() // A Species can have many Stocks
                .HasForeignKey(s => s.SpeciesId); // Foreign Key in Stock is SpeciesId
        }

        // Configure the Line entity
        private void ConfigureLine(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Line>()
                .Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Line>()
                .HasMany(l => l.Animals)
                .WithOne(a => a.Line)
                .HasForeignKey(a => a.LineId);
        }

        // Configure the Litter entity
        private void ConfigureLitter(ModelBuilder modelBuilder)
        {
            // Litter to Pairing (Many-to-One)
            modelBuilder.Entity<Litter>()
                .HasOne(l => l.Pair) // Each Litter has one Pairing
                .WithMany() // A Pairing can have many Litters
                .HasForeignKey(l => l.PairId); // Foreign key in Litter is PairId

            // Litter to Breeder (Many-to-Many)
            modelBuilder.Entity<Litter>()
                .HasMany(l => l.Breeders) // A Litter can have many Breeders
                .WithMany() // A Breeder can be associated with many Litters
                .UsingEntity<Dictionary<string, object>>(
                    "LitterBreeder", // Junction table for Many-to-Many
                    j => j.HasOne<Breeder>().WithMany().HasForeignKey("BreederId"),
                    j => j.HasOne<Litter>().WithMany().HasForeignKey("LitterId")
                );
        }

        // Configure the Species entity
        private void ConfigureSpecies(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Species>()
                .Property(s => s.ScientificName)
                .IsRequired()
                .HasMaxLength(100);
        }

        // Configure the Project entity
        private void ConfigureProject(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .Property(p => p.Description)
                .HasMaxLength(500);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Line)
                .WithOne()
                .HasForeignKey<Project>(p => p.LineId);  // Assumes each line is associated with a line
        }

        // Configure the Pairing entity
        private void ConfigurePairing(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pairing>()
                .HasOne(p => p.Dam)
                .WithMany()
                .HasForeignKey(p => p.DamId);  // Foreign key to Dam

            modelBuilder.Entity<Pairing>()
                .HasOne(p => p.Sire)
                .WithMany()
                .HasForeignKey(p => p.SireId);  // Foreign key to Sire

            modelBuilder.Entity<Pairing>()
                .HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId);  // Foreign key to Project
        }

        //configure Animal record entity
        private void ConfigureAnimalRecord(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AnimalRecord>()
                .HasOne(ar => ar.Animal)           // Each AnimalRecord has one associated Animal
                .WithMany()                         // Animal can have many associated AnimalRecords
                .HasForeignKey(ar => ar.AnimalId);  // Foreign key in AnimalRecord is AnimalId
        }
    }
}
