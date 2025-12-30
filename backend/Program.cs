using Microsoft.EntityFrameworkCore;
using SwiftDashboard.Data;
using SwiftDashboard.Hubs;
using SwiftDashboard.Interfaces;
using SwiftDashboard.Models;
using SwiftDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Build connection string from environment variables if not provided
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    var dbHost = builder.Configuration["DB_HOST"];
    var dbPort = builder.Configuration["DB_PORT"];
    var dbName = builder.Configuration["MYSQL_DATABASE"];
    var dbUser = builder.Configuration["DB_USER"];
    var dbPassword = builder.Configuration["MYSQL_ROOT_PASSWORD"];
    
    connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};User={dbUser};Password={dbPassword};Connection Timeout=30;Command Timeout=60;";
}

if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 43));
        
        builder.Services.AddDbContext<SwiftDbContext>(options =>
            options.UseMySql(connectionString, serverVersion, mysqlOptions =>
            {
                mysqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DbContext Configuration Failed: {ex}");
        throw;
    }
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient();

builder.Services.AddMemoryCache();

// Register application services
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IInfoService, InfoService>();

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddScoped<IHolidayService, NoOpHolidayService>();
}
else
{
    builder.Services.AddScoped<IHolidayService, HolidayService>();
}



builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDevPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://dashboard.swiftmarine.dk",
                "http://dashboard.swiftmarine.dk")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("FrontendDevPolicy");

app.MapControllers();
app.MapHub<InvoiceUpdateHub>("/api/invoiceHub");
app.MapHub<InfoUpdateHub>("/api/infoHub");

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "2.0"
}));

// Only seed data in non-test environments
var environment = app.Environment;
if (!environment.EnvironmentName.Contains("Test", StringComparison.OrdinalIgnoreCase))
{
    await SeedData(app);
}

app.Run();

static async Task SeedData(IApplicationBuilder app)
{
    using var scope = app.ApplicationServices.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SwiftDbContext>();

    // Only seed Info if not exists
    if (!await dbContext.Info.AnyAsync(i => i.Id == 1))
    {
        dbContext.Info.Add(new SwiftDashboard.Models.Info { Id = 1, Text = "Welcome to Swift Display Dashboard." });
        await dbContext.SaveChangesAsync();
    }
}

// Make Program class accessible for integration tests
public partial class Program { }