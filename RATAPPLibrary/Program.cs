using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Services;

namespace RATAPPLibrary
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Manually set the environment if needed
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development"); //FIXME: Set to production after testing 

            var _env = builder.Environment;
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Register the RatAppDbContextFactory as a singleton
            builder.Services.AddSingleton(provider => new RatAppDbContextFactory(connectionString));

            // Optionally, register RatAppDbContext itself for scoped dependency injection
            builder.Services.AddScoped(provider =>
            {
                var factory = provider.GetRequiredService<RatAppDbContextFactory>();
                return factory.CreateContext();
            });

            // Register other services
            builder.Services.AddScoped<PasswordHashing>();
            builder.Services.AddScoped<LoginService>();
            builder.Services.AddScoped<AnimalService>();
            builder.Services.AddScoped<SpeciesService>();
            builder.Services.AddScoped<LineService>();

            // Configure controllers and Swagger
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();

            // Build and configure the application
            var app = builder.Build();

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
