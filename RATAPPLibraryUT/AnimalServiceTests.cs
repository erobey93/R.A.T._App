using Microsoft.Extensions.Logging;
using Moq;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Services;
using Xunit;

namespace RATAPPLibraryUT
{
    public class AnimalServiceTests : TestBase
    {
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly AnimalService _animalService;

        public AnimalServiceTests()
        {
            _contextFactory = new RatAppDbContextFactory();
            _animalService = new AnimalService(_contextFactory);
        }

        [Fact]
        public async Task GetAllAnimals_ReturnsAllAnimals_WhenNoFiltersApplied()
        {
            // Act
            var animals = await _animalService.GetAllAnimalsAsync();

            // Assert
            Assert.NotNull(animals);
        }

        [Theory]
        [InlineData("Rat")]
        [InlineData("Mouse")]
        public async Task GetAllAnimals_FiltersCorrectly_WhenSpeciesProvided(string species)
        {
            // Act
            var animals = await _animalService.GetAllAnimalsAsync(species: species);

            // Assert
            Assert.All(animals, animal => 
                Assert.Equal(species, animal.species, ignoreCase: true));
        }

        [Theory]
        [InlineData("Male")]
        [InlineData("Female")]
        public async Task GetAllAnimals_FiltersCorrectly_WhenSexProvided(string sex)
        {
            // Act
            var animals = await _animalService.GetAllAnimalsAsync(sex: sex);

            // Assert
            Assert.All(animals, animal => 
                Assert.Equal(sex, animal.sex, ignoreCase: true));
        }

        [Fact]
        public async Task GetAllAnimals_FiltersCorrectly_WhenSearchTermProvided()
        {
            // Arrange
            string searchTerm = "test";

            // Act
            var animals = await _animalService.GetAllAnimalsAsync(searchTerm: searchTerm);

            // Assert
            Assert.All(animals, animal =>
                Assert.True(
                    animal.name.ToLower().Contains(searchTerm) ||
                    animal.Id.ToString().Contains(searchTerm)
                ));
        }

        [Fact]
        public async Task GetAnimalById_ReturnsCorrectAnimal_WhenValidIdProvided()
        {
            // Arrange
            int testId = 1; // Assuming this ID exists in test database

            // Act
            var animal = await _animalService.GetAnimalByIdAsync(testId);

            // Assert
            Assert.NotNull(animal);
            Assert.Equal(testId, animal.Id);
        }

        [Fact]
        public async Task GetAnimalById_ThrowsException_WhenInvalidIdProvided()
        {
            // Arrange
            int invalidId = -1;

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _animalService.GetAnimalByIdAsync(invalidId));
        }

        [Theory]
        [InlineData("Rat", "Male")]
        [InlineData("Mouse", "Female")]
        public async Task GetAllAnimals_FiltersCorrectly_WhenBothSpeciesAndSexProvided(string species, string sex)
        {
            // Act
            var animals = await _animalService.GetAllAnimalsAsync(species: species, sex: sex);

            // Assert
            Assert.All(animals, animal =>
            {
                Assert.Equal(species, animal.species, ignoreCase: true);
                Assert.Equal(sex, animal.sex, ignoreCase: true);
            });
        }
    }
}
