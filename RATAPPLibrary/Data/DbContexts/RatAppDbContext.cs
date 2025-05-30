using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Data.Models.Ancestry;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Identity.Data;
using RATAPPLibrary.Data.Models.Requests;
using RATAPPLibrary.Data.Models.Animal_Management;

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
            if (optionsBuilder == null)
            {
                optionsBuilder
                    .UseSqlServer("Server=EARSLAPTOP;Database=RATAPPLIBRARY2;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;")
                    .UseLoggerFactory(MyLoggerFactory) // Attach the logger
                    .EnableSensitiveDataLogging()    // Show parameter values in logs (optional)
                    .LogTo(Console.WriteLine, LogLevel.Debug); // Set log level to Debug
            }
        }

        //Account management
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Credentials> Credentials { get; set; }
        public virtual DbSet<AccountType> AccountTypes { get; set; }
        public virtual DbSet<Individual> Individuals { get; set; }

        //Ancestry
        //public DbSet<AncestryRecord> Ancestries { get; set; }
        //public DbSet<AncestryRecordLink> AncestorLink { get; set; }
        public virtual DbSet<Lineage> Lineages { get; set; }

        //Animal Management
        public virtual DbSet<Animal> Animal { get; set; }
        public virtual DbSet<AnimalRecord> AnimalRecord { get; set; }
        public virtual DbSet<AnimalImage> AnimalImage { get; set; }
        public virtual DbSet<Species> Species { get; set; }

        //Breeding
        public virtual DbSet<Breeder> Breeder { get; set; }
        public virtual DbSet<Club> Club { get; set; } // Clubs should be of type 'Club'
        public virtual DbSet<BreederClub> BreederClub { get; set; } // BreederClub should be a junction table for many-to-many
        public virtual DbSet<Stock> Stock { get; set; }
        public virtual DbSet<Line> Line { get; set; }
        public virtual DbSet<Litter> Litter { get; set; }
        public virtual DbSet<Project> Project { get; set; }
        public virtual DbSet<Pairing> Pairing { get; set; }

        //Genetics
        public virtual DbSet<Trait> Trait { get; set; }
        public virtual DbSet<TraitType> TraitType { get; set; }
        public virtual DbSet<AnimalTrait> AnimalTrait { get; set; }
        public virtual DbSet<Chromosome> Chromosomes { get; set; }
        public virtual DbSet<ChromosomePair> ChromosomePairs { get; set; }
        public virtual DbSet<Gene> Genes { get; set; }
        public virtual DbSet<Allele> Alleles { get; set; }
        public virtual DbSet<Genotype> Genotypes { get; set; }
        public virtual DbSet<BreedingCalculation> BreedingCalculations { get; set; }
        public virtual DbSet<PossibleOffspring> PossibleOffspring { get; set; }
        public virtual DbSet<GenericGenotype> GenericGenotype { get; set; }
        public DbContextOptions<RatAppDbContext> Options { get; internal set; }

        //Health
        //public DbSet<HealthRecord> HealthRecord { get; set; }

        //Requests TODO
        //public DbSet<Models.Requests.LoginRequest> LoginRequest { get; set; }
        //public DbSet<UpdateCredentialsRequest> UpdateCredentialsRequest { get; set; }
        //public DbSet<LoginResponse> LoginResponse { get; set; }

        // Constructor for Dependency Injection (recommended)
        public RatAppDbContext(DbContextOptions<RatAppDbContext> options) : base(options)
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
            ConfigureLineage(modelBuilder);

            ConfigureAnimalTrait(modelBuilder);
            ConfigureChromosome(modelBuilder);
            ConfigureChromosomePair(modelBuilder);
            ConfigureGene(modelBuilder);
            ConfigureAllele(modelBuilder);
            ConfigureGenotype(modelBuilder);
            ConfigureBreedingCalculation(modelBuilder);
            ConfigurePossibleOffspring(modelBuilder);
            ConfigureGenericGenotype(modelBuilder);

            ConfigureAnimalImage(modelBuilder);
        }
        private void ConfigureAnimalImage(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AnimalImage>(entity =>
            {
                // Primary key configuration
                entity.HasKey(ai => ai.Id);

                // Relationship configuration: AnimalImage -> Animal
                entity.HasOne(ai => ai.Animal)
                      .WithMany(a => a.AdditionalImages) // Assuming Animal has a collection of AnimalImages
                      .HasForeignKey(ai => ai.AnimalId)
                      .OnDelete(DeleteBehavior.Cascade); // Adjust behavior as needed

                // Unique constraint: SpeciesId and Number
                entity.HasIndex(ai => new { ai.AnimalId, ai.CreatedOn })
                      .IsUnique();

                // Property configurations
                entity.Property(ai => ai.ImageUrl)
                      .IsRequired()
                      .HasMaxLength(2048); // Assuming URL maximum length

                entity.Property(ai => ai.CreatedOn)
                      .IsRequired();

                entity.Property(ai => ai.LastUpdated)
                      .IsRequired();
            });
        }

        private void ConfigureChromosome(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chromosome>()
                .HasOne(c => c.Species)
                .WithMany()
                .HasForeignKey(c => c.SpeciesId)
                .OnDelete(DeleteBehavior.NoAction);

            // Update the unique index to include Arm, Region, and Band
            modelBuilder.Entity<Chromosome>()
                .HasIndex(c => new { c.SpeciesId, c.Number, c.Arm, c.Region, c.Band })
                .IsUnique();
        }

        private void ConfigureChromosomePair(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChromosomePair>()
                .HasOne(cp => cp.MaternalChromosome)
                .WithMany(c => c.MaternalPairs)
                .HasForeignKey(cp => cp.MaternalChromosomeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ChromosomePair>()
                .HasOne(cp => cp.PaternalChromosome)
                .WithMany(c => c.PaternalPairs)
                .HasForeignKey(cp => cp.PaternalChromosomeId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private void ConfigureGene(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Gene>()
                .HasOne(g => g.ChromosomePair)
                .WithMany(cp => cp.Genes)
                .HasForeignKey(g => g.ChromosomePairId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Gene>()
                .HasIndex(g => new { g.ChromosomePairId, g.Position })
                .IsUnique();
        }

        private void ConfigureAllele(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Allele>()
                .HasOne(a => a.Gene)
                .WithMany(g => g.Alleles)
                .HasForeignKey(a => a.GeneId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Allele>()
                .HasIndex(a => new { a.GeneId, a.Symbol })
                .IsUnique();
        }

        private void ConfigureGenotype(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Genotype>()
                .HasOne(g => g.Animal)
                .WithMany(a => a.Genotypes)
                .HasForeignKey(g => g.AnimalId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Genotype>()
                .HasOne(g => g.ChromosomePair)
                .WithMany(cp => cp.Genotypes)
                .HasForeignKey(g => g.ChromosomePairId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Genotype>()
                .HasIndex(g => new { g.AnimalId, g.ChromosomePairId })
                .IsUnique();
        }

        private void ConfigureGenericGenotype(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GenericGenotype>(entity =>
            {
                // Set primary key
                entity.HasKey(gg => gg.GenotypeId);

                // Define relationships and navigation properties
                entity.HasOne(gg => gg.ChromosomePair)
                      .WithMany(cp => cp.GenericGenotype)
                      .HasForeignKey(gg => gg.ChromosomePairId)
                      .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete

                entity.HasOne(gg => gg.Trait)
                      .WithMany(t => t.GenericGenotype)
                      .HasForeignKey(gg => gg.TraitId)
                      .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete

                // Define unique constraints if needed
                entity.HasIndex(gg => new { gg.ChromosomePairId, gg.TraitId })
                      .IsUnique();

                // Configure created and updated timestamps to default to the current date
                entity.Property(gg => gg.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(gg => gg.UpdatedAt)
                      .HasDefaultValueSql("GETDATE()");
            });
        }


        private void ConfigureBreedingCalculation(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BreedingCalculation>()
                .HasOne(bc => bc.Pairing)
                .WithMany()
                .HasForeignKey(bc => bc.PairingId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private void ConfigurePossibleOffspring(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PossibleOffspring>()
                .HasOne(po => po.BreedingCalculation)
                .WithMany(bc => bc.PossibleOffspring)
                .HasForeignKey(po => po.CalculationId)
                .OnDelete(DeleteBehavior.Cascade);
        }


        private void ConfigureAnimalTrait(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AnimalTrait>()
                .HasOne(at => at.Animal)
                .WithMany(a => a.Traits)  // Define the inverse navigation property on Animal
                .HasForeignKey(at => at.AnimalId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AnimalTrait>()
                .HasOne(at => at.Trait)
                .WithMany(t => t.AnimalTraits)  // Define the inverse navigation property on Trait
                .HasForeignKey(at => at.TraitId)
                .OnDelete(DeleteBehavior.NoAction);
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

        //lineage
        private void ConfigureLineage(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Lineage>()
                .HasOne(l => l.Animal)
                .WithMany() // Or appropriate navigation property in Animal
                .HasForeignKey(l => l.AnimalId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Lineage>()
                .HasOne(l => l.Ancestor)
                .WithMany() // Or appropriate navigation property in Animal
                .HasForeignKey(l => l.AncestorId)
                .OnDelete(DeleteBehavior.NoAction);
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
                .Property(i => i.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Individual>()
                .Property(i => i.LastName)
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
                .Property(i => i.City)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Individual>()
                .Property(i => i.State)
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

            // Define `DateOfDeath` as an optional (nullable) property
            modelBuilder.Entity<Animal>()
                .Property(a => a.imageUrl)
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
                .WithMany()                 // A Stock can have many Lines
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

            // Adding the SpeciesId relationship
            modelBuilder.Entity<Trait>()
               .HasOne(t => t.Species)  // Navigation property, referencing the Species entity
               .WithMany() // No navigation property back to Trait, but you can add one if needed
               .HasForeignKey(t => t.SpeciesID) // Foreign key in Trait pointing to Species
               .IsRequired(true);  // Making SpeciesId required, ensuring a trait is always linked to a species
        }
    }
}
