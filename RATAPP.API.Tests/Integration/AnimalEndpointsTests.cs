using System.Net;
using System.Net.Http.Json;
using RATAPP.API.Models;
using RATAPP.API.Tests.TestHelpers;
using RATAPPLibrary.Data.Models;
using Xunit;

namespace RATAPP.API.Tests.Integration
{
    public class AnimalEndpointsTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AnimalEndpointsTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetAnimals_ReturnsSuccessResult()
        {
            // Act
            var response = await _client.GetAsync("/api/animal");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AnimalDto[]>>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Theory]
        [InlineData("Rat")]
        [InlineData("Mouse")]
        public async Task GetAnimals_WithSpeciesFilter_ReturnsFilteredResults(string species)
        {
            // Act
            var response = await _client.GetAsync($"/api/animal?species={species}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AnimalDto[]>>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.All(result.Data, animal => Assert.Equal(species, animal.species));
        }

        [Theory]
        [InlineData("Male")]
        [InlineData("Female")]
        public async Task GetAnimals_WithSexFilter_ReturnsFilteredResults(string sex)
        {
            // Act
            var response = await _client.GetAsync($"/api/animal?sex={sex}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AnimalDto[]>>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.All(result.Data, animal => Assert.Equal(sex, animal.sex));
        }

        [Fact]
        public async Task GetAnimal_WithValidId_ReturnsAnimal()
        {
            // Arrange
            var id = 1; // Assuming this ID exists in test database

            // Act
            var response = await _client.GetAsync($"/api/animal/{id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AnimalDto>>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(id, result.Data.Id);
        }

        [Fact]
        public async Task GetAnimal_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var id = 999; // Non-existent ID

            // Act
            var response = await _client.GetAsync($"/api/animal/{id}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains($"Animal with ID {id} not found", result.Message);
        }

        [Fact]
        public async Task CreateAnimal_WithValidData_ReturnsCreatedAnimal()
        {
            // Arrange
            var newAnimal = new AnimalDto
            {
                name = "Test Animal",
                species = "Rat",
                sex = "Male",
                Line = "1", // Assuming line 1 exists
                DateOfBirth = DateTime.UtcNow.AddMonths(-3)
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/animal", newAnimal);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Animal>>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(newAnimal.name, result.Data.Name);
        }

        [Fact]
        public async Task UpdateAnimal_WithValidData_ReturnsUpdatedAnimal()
        {
            // Arrange
            var id = 1; // Assuming this ID exists
            var updateAnimal = new AnimalDto
            {
                Id = id,
                name = "Updated Name",
                species = "Rat",
                sex = "Male",
                Line = "1"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/animal/{id}", updateAnimal);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Animal>>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(updateAnimal.name, result.Data.Name);
        }

        [Fact]
        public async Task UpdateAnimal_WithIdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var id = 1;
            var updateAnimal = new AnimalDto
            {
                Id = 2, // Mismatched ID
                name = "Updated Name",
                species = "Rat"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/animal/{id}", updateAnimal);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("ID in URL must match ID in request body", result.Message);
        }

        [Fact]
        public async Task DeleteAnimal_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var id = 1; // Assuming this ID exists

            // Act
            var response = await _client.DeleteAsync($"/api/animal/{id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteAnimal_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var id = 999; // Non-existent ID

            // Act
            var response = await _client.DeleteAsync($"/api/animal/{id}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains($"Animal with ID {id} not found", result.Message);
        }
    }
}
