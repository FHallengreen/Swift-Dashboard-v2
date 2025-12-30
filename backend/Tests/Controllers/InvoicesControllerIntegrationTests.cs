using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SwiftDashboard.Data;
using SwiftDashboard.Models;
using SwiftDashboard.Models.DTOs;
using Xunit;

namespace SwiftDashboard.Tests.Controllers;

[Collection("Database collection")]
public class InvoicesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly IServiceScope _scope;
    private readonly SwiftDbContext _dbContext;

    public InvoicesControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<SwiftDbContext>();
    }

    [Fact]
    public async Task GetInvoices_ReturnsOkWithInvoices()
    {
        // Arrange
        _dbContext.Invoices.AddRange(new[]
        {
            new Invoice { Year = 2024, Month = 1, Amount = 1000m },
            new Invoice { Year = 2024, Month = 2, Amount = 1500m }
        });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var response = await _client.GetAsync("/api/invoices", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invoices = await response.Content.ReadFromJsonAsync<List<object>>(cancellationToken: TestContext.Current.CancellationToken);
        invoices.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCurrentMonthInvoice_ReturnsOkWithCurrentMonthData()
    {
        // Act
        var response = await _client.GetAsync("/api/invoices/current", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostInvoice_CreatesNewInvoice()
    {
        // Arrange
        var amount = 2000m;

        // Act
        var response = await _client.PostAsJsonAsync("/api/invoices", amount, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var today = DateTime.Today;
        var savedInvoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Year == today.Year && i.Month == today.Month, cancellationToken: TestContext.Current.CancellationToken);
        savedInvoice.Should().NotBeNull();
        savedInvoice!.Amount.Should().Be(amount);
    }

    [Fact]
    public async Task UpdateInvoice_UpdatesExistingInvoice()
    {
        // Arrange - clear any existing invoice for this period
        var year = 2024;
        var month = 5;
        var existing = await _dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Year == year && i.Month == month, cancellationToken: TestContext.Current.CancellationToken);
        if (existing != null)
        {
            _dbContext.Invoices.Remove(existing);
            await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        
        var invoice = new Invoice { Year = year, Month = month, Amount = 1000m };
        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var updateModel = new UpdateInvoiceModel { Amount = 2500m };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/invoices/{year}/{month}", updateModel, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Reload from database to get fresh data (not cached)
        _dbContext.ChangeTracker.Clear();
        var updatedInvoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Year == year && i.Month == month, cancellationToken: TestContext.Current.CancellationToken);
        updatedInvoice.Should().NotBeNull();
        updatedInvoice!.Amount.Should().Be(2500m);
    }

    public void Dispose()
    {
        // Clean up test data after each test
        _dbContext.Invoices.RemoveRange(_dbContext.Invoices);
        _dbContext.SaveChanges();
        _scope.Dispose();
        _client.Dispose();
    }
}
