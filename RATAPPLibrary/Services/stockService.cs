using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Breeding;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RATAPPLibrary.Services
{
    public class StockService
    {
        private readonly RatAppDbContext _context;

        public StockService(RatAppDbContext context)
        {
            _context = context;
        }

        // Get stock by ID
        public async Task<Stock> GetStockAsync_ById(int id)
        {
            var stock = await _context.Stock
                .Include(s => s.Breeder) // Include related Breeder data
                .Include(s => s.Species) // Include related Species data
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stock == null)
            {
                throw new Exception("Stock not found");
            }

            return stock;
        }

        // Get all stocks
        public async Task<List<Stock>> GetAllStocksAsync()
        {
            return await _context.Stock
                .Include(s => s.Breeder)
                .Include(s => s.Species)
                .ToListAsync();
        }

        // Create a new stock
        public async Task<Stock> CreateStockAsync(Stock stock)
        {
            _context.Stock.Add(stock);
            await _context.SaveChangesAsync();
            return stock;
        }

        // Update an existing stock
        public async Task<Stock> UpdateStockAsync(Stock stock)
        {
            var existingStock = await _context.Stock.FindAsync(stock.Id);

            if (existingStock == null)
            {
                throw new Exception("Stock not found");
            }

            _context.Entry(existingStock).CurrentValues.SetValues(stock);
            await _context.SaveChangesAsync();

            return stock;
        }

        // Delete a stock by ID
        public async Task DeleteStockAsync(int id)
        {
            var stock = await _context.Stock.FindAsync(id);

            if (stock == null)
            {
                throw new Exception("Stock not found");
            }

            _context.Stock.Remove(stock);
            await _context.SaveChangesAsync();
        }
    }
}