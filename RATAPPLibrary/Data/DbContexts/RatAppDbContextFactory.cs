using Microsoft.EntityFrameworkCore;

namespace RATAPPLibrary.Data.DbContexts
{
    public class RatAppDbContextFactory
    {
        private readonly string _connectionString;

        public RatAppDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public RatAppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RatAppDbContext>()
                .UseSqlServer(_connectionString)
                .Options;
            return new RatAppDbContext(options);
        }
    }
}
