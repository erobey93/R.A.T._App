using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPPLibrary.Services.Genetics;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Ancestry;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;

namespace RATAPPLibraryUT
{
    [TestClass]
    public class BreedingCalculationServiceTests : TestBase
    {
        private BreedingCalculationService _breedingService;

        [TestInitialize]
        public void Setup()
        {
            _breedingService = new BreedingCalculationService(_contextFactory);
        }

        [TestMethod]
        public async Task CalculateInbreedingCoefficient_NoCommonAncestors_ReturnsZero()
        {
            // Arrange
            var dam = new Animal { Id = 1, Name = "Dam" };
            var sire = new Animal { Id = 2, Name = "Sire" };
            var damParent1 = new Animal { Id = 3, Name = "Dam Parent 1" };
            var damParent2 = new Animal { Id = 4, Name = "Dam Parent 2" };
            var sireParent1 = new Animal { Id = 5, Name = "Sire Parent 1" };
            var sireParent2 = new Animal { Id = 6, Name = "Sire Parent 2" };

            using (var context = _contextFactory.CreateDbContext())
            {
                context.Animals.AddRange(dam, sire, damParent1, damParent2, sireParent1, sireParent2);
                await context.SaveChangesAsync();

                // Add lineage connections
                context.Lineages.AddRange(
                    new Lineage { AnimalId = dam.Id, AncestorId = damParent1.Id, Generation = 1, Sequence = 1, RelationshipType = "Maternal" },
                    new Lineage { AnimalId = dam.Id, AncestorId = damParent2.Id, Generation = 1, Sequence = 2, RelationshipType = "Paternal" },
                    new Lineage { AnimalId = sire.Id, AncestorId = sireParent1.Id, Generation = 1, Sequence = 1, RelationshipType = "Maternal" },
                    new Lineage { AnimalId = sire.Id, AncestorId = sireParent2.Id, Generation = 1, Sequence = 2, RelationshipType = "Paternal" }
                );
                await context.SaveChangesAsync();
            }

            // Act
            var coefficient = await _breedingService.CalculateInbreedingCoefficientAsync(dam.Id, sire.Id);

            // Assert
            Assert.AreEqual(0, coefficient);
        }

        [TestMethod]
        public async Task CalculateInbreedingCoefficient_WithCommonAncestor_ReturnsCorrectValue()
        {
            // Arrange
            var dam = new Animal { Id = 1, Name = "Dam" };
            var sire = new Animal { Id = 2, Name = "Sire" };
            var commonAncestor = new Animal { Id = 3, Name = "Common Ancestor" };
            var damParent = new Animal { Id = 4, Name = "Dam Parent" };
            var sireParent = new Animal { Id = 5, Name = "Sire Parent" };

            using (var context = _contextFactory.CreateDbContext())
            {
                context.Animals.AddRange(dam, sire, commonAncestor, damParent, sireParent);
                await context.SaveChangesAsync();

                // Add lineage connections
                // Dam's lineage: Dam -> DamParent -> CommonAncestor (2 generations)
                context.Lineages.AddRange(
                    new Lineage { AnimalId = dam.Id, AncestorId = damParent.Id, Generation = 1, Sequence = 1, RelationshipType = "Maternal" },
                    new Lineage { AnimalId = dam.Id, AncestorId = commonAncestor.Id, Generation = 2, Sequence = 1, RelationshipType = "Maternal" }
                );

                // Sire's lineage: Sire -> SireParent -> CommonAncestor (2 generations)
                context.Lineages.AddRange(
                    new Lineage { AnimalId = sire.Id, AncestorId = sireParent.Id, Generation = 1, Sequence = 1, RelationshipType = "Maternal" },
                    new Lineage { AnimalId = sire.Id, AncestorId = commonAncestor.Id, Generation = 2, Sequence = 1, RelationshipType = "Maternal" }
                );

                await context.SaveChangesAsync();
            }

            // Act
            var coefficient = await _breedingService.CalculateInbreedingCoefficientAsync(dam.Id, sire.Id);

            // Assert
            // Expected value: (0.5)^(2+2+1) = (0.5)^5 = 0.03125
            Assert.AreEqual(0.03125, coefficient, 0.00001);
        }

        [TestMethod]
        public async Task CalculateInbreedingCoefficient_WithMultipleCommonAncestors_ReturnsCorrectValue()
        {
            // Arrange
            var dam = new Animal { Id = 1, Name = "Dam" };
            var sire = new Animal { Id = 2, Name = "Sire" };
            var commonAncestor1 = new Animal { Id = 3, Name = "Common Ancestor 1" };
            var commonAncestor2 = new Animal { Id = 4, Name = "Common Ancestor 2" };

            using (var context = _contextFactory.CreateDbContext())
            {
                context.Animals.AddRange(dam, sire, commonAncestor1, commonAncestor2);
                await context.SaveChangesAsync();

                // Add lineage connections
                // Dam's lineage to both common ancestors
                context.Lineages.AddRange(
                    new Lineage { AnimalId = dam.Id, AncestorId = commonAncestor1.Id, Generation = 1, Sequence = 1, RelationshipType = "Maternal" },
                    new Lineage { AnimalId = dam.Id, AncestorId = commonAncestor2.Id, Generation = 2, Sequence = 1, RelationshipType = "Maternal" }
                );

                // Sire's lineage to both common ancestors
                context.Lineages.AddRange(
                    new Lineage { AnimalId = sire.Id, AncestorId = commonAncestor1.Id, Generation = 2, Sequence = 1, RelationshipType = "Maternal" },
                    new Lineage { AnimalId = sire.Id, AncestorId = commonAncestor2.Id, Generation = 1, Sequence = 1, RelationshipType = "Maternal" }
                );

                await context.SaveChangesAsync();
            }

            // Act
            var coefficient = await _breedingService.CalculateInbreedingCoefficientAsync(dam.Id, sire.Id);

            // Assert
            // Expected value: (0.5)^(1+2+1) + (0.5)^(2+1+1) = 0.125 + 0.0625 = 0.1875
            Assert.AreEqual(0.1875, coefficient, 0.00001);
        }
    }
}
