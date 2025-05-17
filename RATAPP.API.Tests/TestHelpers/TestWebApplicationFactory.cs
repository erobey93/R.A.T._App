using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Services;

namespace RATAPP.API.Tests.TestHelpers
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContextFactory registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(RatAppDbContextFactory));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add test DbContextFactory
                services.AddSingleton<RatAppDbContextFactory>(new RatAppDbContextFactory());

                // Configure test services
                services.AddScoped<AnimalService>();
            });
        }
    }
}
