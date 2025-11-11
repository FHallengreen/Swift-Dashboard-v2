using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftDashboard.Data;

namespace SwiftDashboard.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Test to prevent seed data from running
        builder.UseEnvironment("Test");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Load test configuration from Tests folder
            config.AddJsonFile("Tests/appsettings.Test.json", optional: false);
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<SwiftDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remove DbContext itself
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(SwiftDbContext));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Add test database configuration
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<SwiftDbContext>(options =>
            {
                options.UseMySql(connectionString, 
                    new MySqlServerVersion(new Version(8, 0, 43)));
            });

            // Build the service provider to ensure database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<SwiftDbContext>();

            // Ensure clean database for tests - delete any existing data first
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                // Clean up test database after all tests
                using var scope = Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SwiftDbContext>();
                db.Database.EnsureDeleted();
            }
            catch (ObjectDisposedException)
            {
                // Services already disposed - this is fine
            }
        }
        base.Dispose(disposing);
    }
}
