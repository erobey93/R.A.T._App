using Microsoft.AspNetCore.Mvc;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Services;
using RATAPP_WEB.Server.Controllers;
using Moq;
using Xunit;

namespace RATAPPLibraryUT
{
    public class AnimalControllerTests : TestBase
    {
        private readonly Mock<AnimalService> _mockAnimalService;
        private readonly AnimalController _controller;

        public AnimalControllerTests()
        {
            _mockAnimalService = new Mock<AnimalService>(null);
            _controller = new AnimalController(_mockAnimalService.Object);
        }

        [Fact]
        public async Task GetAnimals_ReturnsOkResult_WithAnimals()
        {
            // Arrange
            var expectedAnimals = new AnimalDto[]
            {
                new AnimalDto
                {
                    Id = 1,
                    name = "Test Animal 1",
                    sex = "Male",
                    species = "Rat",
                    Line = "1",
                    breeder = "Test Breeder"
                },
                new AnimalDto
                {
                    Id = 2,
                    name = "Test Animal 2",
                    sex = "Female",
                    species = "Rat",
                    Line = "1",
                    breeder = "Test Breeder"
                }
            };

            _mockAnimalService.Setup(service => service.GetAllAnimalsAsync())
                .ReturnsAsync(expectedAnimals);

            // Act
            var result = await _controller.GetAnimals();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<AnimalDto[]>(okResult.Value);
            Assert.Equal(expectedAnimals.Length, returnValue.Length);
        }

        [Fact]
        public async Task GetAnimals_ReturnsNotFound_WhenNoAnimals()
        {
            // Arrange
            _mockAnimalService.Setup(service => service.GetAllAnimalsAsync())
                .ReturnsAsync((AnimalDto[])null);

            // Act
            var result = await _controller.GetAnimals();

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAnimals_ReturnsBadRequest_WhenServiceThrows()
        {
            // Arrange
            _mockAnimalService.Setup(service => service.GetAllAnimalsAsync())
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act
            var result = await _controller.GetAnimals();

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}
