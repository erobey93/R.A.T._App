using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RATAPP.API.Controllers;
using RATAPP.API.Models;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Services;
using Xunit;

namespace RATAPP.API.Tests.Controllers
{
    public class AnimalControllerTests
    {
        private readonly Mock<AnimalService> _mockAnimalService;
        private readonly Mock<ILogger<AnimalController>> _mockLogger;
        private readonly AnimalController _controller;

        public AnimalControllerTests()
        {
            _mockAnimalService = new Mock<AnimalService>(null);
            _mockLogger = new Mock<ILogger<AnimalController>>();
            _controller = new AnimalController(_mockAnimalService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAnimals_ReturnsOkResult_WithAnimals()
        {
            // Arrange
            var expectedAnimals = new[]
            {
                new AnimalDto { Id = 1, name = "Test1", species = "Rat" },
                new AnimalDto { Id = 2, name = "Test2", species = "Mouse" }
            };
            _mockAnimalService.Setup(s => s.GetAllAnimalsAsync(null, null, null))
                .ReturnsAsync(expectedAnimals);

            // Act
            var result = await _controller.GetAnimals();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<AnimalDto[]>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(expectedAnimals, response.Data);
        }

        [Fact]
        public async Task GetAnimals_WithFilters_ReturnsFilteredResults()
        {
            // Arrange
            var species = "Rat";
            var sex = "Male";
            var searchTerm = "test";
            var expectedAnimals = new[]
            {
                new AnimalDto { Id = 1, name = "Test1", species = "Rat", sex = "Male" }
            };

            _mockAnimalService.Setup(s => s.GetAllAnimalsAsync(species, sex, searchTerm))
                .ReturnsAsync(expectedAnimals);

            // Act
            var result = await _controller.GetAnimals(species, sex, searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<AnimalDto[]>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(expectedAnimals, response.Data);
        }

        [Fact]
        public async Task GetAnimal_ReturnsOkResult_WhenAnimalExists()
        {
            // Arrange
            var id = 1;
            var expectedAnimal = new AnimalDto { Id = id, name = "Test", species = "Rat" };
            _mockAnimalService.Setup(s => s.GetAnimalByIdAsync(id))
                .ReturnsAsync(expectedAnimal);

            // Act
            var result = await _controller.GetAnimal(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<AnimalDto>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(expectedAnimal, response.Data);
        }

        [Fact]
        public async Task GetAnimal_ReturnsNotFound_WhenAnimalDoesNotExist()
        {
            // Arrange
            var id = 999;
            _mockAnimalService.Setup(s => s.GetAnimalByIdAsync(id))
                .ThrowsAsync(new KeyNotFoundException($"Animal with ID {id} not found."));

            // Act
            var result = await _controller.GetAnimal(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(notFoundResult.Value);
            Assert.False(response.Success);
            Assert.Contains($"Animal with ID {id} not found", response.Message);
        }

        [Fact]
        public async Task CreateAnimal_ReturnsCreatedResult_WhenValid()
        {
            // Arrange
            var animalDto = new AnimalDto { name = "Test", species = "Rat" };
            var createdAnimal = new Animal { Id = 1, Name = "Test" };
            _mockAnimalService.Setup(s => s.CreateAnimalAsync(animalDto))
                .ReturnsAsync(createdAnimal);

            // Act
            var result = await _controller.CreateAnimal(animalDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var response = Assert.IsType<ApiResponse<Animal>>(createdResult.Value);
            Assert.True(response.Success);
            Assert.Equal(createdAnimal, response.Data);
            Assert.Equal("GetAnimal", createdResult.ActionName);
            Assert.Equal(createdAnimal.Id, createdResult.RouteValues["id"]);
        }

        [Fact]
        public async Task UpdateAnimal_ReturnsOkResult_WhenValid()
        {
            // Arrange
            var id = 1;
            var animalDto = new AnimalDto { Id = id, name = "Updated", species = "Rat" };
            var updatedAnimal = new Animal { Id = id, Name = "Updated" };
            _mockAnimalService.Setup(s => s.UpdateAnimalAsync(animalDto))
                .ReturnsAsync(updatedAnimal);

            // Act
            var result = await _controller.UpdateAnimal(id, animalDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<Animal>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(updatedAnimal, response.Data);
        }

        [Fact]
        public async Task UpdateAnimal_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var id = 1;
            var animalDto = new AnimalDto { Id = 2, name = "Test" };

            // Act
            var result = await _controller.UpdateAnimal(id, animalDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
            Assert.False(response.Success);
            Assert.Contains("ID in URL must match ID in request body", response.Message);
        }

        [Fact]
        public async Task DeleteAnimal_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var id = 1;
            _mockAnimalService.Setup(s => s.DeleteAnimalByIdAsync(id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteAnimal(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAnimal_ReturnsNotFound_WhenAnimalDoesNotExist()
        {
            // Arrange
            var id = 999;
            _mockAnimalService.Setup(s => s.DeleteAnimalByIdAsync(id))
                .ThrowsAsync(new KeyNotFoundException($"Animal with ID {id} not found."));

            // Act
            var result = await _controller.DeleteAnimal(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ApiResponse<object>>(notFoundResult.Value);
            Assert.False(response.Success);
            Assert.Contains($"Animal with ID {id} not found", response.Message);
        }
    }
}
