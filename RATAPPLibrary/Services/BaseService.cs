using System;
using System.Threading.Tasks;
using RATAPPLibrary.Data.DbContexts;

namespace RATAPPLibrary.Services
{
    /// <summary>

    /// Base class for all services that need database access. Provides methods for executing

    /// database operations with proper context management and transaction support.

    ///

    /// Usage:

    /// 1. Inherit from this class in your service:

    ///    public class AnimalService : BaseService

    ///

    /// 2. Use ExecuteInContextAsync for simple operations:

    ///    public async Task<Animal> GetAnimalAsync(int id)

    ///    {

    ///        return await ExecuteInContextAsync(async context =>

    ///            await context.Animals.FindAsync(id));

    ///    }

    ///

    /// 3. Use ExecuteInTransactionAsync for operations that need transaction support:

    ///    public async Task<bool> CreateAnimalAsync(Animal animal)

    ///    {

    ///        return await ExecuteInTransactionAsync(async context =>

    ///        {

    ///            context.Animals.Add(animal);

    ///            await context.SaveChangesAsync();

    ///            return true;

    ///        });

    ///    }

    /// </summary>
    public abstract class BaseService
    {
        private readonly RatAppDbContextFactory _contextFactory;

        protected BaseService(RatAppDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>

        /// Executes a database operation with automatic context management.

        /// The context is automatically created and disposed.

        /// </summary>

        /// <typeparam name="T">The return type of the operation</typeparam>

        /// <param name="operation">The async operation to execute</param>

        /// <returns>The result of the operation</returns>
        protected async Task<T> ExecuteInContextAsync<T>(Func<RatAppDbContext, Task<T>> operation)
        {
            using var context = _contextFactory.CreateContext();
            return await operation(context);
        }

        /// <summary>

        /// Executes a void database operation with automatic context management.

        /// </summary>

        /// <param name="operation">The async operation to execute</param>
        protected async Task ExecuteInContextAsync(Func<RatAppDbContext, Task> operation)
        {
            using var context = _contextFactory.CreateContext();
            await operation(context);
        }

        /// <summary>

        /// Executes a database operation within a transaction.

        /// If any part of the operation fails, the entire transaction is rolled back.

        ///

        /// Use this for operations that need to maintain data consistency, such as:

        /// - Creating related records

        /// - Updating multiple records that must succeed or fail together

        /// - Any operation where partial success is not acceptable

        /// </summary>

        /// <typeparam name="T">The return type of the operation</typeparam>

        /// <param name="operation">The async operation to execute in a transaction</param>

        /// <returns>The result of the operation</returns>
        protected async Task<T> ExecuteInTransactionAsync<T>(Func<RatAppDbContext, Task<T>> operation)
        {
            using var context = _contextFactory.CreateContext();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var result = await operation(context);
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>

        /// Executes a void database operation within a transaction.

        /// </summary>

        /// <param name="operation">The async operation to execute in a transaction</param>
        protected async Task ExecuteInTransactionAsync(Func<RatAppDbContext, Task> operation)
        {
            using var context = _contextFactory.CreateContext();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await operation(context);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
