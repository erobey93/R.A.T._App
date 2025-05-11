using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Breeding;
using Microsoft.EntityFrameworkCore;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// Service class for managing Line entities in the database.
    /// Inherits from BaseService to leverage database context management and transaction support.
    /// </summary>
    public class LineService : BaseService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineService"/> class.
        /// </summary>
        /// <param name="contextFactory">The factory used to create <see cref="RatAppDbContext"/> instances.</param>
        public LineService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// Retrieves a Line by its name. If a Line with the given name does not exist, it creates a new one.
        /// </summary>
        /// <param name="lineName">The name of the Line to retrieve or create.</param>
        /// <param name="stockId">The ID of the Stock to which the new Line should be associated if created.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task result contains the retrieved or newly created <see cref="Line"/>.</returns>
        public async Task<Line> GetOrCreateLineAsync_ByName(int lineId)
        {
            return await ExecuteInContextAsync(async context =>
            {
                // Search for a Line with the given name.
                var line = await context.Line.FirstOrDefaultAsync(l => l.Id == lineId);

                // If no Line with the specified name is found, create a new one.
                if (line == null)
                {
                    line = new Line
                    {
                        Name = line.Name,
                        StockId = line.StockId
                    };

                    // Add the new Line to the context.
                    context.Line.Add(line);
                    // Save the changes to the database.
                    await context.SaveChangesAsync();
                }

                // Return the existing or newly created Line.
                return line;
            });
        }

        /// <summary>
        /// Retrieves a Line by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the Line to retrieve.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task result contains the <see cref="Line"/> if found.</returns>
        /// <exception cref="Exception">Thrown if no Line with the specified ID is found.</exception>
        public async Task<Line> GetLineAsync_ById(int id)
        {
            return await ExecuteInContextAsync(async context =>
            {
                // Find the Line with the given ID.
                var line = await context.Line.FindAsync(id);
                // If no Line is found with the specified ID, throw an exception.
                if (line == null)
                {
                    throw new Exception($"Line with ID {id} not found.");
                }
                // Return the found Line.
                return line;
            });
        }

        /// <summary>
        /// Retrieves all Line entities from the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task result contains a <see cref="List{Line}"/> of all Lines.</returns>
        public async Task<List<Line>> GetAllLinesAsync()
        {
            return await ExecuteInContextAsync(async context =>
            {
                // Retrieve all Lines from the database.
                return await context.Line.ToListAsync();
            });
        }

        /// <summary>
        /// Creates a new Line in the database.
        /// </summary>
        /// <param name="line">The <see cref="Line"/> entity to create.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task result contains the newly created <see cref="Line"/>.</returns>
        public async Task<Line> CreateLineAsync(Line line)
        {
            return await ExecuteInTransactionAsync(async context =>
            {
                // Add the new Line to the context.
                context.Line.Add(line);
                // Save the changes to the database within a transaction.
                await context.SaveChangesAsync();
                // Return the newly created Line.
                return line;
            });
        }

        /// <summary>
        /// Updates an existing Line in the database.
        /// </summary>
        /// <param name="line">The <see cref="Line"/> entity with updated values.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task result contains the updated <see cref="Line"/>.</returns>
        /// <exception cref="Exception">Thrown if no Line with the specified ID is found.</exception>
        public async Task<Line> UpdateLineAsync(Line line)
        {
            return await ExecuteInTransactionAsync(async context =>
            {
                // Find the existing Line by its ID.
                var existingLine = await context.Line.FindAsync(line.Id);
                // If no Line is found with the specified ID, throw an exception.
                if (existingLine == null)
                {
                    throw new Exception($"Line with ID {line.Id} not found.");
                }

                // Update the values of the existing Line with the values from the provided Line entity.
                context.Entry(existingLine).CurrentValues.SetValues(line);
                // Save the changes to the database within a transaction.
                await context.SaveChangesAsync();
                // Return the updated Line.
                return existingLine;
            });
        }

        /// <summary>
        /// Deletes a Line from the database based on its ID.
        /// </summary>
        /// <param name="id">The ID of the Line to delete.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown if no Line with the specified ID is found.</exception>
        public async Task DeleteLineAsync(int id)
        {
            await ExecuteInTransactionAsync(async context =>
            {
                // Find the Line to delete by its ID.
                var line = await context.Line.FindAsync(id);
                // If no Line is found with the specified ID, throw an exception.
                if (line == null)
                {
                    throw new Exception($"Line with ID {id} not found.");
                }
                // Remove the Line from the context.
                context.Line.Remove(line);
                // Save the changes to the database within a transaction.
                await context.SaveChangesAsync();
            });
        }
    }
}