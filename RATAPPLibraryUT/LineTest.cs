using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.DbContexts;
using Microsoft.Data.SqlClient;

namespace RATAPPLibraryUT
{
    public class LineServiceTests
    {
        private readonly RatAppDbContext _dbContext;
        private readonly LineService _lineService;

        public LineServiceTests()
        {
            // Step 1: Create an in-memory database for testing
            var options = new DbContextOptionsBuilder<RatAppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase") // You can name this whatever you want
                .Options;

            // Step 2: Create the DbContext instance with the in-memory options
            _dbContext = new RatAppDbContext(options);

            // Step 3: Initialize your service with the real DbContext (not a mock)
            _lineService = new LineService(_dbContext);
        }

        [Fact]
        public async Task GetOrCreateLineAsync_LineExists_HappyPath()
        {
            // Arrange
            var name = "Fancy";
            int stockId = 1;
            var existingLine = new Line { Id = 1, StockId = stockId, Name = name };

            // Add the existing line to the in-memory database
            _dbContext.Line.Add(existingLine);
            await _dbContext.SaveChangesAsync(); // Make sure to persist the changes to the in-memory database

            // Act
            var result = await _lineService.GetOrCreateLineAsync_ByName(name);

            // Assert
            // Verify that the line was created
            Assert.NotNull(result); // Ensure a new line is returned
            Assert.Equal(name, result.Name); // Check that the new line has the expected name
            Assert.Equal(stockId, result.StockId); // Check the stock ID or any other relevant fields

            // Verify that the new line was actually added to the in-memory database
            var lineInDb = await _dbContext.Line.FirstOrDefaultAsync(l => l.Name == name);
            Assert.NotNull(lineInDb); // Ensure that the line was added to the in-memory database
        }

        [Fact]
        public async Task GetOrCreateLineAsync_LineDoesNotExist_CreatesNewLineWithAppropriateStockId()
        {
            // Arrange
            var name = "Standard"; // The name of the line you're testing for
            var expectedStockId = 1; // Expected stock ID based on species or business logic

            // Act: Verify the line does not exist initially
            var existingLine = await _dbContext.Line.FirstOrDefaultAsync(l => l.Name == name);
            Assert.Null(existingLine); // Ensure the line doesn't exist before creating it

            // Act: Call the method to create the line
            var result = await _lineService.GetOrCreateLineAsync_ByName(name);

            // Assert: Ensure the result is not null (a new line was created)
            Assert.NotNull(result);
            Assert.Equal(name, result.Name); // Check that the line created has the correct name
            Assert.Equal(expectedStockId, result.StockId); // Check that the stock ID is appropriate based on species

            // Act: Verify that the line is now in the in-memory database
            var lineInDb = await _dbContext.Line.FirstOrDefaultAsync(l => l.Name == name);
            Assert.NotNull(lineInDb); // Ensure that the line was added to the database

            // Assert that the stock ID was set correctly for the new line
            Assert.Equal(expectedStockId, lineInDb.StockId);
        }


        [Fact]
        public async Task Test_GetLineForAnimal_HappyPath()
        {
            // Arrange
            var lineId = 1;
            var stockId = 1;
            var line = new Line { Id = lineId, StockId = stockId, Name = "Standard" };
            var animal = new Animal { Id = 100, LineId = lineId, Sex = "Male", Name = "Name" };

            // Add the line and animal to the in-memory database
            await _dbContext.Line.AddAsync(line);
            await _dbContext.Animal.AddAsync(animal);
            await _dbContext.SaveChangesAsync(); // Save changes to the in-memory database

            // Act
            var result = await _lineService.GetLineAsync_ById(animal.LineId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(lineId, result.Id);
            Assert.Equal(line.Name, result.Name);
        }

        //Should attempt to retrieve line object for an animal that doesn't exist
        //expect back a custom exception that animal doesn't exist
        [Fact]
        public async Task Test_GetLineForAnimal_AnimalDoesntExist()
        {
            // Arrange
            var lineId = 1;
            var stockId = 1;
            var line = new Line { Id = lineId, StockId = stockId, Name = "Standard" };

            // Add line to the in-memory database, but no animal exists
            _dbContext.Line.Add(line);
            await _dbContext.SaveChangesAsync();

            // Act & Assert: Try to retrieve a line for a non-existent animal
            var nonExistentAnimalId = 999;

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _lineService.GetLineAsync_ById(nonExistentAnimalId));

            Assert.Equal($"Animal with ID {nonExistentAnimalId} does not exist.", exception.Message);
        }

        //Should attempt to retrieve line object for an animal that doesn't exist
        [Fact]
        public async Task Test_GetLineForAnimal_SQLError()
        {
            // Arrange
            var lineId = 1;
            var stockId = 1;
            var line = new Line { Id = lineId, StockId = stockId, Name = "Standard" };

            // Create a list with one valid line
            var lines = new List<Line> { line }.AsQueryable();
            _dbContext.Line.AddRange(lines);
            await _dbContext.SaveChangesAsync();

            // Attempting to fetch an invalid or non-existent animal
            var nonExistentAnimalId = 999; // An ID that does not exist in the database

            // No animal with the non-existent ID, so the result should be null
            var animals = new List<Animal>().AsQueryable(); // Empty list of animals
            _dbContext.Animal.AddRange(animals);
            await _dbContext.SaveChangesAsync();

            // Act & Assert: Try to retrieve a line for a non-existent animal
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _lineService.GetLineAsync_ById(nonExistentAnimalId));

            Assert.Equal($"Animal with ID {nonExistentAnimalId} does not exist.", exception.Message);
        }

    }
}
