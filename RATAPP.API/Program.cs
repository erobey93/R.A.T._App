using Microsoft.OpenApi.Models;
using RATAPP.API.Middleware;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var config = builder.Configuration.GetSection("Swagger").Get<SwaggerConfig>();
    c.SwaggerDoc(config?.Version ?? "v1", new OpenApiInfo
    {
        Title = config?.Title ?? "RATAPP API",
        Description = config?.Description ?? "API for managing rat/mouse breeding and genetics",
        Version = config?.Version ?? "v1"
    });
});

// TODO: Replace with your actual connection string in appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configure Database
builder.Services.AddSingleton<RatAppDbContextFactory>(sp => new RatAppDbContextFactory(connectionString!));
builder.Services.AddScoped(sp => sp.GetRequiredService<RatAppDbContextFactory>().CreateContext());

// Register Services
builder.Services.AddScoped<AnimalService>();
// Add other services as needed

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RATAPP API v1");
    });
}

// Use custom error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();

public class SwaggerConfig
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
}
