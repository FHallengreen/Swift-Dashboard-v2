using Microsoft.EntityFrameworkCore;
using SwiftDashboard;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

try
{
    // Use hardcoded MySQL version instead of AutoDetect to avoid connection issues during startup
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register IHttpClientFactory for HolidaysController
builder.Services.AddHttpClient();

// Register IMemoryCache for HolidaysController
builder.Services.AddMemoryCache();

// Add SignalR services
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("FrontendDevPolicy");

app.MapControllers();
app.MapHub<InvoiceUpdateHub>("/api/invoiceHub");

// Add health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "2.0"
}));

await SeedData(app);

app.Run();

static async Task SeedData(IApplicationBuilder app)
{
    using var scope = app.ApplicationServices.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SwiftDbContext>();

    // Only seed Info if not exists
    if (!await dbContext.Info.AnyAsync(i => i.Id == 1))
    {
        dbContext.Info.Add(new Info { Id = 1, Text = "Welcome to Swift Display Dashboard." });
        await dbContext.SaveChangesAsync();
    }
}