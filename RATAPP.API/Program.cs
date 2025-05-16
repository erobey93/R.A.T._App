using Microsoft.OpenApi.Models;
using RATAPP.API.Middleware;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "R.A.T. App API",
        Version = "v1",
        Description = "REST API for accessing R.A.T. App data",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@ratapp.com"
        }
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",    // React default
                "http://localhost:8080",    // Vue.js default
                "http://localhost:4200"     // Angular default
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Disposition"); // For file downloads
    });
});

// Register services
builder.Services.AddSingleton<RatAppDbContextFactory>();
builder.Services.AddScoped<AnimalService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "R.A.T. App API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the root
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseErrorHandling(); // Our custom error handling middleware
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make the Program class visible to integration tests
public partial class Program { }
