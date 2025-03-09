using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Services;
using RATAPPLibrary.Data.DbContexts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RATAPPLibraryUT
{
    [TestClass]
    public class LineServiceTests
    {
        private LineService _lineService;
        private RatAppDbContext _context;
        private DbContextOptions<RatAppDbContext> _options;

        [TestInitialize]
        public void Setup()
        {
            // Configure In-Memory Database for testing
            _options = new DbContextOptionsBuilder<RatAppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Initialize DbContext with In-Memory Database
            _context = new RatAppDbContext(_options);

            // Initialize LineService with the DbContext
            _lineService = new LineService(_context);

            // Clear the database before each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [TestMethod]
        public async Task Test_AddLine_SuccessfullyAddsLine()
        {
            var lineId = 1;
            var stockId = 2;
            var lineName = "TODO";

            var line = new Line { StockId = stockId, Id = lineId, Name = lineName };

            await _lineService.GetOrCreateLineAsync_ByName(lineId);

            var addedLine = _context.Line.FirstOrDefault(l => l.Id == lineId);

            Assert.IsNotNull(addedLine);
            Assert.AreEqual(lineName, addedLine.Name);
            Assert.AreEqual(stockId, addedLine.StockId);
        }

        [TestMethod]
        public async Task Test_GetLine_SuccessfullyGetsLine()
        {
            var lineId = 1;
            var stockId = 2;
            var lineName = "TODO";

            var line = new Line { StockId = stockId, Id = lineId, Name = lineName };
            _context.Line.Add(line);
            await _context.SaveChangesAsync();

            var addedLine = await _lineService.GetLineAsync_ById(lineId);

            Assert.IsNotNull(addedLine);
            Assert.AreEqual(lineName, addedLine.Name);
            Assert.AreEqual(stockId, addedLine.StockId);
        }

        [TestMethod]
        public async Task Test_GetLine_ThrowsExceptionWhenLineNotFound()
        {
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await _lineService.GetLineAsync_ById(999);
            });
        }

        [TestMethod]
        public async Task Test_GetOrCreateLineAsync_ByName_CreatesNewIfNotFound()
        {
            var lineId = 1;
            var stockId = 2;
            var lineName = "TODO";

            var newLine = await _lineService.GetOrCreateLineAsync_ByName(lineId);

            Assert.IsNotNull(newLine);
            Assert.AreEqual(lineName, newLine.Name);
            Assert.AreEqual(stockId, newLine.StockId);

            var addedLine = _context.Line.FirstOrDefault(l => l.Id == lineId);
            Assert.IsNotNull(addedLine);
            Assert.AreEqual(lineName, addedLine.Name);
            Assert.AreEqual(stockId, addedLine.StockId);
        }

        [TestMethod]
        public async Task Test_GetOrCreateLineAsync_ByName_ReturnsExistingIfFound()
        {
            var lineId = 1;
            var stockId = 2;
            var lineName = "Existing Line";

            var existingLine = new Line { StockId = stockId, Id = lineId, Name = lineName };
            _context.Line.Add(existingLine);
            await _context.SaveChangesAsync();

            var retrievedLine = await _lineService.GetOrCreateLineAsync_ByName(lineId);

            Assert.IsNotNull(retrievedLine);
            Assert.AreEqual(lineName, retrievedLine.Name);
            Assert.AreEqual(stockId, retrievedLine.StockId);
        }

        [TestMethod]
        public async Task Test_GetOrCreateLineAsync_ByName_MultipleCalls()
        {
            var lineId = 1;
            var stockId = 2;
            var lineName = "TODO";

            var firstCall = await _lineService.GetOrCreateLineAsync_ByName(lineId);
            var secondCall = await _lineService.GetOrCreateLineAsync_ByName(lineId);

            Assert.IsNotNull(firstCall);
            Assert.IsNotNull(secondCall);
            Assert.AreEqual(firstCall.Id, secondCall.Id);
            Assert.AreEqual(lineName, firstCall.Name);
            Assert.AreEqual(stockId, firstCall.StockId);
            Assert.AreEqual(lineName, secondCall.Name);
            Assert.AreEqual(stockId, secondCall.StockId);

            var count = _context.Line.Count(l => l.Id == lineId);
            Assert.AreEqual(1, count);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}