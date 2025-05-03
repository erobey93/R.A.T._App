using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Animal_Management;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Services;
using System;
using System.Threading.Tasks;
using System.Linq;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using RATAPPLibrary.Data.Models;

namespace RATAPPLibraryUT
{
    [TestClass]
    public class BreedingServiceTests
    {
        private BreedingService _breedingService;
        private RatAppDbContext _context;
        private DbContextOptions<RatAppDbContext> _options;

        [TestInitialize]
        public void Setup()
        {
            // Configure In-Memory Database for testing
            _options = new DbContextOptionsBuilder<RatAppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Initialize DbContext with In-Memory Database
            _context = new RatAppDbContext(_options);

            // Initialize BreedingService with the DbContext
            _breedingService = new BreedingService(_context);

            // Clear the database before each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Seed test data
            SeedData().Wait();
        }

        private async Task SeedData()
        {
            var species = new Species { Id = 1, CommonName = "Rat", ScientificName = "Rattus norvegicus" };
            _context.Species.Add(species);

            var newStock = new Stock
            {
                Id = 1,
                BreederId = 1, // Assuming a Breeder with Id 1 exists
                SpeciesId = 1, // Assuming a Species with Id 2 exists
                Description = "Test Stock"
            };
            _context.Stock.Add(newStock);

            var line = new Line { Id = 1, Name = "Test", StockId = 1 };
            _context.Line.Add(line);

            var project = new Project { Id = 1, LineId = 1, Name = "testProject" }; 
            _context.Project.Add(project);

            var male = new Animal
            {
                Id = 1,
                Name = "Male1",
                Sex = "Male",
                DateOfBirth = DateTime.Now.AddMonths(-6),
                StockId = 1
            };

            var female = new Animal
            {
                Id = 2,
                Name = "Female1",
                Sex = "Female",
                DateOfBirth = DateTime.Now.AddMonths(-5),
                StockId = 1
            };

            _context.Animal.AddRange(male, female);

            var pairing = new Pairing
            {
                Id = 1,
                pairingId = "1", 
                SireId = 1,
                DamId = 2,
                PairingStartDate = DateTime.Now.AddDays(-30),
                PairingEndDate = DateTime.Now.AddDays(10),
                ProjectId = 1,
            };

            var pairing2 = new Pairing
            {
                Id = 2,
                pairingId = "12",
                SireId = 1,
                DamId = 2,
                PairingStartDate = DateTime.Now.AddDays(-30),
                ProjectId = 1,
            };

            var pairing3 = new Pairing
            {
                Id = 3,
                pairingId = "123",
                SireId = 1,
                DamId = 2,
                ProjectId = 1,
            };

            _context.Pairing.AddRange(pairing, pairing2, pairing3);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// This method tests the CreatePairingAsync method.
        /// It validates that:
        /// 1. A new pairing can be created with valid animals
        /// 2. The pairing is properly stored in the database
        /// 3. The pairing has correct relationships to male and female animals
        /// </summary>
        [TestMethod]
        public async Task CreatePairing_ShouldCreateNewPairing()
        {
            // Arrange
            var newPairing = new Pairing
            {
                SireId = 1,
                DamId = 2,
                PairingStartDate = DateTime.Now,
                PairingEndDate = DateTime.Now.AddDays(21),
            };

            // Act
            await _breedingService.CreatePairingAsync(newPairing.pairingId,newPairing.DamId, newPairing.SireId, newPairing.ProjectId, newPairing.PairingStartDate, newPairing.PairingEndDate);

            List<Pairing> pairings = await _breedingService.GetAllPairingsAsync();

            var createdPairing = pairings[0];

            // Assert
            Assert.IsNotNull(createdPairing);
            //Assert.IsTrue(createdPairing); FIXME

            //to check the below variables I need to go get the pairing, it doesn't make sense to return it in this method TODO 
            //Assert.IsTrue(createdPairing.Id > 0);
            //Assert.AreEqual(1, createdPairing.MaleId);
            //Assert.AreEqual(2, createdPairing.FemaleId);
            //Assert.AreEqual("Active", createdPairing.Status);
        }

        /// <summary>
        /// This method tests the GetAllPairingsByAnimalIdAsync method.
        /// It validates that:
        /// 1. An existing pairing can be retrieved by its related animal ID
        /// 2. The retrieved pairing has correct properties and relationships
        /// </summary>
        [TestMethod]
        public async Task GetPairingByAnimalId_ShouldReturnCorrectPairing()
        {
            // Act
            var pairing = await _breedingService.GetAllPairingsByAnimalIdAsync(1);

            // Assert
            Assert.IsNotNull(pairing);
            Assert.AreEqual(1, pairing[0].SireId);
            Assert.AreEqual(2, pairing[0].DamId);
            Assert.AreEqual("1", pairing[0].pairingId);
            Assert.AreEqual(1, pairing[0].Id);
        }

        //TODO check this because I defintitely had pairing get functionality implemented 

        /// <summary>
        /// This method tests the GetPairingByLineIdAsync method.
        /// It validates that:
        /// 1. An existing pairing can be retrieved by its Line ID
        /// 2. The retrieved pairing has correct properties and relationships
        /// </summary>
        [TestMethod]
        public async Task GetPairingByLineId_ShouldReturnCorrectPairing()
        {
            // Act
            var pairing = await _breedingService.GetAllPairingsByLineIdAsync(1);

            // Assert
            Assert.IsNotNull(pairing);
            Assert.AreEqual(1, pairing[0].SireId);
            Assert.AreEqual(2, pairing[0].DamId);
            Assert.AreEqual("1", pairing[0].pairingId);
            Assert.AreEqual(1, pairing[0].Id);
        }

        /// <summary>
        /// This method tests the GetPairingBySpeciesAsync method.
        /// It validates that:
        /// 1. An existing pairing can be retrieved by its common Species name (not ID)
        /// 2. The retrieved pairing has correct properties and relationships
        /// </summary>
        [TestMethod]
        public async Task GetPairingBySpeciesId_ShouldReturnCorrectPairing()
        {
            // Act
            var pairings = await _breedingService.GetAllPairingsBySpeciesAsync("Rat");

            // Assert
            Assert.IsNotNull(pairings);
            Assert.AreEqual(3, pairings.Count); 
            Assert.AreEqual(1, pairings[0].SireId);
            Assert.AreEqual(2, pairings[0].DamId);
            Assert.AreEqual("1", pairings[0].pairingId);
            Assert.AreEqual(1, pairings[0].Id);
        }

        //TODO don't have update pairing yet 
        /// <summary>
        /// This method tests the UpdatePairingAsync method.
        /// It validates that:
        /// 1. An existing pairing can be updated
        /// 2. The changes are properly saved to the database
        /// 3. Only the specified properties are modified
        /// </summary>
        [TestMethod]
        public async Task UpdatePairing_ShouldUpdateExistingPairing()
        {
            //TODO
            throw new NotImplementedException();
            // Arrange
            //var pairing = await _breedingService.GetAllPairingsByAnimalIdAsync(1);
            //pairing[0].PairingStartDate = DateTime.Now;

            //// Act
            //var updatedPairing = await _breedingService.UpdatePairingAsync(pairing);

            //// Assert
            //Assert.IsNotNull(updatedPairing);
            //Assert.AreEqual("Completed", updatedPairing.Status);
            //Assert.AreEqual("Updated pairing notes", updatedPairing.Notes);
            //Assert.AreEqual(1, updatedPairing.MaleId); // Original value should remain unchanged
        }

        //TODO need create litter (this is weird, I thought I had this method?)
        /// <summary>
        /// This method tests the CreateLitterAsync method.
        /// It validates that:
        /// 1. A new litter can be created for an existing pairing
        /// 2. The litter is properly stored in the database
        /// 3. The litter has correct relationships to its pairing
        /// </summary>
        [TestMethod]
        public async Task CreateLitter_ShouldCreateNewLitter()
        {
            // Arrange
            var newLitter = new Litter
            {
                LitterId = "1",
                PairId = 1,
                Name = "testLitter",
                DateOfBirth = DateTime.Now,
                NumPups = 8,
                Notes = "Healthy litter"
            };

        //    // Act
           var createdLitter = await _breedingService.CreateLitterAsync(newLitter);

            // Assert
            Assert.IsNotNull(createdLitter);
            Assert.IsTrue(createdLitter);
            //Assert.IsTrue(createdLitter.Id > 0);
            //Assert.AreEqual(1, createdLitter.PairingId);
            //Assert.AreEqual(8, createdLitter.NumberBorn);
            //Assert.AreEqual(8, createdLitter.NumberSurvived);
        }

        //TODO don't have get active pairings yet 
        /// <summary>
        /// This method tests the GetActivePairingsAsync method.
        /// It validates that:
        /// 1. All active pairings can be retrieved
        /// 2. Only pairings that don't have an end date set (so they're active)
        /// 3. The pairings contain valid data
        /// </summary>
        [TestMethod]
        public async Task GetActivePairings_ShouldReturnOnlyActivePairings()
        {
            // Act
            var activePairings = await _breedingService.GetAllActivePairingsAsync();

            // Assert
            Assert.IsNotNull(activePairings);
            Assert.AreEqual(1, activePairings.Count());
            Assert.IsTrue(activePairings.All(p => p.PairingEndDate == null));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
