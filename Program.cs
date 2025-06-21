using Microsoft.EntityFrameworkCore;
using ProductCatalogClean.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Database Context with environment variable support
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("PostgreSQLConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

Console.WriteLine($"🐘 Using PostgreSQL Connection: {(Environment.GetEnvironmentVariable("DATABASE_URL") != null ? "Production" : "Development")}");

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Test PostgreSQL connection
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Test connection
        var canConnect = await context.Database.CanConnectAsync();
        if (canConnect)
        {
            Console.WriteLine("✅ Connected to PostgreSQL database successfully!");

            // Count products
            var productCount = await context.Products.CountAsync();
            Console.WriteLine($"📊 Found {productCount} products in database");

            // If no products, add sample data
            if (productCount == 0)
            {
                Console.WriteLine("🔄 No products found, creating sample data...");
                await context.Database.EnsureCreatedAsync();
                // The migration will handle table creation
            }
        }
        else
        {
            Console.WriteLine("❌ Cannot connect to PostgreSQL database");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database connection error: {ex.Message}");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Root endpoint
app.MapGet("/", () => "🚀 ProductCatalog API is running! Visit /swagger for documentation.");

app.Run();