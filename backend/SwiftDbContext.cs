using Microsoft.EntityFrameworkCore;

namespace SwiftDashboard;

public class SwiftDbContext(DbContextOptions<SwiftDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Info> Info { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique constraint on Year and Month for Invoices
        modelBuilder.Entity<Invoice>()
            .HasIndex(i => new { i.Year, i.Month })
            .IsUnique();
    }
}