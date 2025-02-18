using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Data.Models.Ancestry;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Identity.Data;
using RATAPPLibrary.Data.Models.Requests;

namespace RATAPPLibrary.Data.DbContexts
{
    public class RatAppDbContext : DbContext
    {
        public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(); // Logs to the console
        });

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer("Server=EARSLAPTOP;Database=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;")
                .UseLoggerFactory(MyLoggerFactory) // Attach the logger
                .EnableSensitiveDataLogging()    // Show parameter values in logs (optional)
                .LogTo(Console.WriteLine, LogLevel.Debug); // Set log level to Debug
        }

        //Account management
        public DbSet<User> Users { get; set; }
        public DbSet<Credentials> Credentials { get; set; }
        public DbSet<AccountType> AccountTypes { get; set; }
        public DbSet<Individual> Individuals { get; set; }

        //Ancestry
        public DbSet<AncestryRecord> Ancestries { get; set; }
        public DbSet<AncestryRecordLink> AncestorLink { get; set; }

        //Animal Management
        public DbSet<Animal> Animals { get; set; }
        public DbSet<AnimalRecord> AnimalRecords { get; set; }
        public DbSet<Species> Species { get; set; }

        //Breeding
        public DbSet<Breeder> Breeder { get; set; }
        public DbSet<Club> Club { get; set; } // Clubs should be of type 'Club'
        public DbSet<BreederClub> BreederClub { get; set; } // BreederClub should be a junction table for many-to-many
        public DbSet<Stock> Stock { get; set; }
        public DbSet<Line> Line { get; set; }
        public DbSet<Litter> Litter { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<Pairing> Pairing { get; set; }

        //Genetics
        public DbSet<Trait> Trait { get; set; }  // This should exist
        public DbSet<TraitType> TraitType { get; set; }  // This should exist  
        public DbSet<AnimalTrait> AnimalTrait { get; set; }

        //Health
        public DbSet<HealthRecord> HealthRecord { get; set; }

        //Requests
        public DbSet<Models.Requests.LoginRequest> LoginRequest { get; set; }
        public DbSet<UpdateCredentialsRequest> UpdateCredentialsRequest { get; set; }
        public DbSet<LoginResponse> LoginResponse { get; set; }

        // Constructor for Dependency Injection (recommended)
        public RatAppDbContext(DbContextOptions <RatAppDbContext> options) : base(options)
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

            ConfigureStock(modelBuilder);
            ConfigureLine(modelBuilder);
            ConfigureSpecies(modelBuilder);
            ConfigureAnimal(modelBuilder);

            ConfigureLitter(modelBuilder);
            ConfigureProject(modelBuilder);
            ConfigurePairing(modelBuilder);
            ConfigureAnimalRecord(modelBuilder);

            ConfigureTrait(modelBuilder);

            // Configure the Ancestry entities
            //ConfigureAncestryRecord(modelBuilder);
            //ConfigureAncestryRecordLink(modelBuilder);

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
                .HasForeignKey(u => u.AccountTypeId)
                .OnDelete(DeleteBehavior.NoAction); //TODO must have a default account type

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
        //if user is deleted, breeder should be deleted as well TODO think through these relationships, scenarios and relevance before solidfying choices, but just making decisions to move forward for now 
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
                .HasForeignKey(a => a.LineId) // Foreign key in Animal is LineId
                .OnDelete(DeleteBehavior.NoAction); //I don't want the animal to be deleted if the line is deleted TODO

        }

        // Configure the Stock entity
        private void ConfigureStock(ModelBuilder modelBuilder)
        {
            // Configure the relationship between Stock and Breeder
            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Breeder) // A Stock belongs to one Breeder
                .WithMany() // A Breeder can have many Stocks
                .HasForeignKey(s => s.BreederId) // Foreign Key in Stock is BreederId
                .OnDelete(DeleteBehavior.NoAction); //TODO - if breeder is deleted, what should happen to stock? My thought is that stock should not be deleted because it is a collection of animals and would cause huge issues if something accidental happened 

            // Configure the relationship between Stock and Species
            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Species) // A Stock belongs to one Species
                .WithMany() // A Species can have many Stocks
                .HasForeignKey(s => s.Id) // Foreign Key in Stock is SpeciesId
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete
        }

        // Configure the Line entity
        private void ConfigureLine(ModelBuilder modelBuilder)
        {
            // Configure the Name property
            modelBuilder.Entity<Line>()
                .Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Configure the relationship with Stock
            modelBuilder.Entity<Line>()
                .HasOne(l => l.Stock)       // Each Line belongs to one Stock
                .WithMany()                 // A Stock can have many Lines (if this is the intended relationship)
                .HasForeignKey(l => l.StockId) // Foreign key in Line table is StockId
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete TODO 

            // Configure the relationship with Animals
            modelBuilder.Entity<Line>()
                .HasMany(l => l.Animals)    // A Line can have many Animals
                .WithOne(a => a.Line)       // Each Animal belongs to one Line
                .HasForeignKey(a => a.LineId); // Foreign key in Animal table is LineId
        }

        // Configure the Litter entity
        private void ConfigureLitter(ModelBuilder modelBuilder)
        {
            // Litter to Pairing (Many-to-One)
            modelBuilder.Entity<Litter>()
                .HasOne(l => l.Pair) // Each Litter has one Pairing
                .WithMany() // A Pairing can have many Litters
                .HasForeignKey(l => l.PairId) // Foreign key in Litter is PairId
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete TODO 

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
               .HasForeignKey(p => p.DamId)
               .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete

            modelBuilder.Entity<Pairing>()
                .HasOne(p => p.Sire)
                .WithMany()
                .HasForeignKey(p => p.SireId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete

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

        private void ConfigureTrait(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TraitType>()
                .HasKey(tt => tt.Id);

            modelBuilder.Entity<TraitType>()
                .Property(tt => tt.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<TraitType>()
                .HasMany(tt => tt.Traits)
                .WithOne(t => t.TraitType)
                .HasForeignKey(t => t.TraitTypeId);

            modelBuilder.Entity<Trait>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Trait>()
                .Property(t => t.CommonName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Trait>()
                .Property(t => t.Genotype)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
