using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Services;

using System.Text; 

namespace RATAPPLibrary
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);

            // Manually set the environment if needed
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development"); //FIXME set to to production after testing 

            // After setting the environment, you can configure services
            var _env = builder.Environment;

            if (_env.IsDevelopment())  // For development or tests, use InMemory
            {
                //builder.Services.AddDbContext<RatAppDbContext>(options =>
                //    options.UseInMemoryDatabase("TestDatabase"));
                builder.Services.AddDbContext<RatAppDbContext>(options =>
                   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            }
            else  // For production, use SQL Server
            {
                builder.Services.AddDbContext<RatAppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            }


        // Register PasswordHashing as a service for dependency injection
        builder.Services.AddScoped<PasswordHashing>();

            // Register services
            builder.Services.AddScoped<LoginService>();
            builder.Services.AddScoped<AnimalService>();
            builder.Services.AddScoped<SpeciesService>();
            builder.Services.AddScoped<LineService>();

            // Add DbContext and other services

            //builder.Services.AddDbContext<RatAppDbContext>(options =>
            //   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
                    };
                });

            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}