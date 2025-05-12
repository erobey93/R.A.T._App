using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Animal_Management;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services.Genetics;
using RATAPPLibrary.Services.Genetics.Interfaces;
using System;
using System.Threading.Tasks;

namespace RATAPPLibraryUT.Genetics.Base
{
    public abstract class GenomicsTestBase
    {
        protected RatAppDbContext _context;
        protected IChromosomeService _chromosomeService;
        protected IGeneService _geneService;
        protected IBreedingCalculationService _breedingCalculationService;

        protected Species _testSpecies;
        protected Animal _testDam;
        protected Animal _testSire;

        [TestInitialize]
        public virtual async Task TestInitialize()
        {
            var options = new DbContextOptionsBuilder<RatAppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new RatAppDbContext(options);

            // Initialize services
            _chromosomeService = new ChromosomeService(_context);
            _geneService = new GeneService(_context);
            _breedingCalculationService = new BreedingCalculationService(_context);

            // Setup basic test data
            await SetupTestDataAsync();
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        protected virtual async Task SetupTestDataAsync()
        {
            // Create test species
            _testSpecies = new Species
            {
                CommonName = "Test Rat",
                ScientificName = "Rattus test"
            };
            _context.Species.Add(_testSpecies);

            // Create test animals
            _testDam = new Animal
            {
                Name = "Test Dam",
                Sex = "Female",
                DateOfBirth = DateTime.UtcNow.AddYears(-1)
            };
            _context.Animal.Add(_testDam);

            _testSire = new Animal
            {
                Name = "Test Sire",
                Sex = "Male",
                DateOfBirth = DateTime.UtcNow.AddYears(-1)
            };
            _context.Animal.Add(_testSire);

            await _context.SaveChangesAsync();
        }

        protected async Task<(Chromosome maternal, Chromosome paternal)> CreateTestChromosomePairAsync()
        {
            var maternal = await _chromosomeService.CreateChromosomeAsync(
                name: "Test Maternal",
                number: 1,
                speciesId: _testSpecies.Id
            );

            var paternal = await _chromosomeService.CreateChromosomeAsync(
                name: "Test Paternal",
                number: 1,
                speciesId: _testSpecies.Id
            );

            return (maternal, paternal);
        }

        protected async Task<Gene> CreateTestGeneAsync(Guid chromosomePairId)
        {
            return await _geneService.CreateGeneAsync(new CreateGeneRequest
            {
                Name = "Test Gene",
                CommonName = "Test Gene Common Name",
                ChromosomePairId = chromosomePairId,
                Position = 1,
                Category = "physical",
                ImpactLevel = "cosmetic",
                ExpressionAge = "birth",
                Penetrance = "complete",
                Expressivity = "consistent",
                RequiresMonitoring = false
            });
        }

        protected async Task<Allele> CreateTestAlleleAsync(Guid geneId, bool isWildType = true)
        {
            return await _geneService.CreateAlleleAsync(new CreateAlleleRequest
            {
                GeneId = geneId,
                Name = isWildType ? "Wild Type" : "Variant",
                Symbol = isWildType ? "+" : "v",
                IsWildType = isWildType,
                Phenotype = isWildType ? "Normal" : "Variant",
                RiskLevel = "none"
            });
        }

        protected async Task<BreedingCalculation> CreateTestBreedingCalculationAsync()
        {
            // Create a test pairing
            var pairing = new Pairing
            {
                DamId = _testDam.Id,
                SireId = _testSire.Id,
                pairingId = "TEST-001",
                CreatedOn = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
            _context.Pairing.Add(pairing);
            await _context.SaveChangesAsync();

            return await _breedingCalculationService.CreateBreedingCalculationAsync((int)pairing.Id);
        }
    }
}
