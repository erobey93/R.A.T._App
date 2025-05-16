using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Breeding;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// Service for managing breeding stock information in the R.A.T. App.
    /// Handles the tracking and management of breeding stock records, which represent
    /// distinct populations of animals maintained by breeders.
    /// 
    /// Key Features:
    /// - Stock Management:
    ///   * Create and track breeding populations
    ///   * Associate stocks with breeders and species
    ///   * Maintain stock records and relationships
    /// 
    /// Data Structure:
    /// - Stock records link:
    ///   * Breeder: Who maintains the stock
    ///   * Species: What type of animal
    ///   * Lines: Varieties within the stock
    /// 
    /// Relationships:
    /// - One breeder can maintain multiple stocks
    /// - Each stock belongs to one species
    /// - Multiple lines can come from one stock
    /// 
    /// Known Limitations:
    /// - No validation for stock naming conventions
    /// - No tracking of stock origins or history
    /// - Limited breeder information integration
    /// 
    /// Dependencies:
    /// - Inherits from BaseService for database operations
    /// - Requires RatAppDbContext for data access
    /// </summary>
    public class StockService : BaseService
    {
        public StockService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// Retrieves a stock record by its ID, including related breeder and species data.
        /// 
        /// Includes:
        /// - Breeder information
        /// - Species details
        /// 
        /// Throws:
        /// - Exception if stock not found
        /// </summary>
        /// <param name="id">ID of the stock to retrieve</param>
        /// <returns>Stock object with related data</returns>
        public async Task<Stock> GetStockAsync_ById(int id)
        {
            return await ExecuteInContextAsync(async _context =>
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
            });
        }

        // Get all stocks
        public async Task<List<Stock>> GetAllStocksAsync()
        {
            return await ExecuteInContextAsync(async _context =>
            {
                return await _context.Stock
                .Include(s => s.Breeder)
                .Include(s => s.Species)
                .ToListAsync();
            });
        }

        /// <summary>
        /// Creates a new stock record in the database.
        /// 
        /// Required Stock Information:
        /// - Breeder association
        /// - Species designation
        /// - Stock identification details
        /// 
        /// Note: Ensure all required relationships (Breeder, Species)
        /// are properly set before creating the stock.
        /// 
        /// TODO: Add validation for required relationships
        /// </summary>
        /// <param name="stock">Stock object with all required data</param>
        /// <returns>Created Stock object</returns>
        public async Task<Stock> CreateStockAsync(Stock stock)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                _context.Stock.Add(stock);
                await _context.SaveChangesAsync();
                return stock;
            });
        }

        /// <summary>
        /// Updates an existing stock record.
        /// 
        /// Updateable Fields:
        /// - Stock details
        /// - Breeder association
        /// - Species designation
        /// 
        /// Throws:
        /// - Exception if stock not found
        /// 
        /// Note: This operation replaces all values of the existing stock
        /// with the values from the provided stock object.
        /// </summary>
        /// <param name="stock">Stock object with updated values</param>
        /// <returns>Updated Stock object</returns>
        public async Task<Stock> UpdateStockAsync(Stock stock)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                var existingStock = await _context.Stock.FindAsync(stock.Id);

                if (existingStock == null)
                {
                    throw new Exception("Stock not found");
                }

                _context.Entry(existingStock).CurrentValues.SetValues(stock);
                await _context.SaveChangesAsync();

                return stock;
            });
        }

        /// <summary>
        /// Deletes a stock record from the database.
        /// 
        /// IMPORTANT: This operation should be used with caution as it may affect:
        /// - Associated breeding lines
        /// - Historical breeding records
        /// - Animal lineage tracking
        /// 
        /// TODO: Add validation to prevent deletion of stocks with dependencies
        /// 
        /// Throws:
        /// - Exception if stock not found
        /// </summary>
        /// <param name="id">ID of the stock to delete</param>
        public async Task DeleteStockAsync(int id)
        {
            await ExecuteInContextAsync(async _context =>
            {
                var stock = await _context.Stock.FindAsync(id);

                if (stock == null)
                {
                    throw new Exception("Stock not found");
                }

                _context.Stock.Remove(stock);
                await _context.SaveChangesAsync();
            });
        }
    }
}
