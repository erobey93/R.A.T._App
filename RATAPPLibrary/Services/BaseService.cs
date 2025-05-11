using System;
using System.Threading.Tasks;
using RATAPPLibrary.Data.DbContexts;

namespace RATAPPLibrary.Services
{
    //base service serves as a central location for all services to manage their context as opposed to passing the same context around which leads to data races and other issues 
    public abstract class BaseService
    {
        private readonly RatAppDbContextFactory _contextFactory;

        protected BaseService(RatAppDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        protected async Task<T> ExecuteInContextAsync<T>(Func<RatAppDbContext, Task<T>> operation)
        {
            using var context = _contextFactory.CreateContext();
            return await operation(context);
        }

        protected async Task ExecuteInContextAsync(Func<RatAppDbContext, Task> operation)
        {
            using var context = _contextFactory.CreateContext();
            await operation(context);
        }

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
