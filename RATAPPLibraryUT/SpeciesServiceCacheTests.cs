using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace RATAPPLibraryUT
{
    [TestClass]
    public class SpeciesServiceCacheTests
    {
        private SpeciesService _speciesService;
        private RatAppDbContext _context;
        private ICacheService _cache;
        private DbContextOptions<RatAppDbContext> _options;

        [TestInitialize]
        public void Setup()
        {
            // Configure In-Memory Database
            _options = new DbContextOptionsBuilder<RatAppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new RatAppDbContext(_options);
            _cache = new CacheService();
            _speciesService = new SpeciesService(_context); //, _cache is TODO

            // Clear the database
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Seed test data
            SeedData().Wait();
        }

        private async Task SeedData()
        {
            var species1 = new Species { CommonName = "Rat", ScientificName = "Rattus norvegicus" };
            var species2 = new Species { CommonName = "Mouse", ScientificName = "Mus musculus" };

            _context.Species.AddRange(species1, species2);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Tests that GetAllSpeciesAsync returns cached data on subsequent calls
        /// </summary>
        [TestMethod]
        public async Task GetAllSpecies_ReturnsCachedDataOnSubsequentCalls()
        {
            // First call - should hit database
            var firstCall = await _speciesService.GetAllSpeciesAsync();
            var firstCallList = firstCall.ToList();

            // Add a new species directly to database (bypassing service)
            var newSpecies = new Species { CommonName = "Hamster", ScientificName = "Mesocricetus auratus" };
            _context.Species.Add(newSpecies);
            await _context.SaveChangesAsync();

            // Second call - should return cached data (won't include new species)
            var secondCall = await _speciesService.GetAllSpeciesAsync();
            var secondCallList = secondCall.ToList();

            // Verify both calls return same data (cached)
            Assert.AreEqual(firstCallList.Count, secondCallList.Count);
            Assert.AreEqual(2, secondCallList.Count); // Original 2 species, not including new one
        }

        //TODO need species to 
        /// <summary>
        /// Tests that cache is invalidated when species are updated
        /// </summary>
        [TestMethod]
        public async Task UpdateSpecies_InvalidatesCache()
        {
            // First call to cache the data
            var initialSpecies = (await _speciesService.GetAllSpeciesObjectsAsync()).ToList();
            var speciestoUpdate = initialSpecies.First();

            // Update the species
            speciestoUpdate.CommonName = "Updated Name";
            await _speciesService.EditSpeciesAsync(speciestoUpdate.Id,speciestoUpdate.CommonName ,speciestoUpdate.ScientificName);

            // Get species again - should hit database due to cache invalidation
            var updatedSpecies = (await _speciesService.GetAllSpeciesAsync()).ToList();

            // Verify the update is reflected
            Assert.AreEqual("Updated Name", updatedSpecies.First());
        }

        //TODO don't have delete yet 
        /// <summary>
        /// Tests that cache is invalidated when species are deleted
        /// </summary>
        //[TestMethod]
        //public async Task DeleteSpecies_InvalidatesCache()
        //{
        //    // First call to cache the data
        //    var initialSpecies = (await _speciesService.GetAllSpeciesAsync()).ToList();
        //    var initialCount = initialSpecies.Count;

        //    // Delete a species that has no animals (won't throw exception)
        //    await _speciesService.DeleteSpeciesAsync(initialSpecies.First().Id);

        //    // Get species again - should hit database due to cache invalidation
        //    var remainingSpecies = (await _speciesService.GetAllSpeciesAsync()).ToList();

        //    // Verify one species was removed
        //    Assert.AreEqual(initialCount - 1, remainingSpecies.Count);
        //}

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
