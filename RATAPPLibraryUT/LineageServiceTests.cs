using Microsoft.Extensions.Logging;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Ancestry;
using RATAPPLibrary.Services;
using Xunit;

namespace RATAPPLibraryUT
{
    public class LineageServiceTests : TestBase
    {
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly LineageService _lineageService;

        public LineageServiceTests()
        {
            //_contextFactory = new RatAppDbContextFactory();
            _lineageService = new LineageService(_contextFactory);
        }

        [Fact]
        public async Task GetAncestorsByAnimalId_ReturnsCorrectAncestors_WhenValidIdProvided()
        {
            // Arrange
            int animalId = 1234; // Test Dam 1's ID from test data

            // Act
            var ancestors = await _lineageService.GetAncestorsByAnimalId(animalId);

            // Assert
            Assert.NotNull(ancestors);
            Assert.Equal(2, ancestors.Count()); // Should have 2 ancestors (dam2 and sire1)
            
            // Verify the ancestors are correct
            Assert.Contains(ancestors, l => l.AncestorId == 123456 && l.RelationshipType == "Paternal"); // sire1
            Assert.Contains(ancestors, l => l.AncestorId == 12345 && l.RelationshipType == "Maternal"); // dam2
        }

        [Fact]
        public async Task GetAncestorsByAnimalId_ReturnsEmptyList_WhenNoAncestorsExist()
        {
            // Arrange
            int animalId = 123456; // Test Sire 1's ID from test data (has no ancestors in test data)

            // Act
            var ancestors = await _lineageService.GetAncestorsByAnimalId(animalId);

            // Assert
            Assert.NotNull(ancestors);
            Assert.Empty(ancestors);
        }

        [Fact]
        public async Task GetDescendantsByAnimalId_ReturnsCorrectDescendants_WhenValidIdProvided()
        {
            // Arrange
            int animalId = 123456; // Test Sire 1's ID from test data

            // Act
            var descendants = await _lineageService.GetDescendantsByAnimalId(animalId);

            // Assert
            Assert.NotNull(descendants);
            Assert.Single(descendants); // Should have 1 descendant (dam1)
            
            var descendant = descendants.First();
            Assert.Equal(1234, descendant.AnimalId); // dam1's ID
            Assert.Equal("Paternal", descendant.RelationshipType);
        }

        [Fact]
        public async Task GetDescendantsByAnimalId_ReturnsEmptyList_WhenNoDescendantsExist()
        {
            // Arrange
            int animalId = 1234; // Test Dam 1's ID from test data (has no descendants in test data)

            // Act
            var descendants = await _lineageService.GetDescendantsByAnimalId(animalId);

            // Assert
            Assert.NotNull(descendants);
            Assert.Empty(descendants);
        }

        [Fact]
        public async Task GetAncestorsByAnimalId_IncludesAncestorObjects_WhenReturningLineages()
        {
            // Arrange
            int animalId = 1234; // Test Dam 1's ID from test data

            // Act
            var ancestors = await _lineageService.GetAncestorsByAnimalId(animalId);

            // Assert
            Assert.All(ancestors, lineage =>
            {
                Assert.NotNull(lineage.Ancestor);
                Assert.NotNull(lineage.Ancestor.Name);
            });
        }

        [Fact]
        public async Task GetDescendantsByAnimalId_IncludesAnimalObjects_WhenReturningLineages()
        {
            // Arrange
            int animalId = 123456; // Test Sire 1's ID from test data

            // Act
            var descendants = await _lineageService.GetDescendantsByAnimalId(animalId);

            // Assert
            Assert.All(descendants, lineage =>
            {
                Assert.NotNull(lineage.Animal);
                Assert.NotNull(lineage.Animal.Name);
            });
        }
    }
}
