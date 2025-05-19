var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register RatAppDbContextFactory
builder.Services.AddSingleton<RatAppDbContextFactory>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "R.A.T. App API",
        Version = "v1",
        Description = "API for accessing animal data in the R.A.T. App"
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp",
        builder => builder
            .WithOrigins("http://localhost:5173") // Vue.js default dev server
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowWebApp");

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
