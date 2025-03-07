using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace RATAPPLibraryUT
{
    [TestClass]
    public class TraitTests
    {
        private TraitService _traitService;
        private RatAppDbContext _context;
        private DbContextOptions<RatAppDbContext> _options;

        [TestInitialize]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<RatAppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new RatAppDbContext(_options);
            _traitService = new TraitService(_context);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            SeedData().Wait();
        }

        private async Task SeedData()
        {
            var species = new Species { Id = 1, CommonName = "Rat", ScientificName = "Rattus norvegicus" };
            _context.Species.Add(species);
            await _context.SaveChangesAsync();

            var traitTypeColor = new TraitType { Id = 1, Name = "Color", Description = "Coat Color" };
            var traitTypePattern = new TraitType { Id = 2, Name = "Pattern", Description = "Coat Pattern" };
            _context.TraitType.AddRange(traitTypeColor, traitTypePattern);
            await _context.SaveChangesAsync();

            var traitBlack = new Trait { Id = 1, CommonName = "Black", TraitTypeId = 1, SpeciesID = 1, Genotype = "bb" };
            var traitAgouti = new Trait { Id = 2, CommonName = "Agouti", TraitTypeId = 1, SpeciesID = 1, Genotype = "AA" };
            var traitSelf = new Trait { Id = 3, CommonName = "Self", TraitTypeId = 2, SpeciesID = 1, Genotype = "ss" };
            _context.Trait.AddRange(traitBlack, traitAgouti, traitSelf);
            await _context.SaveChangesAsync();

            var animal = new Animal { Id = 1, Sex = "Male", Name = "animalName" };
            _context.Animal.Add(animal);
            await _context.SaveChangesAsync();

            var animalTrait1 = new AnimalTrait { AnimalId = 1, TraitId = 1 };
            _context.AnimalTrait.Add(animalTrait1);
            await _context.SaveChangesAsync();
        }

        [TestMethod]
        public async Task Test_GetTraitTypeIdByNameAsync_SuccessfullyGetsId()
        {
            var id = await _traitService.GetTraitTypeIdByNameAsync("Color");
            Assert.AreEqual(1, id);
        }

        [TestMethod]
        public async Task Test_GetTraitTypeIdByNameAsync_ThrowsExceptionWhenNotFound()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await _traitService.GetTraitTypeIdByNameAsync("NonExistent");
            });
        }

        [TestMethod]
        public async Task Test_GetTraitByNameAsync_SuccessfullyGetsTrait()
        {
            var trait = await _traitService.GetTraitByNameAsync("Black");
            Assert.IsNotNull(trait);
            Assert.AreEqual("Black", trait.CommonName);
        }

        [TestMethod]
        public async Task Test_GetTraitByNameAsync_ThrowsExceptionWhenNotFound()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await _traitService.GetTraitByNameAsync("NonExistent");
            });
        }

        [TestMethod]
        public async Task Test_GetTraitObjectsByTypeAndSpeciesAsync_SuccessfullyGetsTraits()
        {
            var traits = await _traitService.GetTraitObjectsByTypeAndSpeciesAsync("Color", "Rat");
            Assert.IsNotNull(traits);
            Assert.AreEqual(2, traits.Count);
        }

        [TestMethod]
        public async Task Test_GetTraitsByTypeAndSpeciesAsync_SuccessfullyGetsTraitNames()
        {
            var traitNames = await _traitService.GetTraitsByTypeAndSpeciesAsync("Color", "Rat");
            Assert.IsNotNull(traitNames);
            Assert.IsTrue(traitNames.Contains("Black"));
            Assert.IsTrue(traitNames.Contains("Agouti"));
        }

        [TestMethod]
        public async Task Test_GetTraitsBySpeciesAsync_SuccessfullyGetsTraits()
        {
            var traits = await _traitService.GetTraitsBySpeciesAsync("Rat");
            Assert.IsNotNull(traits);
            Assert.AreEqual(3, traits.Count());
        }

        [TestMethod]
        public async Task Test_CreateTraitTypeAsync_SuccessfullyCreatesTraitType()
        {
            var newTraitType = await _traitService.CreateTraitTypeAsync("NewType", "New Description");
            Assert.IsNotNull(newTraitType);
            Assert.AreEqual("NewType", newTraitType.Name);
        }

        [TestMethod]
        public async Task Test_CreateTraitAsync_SuccessfullyCreatesTrait()
        {
            var newTrait = await _traitService.CreateTraitAsync("NewTrait", 1, "Rat", "New Description");
            Assert.IsNotNull(newTrait);
            Assert.AreEqual("NewTrait", newTrait.CommonName);
        }

        [TestMethod]
        public async Task Test_GetAllTraitsAsync_SuccessfullyGetsAllTraits()
        {
            var traits = await _traitService.GetAllTraitsAsync();
            Assert.IsNotNull(traits);
            Assert.IsTrue(traits.Count() >= 3);
        }

        [TestMethod]
        public async Task Test_GetTraitsByTraitTypeIdAsync_SuccessfullyGetsTraitsByType()
        {
            var traits = await _traitService.GetTraitsByTraitTypeIdAsync(1);
            Assert.IsNotNull(traits);
            Assert.AreEqual(2, traits.Count());
        }

        [TestMethod]
        public async Task Test_GetAllTraitTypesAsync_SuccessfullyGetsAllTraitTypes()
        {
            var traitTypes = await _traitService.GetAllTraitTypesAsync();
            Assert.IsNotNull(traitTypes);
            Assert.IsTrue(traitTypes.Count() >= 2);
        }

        [TestMethod]
        public async Task Test_GetTraitByIdAsync_SuccessfullyGetsTraitById()
        {
            var trait = await _traitService.GetTraitByIdAsync(1);
            Assert.IsNotNull(trait);
            Assert.AreEqual("Black", trait.CommonName);
        }

        [TestMethod]
        public async Task Test_GetColorTraitsForSingleAnimal_SuccessfullyGetsTraitsForAnimal()//TODO what's going on here? Not passing 
        {
            var traits = await _traitService.GetColorTraitsForSingleAnimal(1, "Color", "Rat");
            Assert.IsNotNull(traits);
            Assert.AreEqual(1, traits.Count());
            Assert.AreEqual("Black", traits.First().CommonName);
        }

        [TestMethod]
        public async Task Test_CreateAnimalTraitAsync_SuccessfullyCreatesAnimalTrait()
        {
            var newAnimalTrait = await _traitService.CreateAnimalTraitAsync(2, 1);
            Assert.IsNotNull(newAnimalTrait);
            Assert.AreEqual(2, newAnimalTrait.TraitId);
            Assert.AreEqual(1, newAnimalTrait.AnimalId);
        }

        [TestMethod]
        public async Task Test_GetTraitByAnimalIdAndTraitIdAsync_SuccessfullyGetsTraitByAnimalAndTraitId()
        {
            var trait = await _traitService.GetTraitByAnimalIdAndTraitIdAsync(1, 1);
            Assert.IsNotNull(trait);
            Assert.AreEqual("Black", trait.CommonName);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}