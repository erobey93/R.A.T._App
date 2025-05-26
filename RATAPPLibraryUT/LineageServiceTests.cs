using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Ancestry;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RATAPPLibraryUT
{
    [TestClass]
    public class LineageServiceTests : TestBase
    {
        private LineageService _lineageService;
        private TraitService _traitService;

        [TestInitialize]
        public void Setup()
        {
            _lineageService = new LineageService(_contextFactory);
            _traitService = new TraitService(_contextFactory);
        }

        [TestMethod]
        public async Task GetAncestorTraits_ReturnsCorrectTraits()
        {
            // Arrange
            using (var context = _contextFactory.CreateContext())
            {
                // Create trait types
                var colorType = new TraitType { Name = "Color", Type = "Physical" };
                var earType = new TraitType { Name = "Ear Type", Type = "Physical" };
                context.TraitType.Add(colorType);
                context.TraitType.Add(earType);
                await context.SaveChangesAsync();

                // Create traits
                var blackTrait = new Trait { CommonName = "Black", TraitTypeId = colorType.Id, Genotype = "N/A", SpeciesID = 1 };
                var standardEarTrait = new Trait { CommonName = "Standard", TraitTypeId = earType.Id, Genotype = "N/A", SpeciesID = 1 };
                context.Trait.Add(blackTrait);
                context.Trait.Add(standardEarTrait);
                await context.SaveChangesAsync();

                // Create animals
                var animal = new Animal { Name = "Test Animal" };
                var dam = new Animal { Name = "Dam" };
                var sire = new Animal { Name = "Sire" };
                context.Animal.Add(animal);
                context.Animal.Add(dam);
                context.Animal.Add(sire);
                await context.SaveChangesAsync();

                // Create lineage connections
                var damLineage = new Lineage
                {
                    AnimalId = animal.Id,
                    AncestorId = dam.Id,
                    Generation = 1,
                    Sequence = 1,
                    RelationshipType = "Maternal"
                };
                var sireLineage = new Lineage
                {
                    AnimalId = animal.Id,
                    AncestorId = sire.Id,
                    Generation = 1,
                    Sequence = 2,
                    RelationshipType = "Paternal"
                };
                context.Lineages.Add(damLineage);
                context.Lineages.Add(sireLineage);
                await context.SaveChangesAsync();

                // Add traits to ancestors
                var damColorTrait = new AnimalTrait { AnimalId = dam.Id, TraitId = blackTrait.Id };
                var damEarTrait = new AnimalTrait { AnimalId = dam.Id, TraitId = standardEarTrait.Id };
                var sireColorTrait = new AnimalTrait { AnimalId = sire.Id, TraitId = blackTrait.Id };
                context.AnimalTrait.Add(damColorTrait);
                context.AnimalTrait.Add(damEarTrait);
                context.AnimalTrait.Add(sireColorTrait);
                await context.SaveChangesAsync();

                // Act
                var result = await _lineageService.GetAncestorTraitsAsync(animal.Id);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count); // Should have traits for both dam and sire

                // Check dam's traits
                var damTraits = result["Maternal (Gen 1)"];
                Assert.IsNotNull(damTraits);
                Assert.IsTrue(damTraits.ContainsKey("Color"));
                Assert.IsTrue(damTraits.ContainsKey("Ear Type"));
                Assert.AreEqual("Black", damTraits["Color"][0]);
                Assert.AreEqual("Standard", damTraits["Ear Type"][0]);

                // Check sire's traits
                var sireTraits = result["Paternal (Gen 1)"];
                Assert.IsNotNull(sireTraits);
                Assert.IsTrue(sireTraits.ContainsKey("Color"));
                Assert.AreEqual("Black", sireTraits["Color"][0]);
            }
        }
    }
}
