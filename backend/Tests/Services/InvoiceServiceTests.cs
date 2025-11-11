using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using SwiftDashboard.Data;
using SwiftDashboard.Hubs;
using SwiftDashboard.Models;
using SwiftDashboard.Services;
using Xunit;

namespace SwiftDashboard.Tests.Services;

public class InvoiceServiceTests : IDisposable
{
    private readonly SwiftDbContext _dbContext;
    private readonly Mock<IHubContext<InvoiceUpdateHub>> _mockHubContext;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        var options = new DbContextOptionsBuilder<SwiftDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new SwiftDbContext(options);
        _mockHubContext = new Mock<IHubContext<InvoiceUpdateHub>>();
        
        // Setup SignalR hub mock
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);
        mockClients.Setup(x => x.All).Returns(mockClientProxy.Object);

        _service = new InvoiceService(_dbContext, _mockHubContext.Object);
    }

    [Fact]
    public async Task CreateOrUpdateInvoiceAsync_CreatesNewInvoice_WhenNoneExistsForCurrentMonth()
    {
        // Arrange
        var amount = 1000.50m;
        var today = DateTime.Today;

        // Act
        var result = await _service.CreateOrUpdateInvoiceAsync(amount);

        // Assert
        result.Should().NotBeNull();
        result.Year.Should().Be(today.Year);
        result.Month.Should().Be(today.Month);
        result.Amount.Should().Be(amount);

        var savedInvoice = await _dbContext.Invoices.FirstOrDefaultAsync();
        savedInvoice.Should().NotBeNull();
        savedInvoice!.Amount.Should().Be(amount);
    }

    [Fact]
    public async Task CreateOrUpdateInvoiceAsync_UpdatesExistingInvoice_WhenOneExistsForCurrentMonth()
    {
        // Arrange
        var today = DateTime.Today;
        var existingInvoice = new Invoice
        {
            Year = today.Year,
            Month = today.Month,
            Amount = 500m
        };
        _dbContext.Invoices.Add(existingInvoice);
        await _dbContext.SaveChangesAsync();

        var newAmount = 1500m;

        // Act
        var result = await _service.CreateOrUpdateInvoiceAsync(newAmount);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(newAmount);

        var invoiceCount = await _dbContext.Invoices.CountAsync();
        invoiceCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllInvoicesAsync_ReturnsAllInvoices()
    {
        // Arrange
        var invoices = new List<Invoice>
        {
            new() { Year = 2024, Month = 1, Amount = 1000m },
            new() { Year = 2024, Month = 2, Amount = 1200m },
            new() { Year = 2024, Month = 3, Amount = 900m }
        };
        _dbContext.Invoices.AddRange(invoices);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllInvoicesAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(invoices);
    }

    [Fact]
    public async Task GetAllInvoicesAsync_ReturnsEmptyList_WhenNoInvoices()
    {
        // Act
        var result = await _service.GetAllInvoicesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCurrentMonthInvoiceAsync_ReturnsInvoice_WhenExists()
    {
        // Arrange
        var today = DateTime.Today;
        var currentMonthInvoice = new Invoice
        {
            Year = today.Year,
            Month = today.Month,
            Amount = 2500m
        };
        _dbContext.Invoices.Add(currentMonthInvoice);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetCurrentMonthInvoiceAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Year.Should().Be(today.Year);
        result.Month.Should().Be(today.Month);
        result.Amount.Should().Be(2500m);
    }

    [Fact]
    public async Task GetCurrentMonthInvoiceAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Act
        var result = await _service.GetCurrentMonthInvoiceAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateInvoiceAsync_CreatesNewInvoice_WhenDoesNotExist()
    {
        // Arrange
        var year = 2024;
        var month = 6;
        var amount = 3000m;

        // Act
        var result = await _service.UpdateInvoiceAsync(year, month, amount);

        // Assert
        result.Should().NotBeNull();
        result.Year.Should().Be(year);
        result.Month.Should().Be(month);
        result.Amount.Should().Be(amount);

        var savedInvoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Year == year && i.Month == month);
        savedInvoice.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateInvoiceAsync_UpdatesExistingInvoice_WhenExists()
    {
        // Arrange
        var year = 2024;
        var month = 7;
        var existingInvoice = new Invoice
        {
            Year = year,
            Month = month,
            Amount = 1000m
        };
        _dbContext.Invoices.Add(existingInvoice);
        await _dbContext.SaveChangesAsync();

        var newAmount = 2000m;

        // Act
        var result = await _service.UpdateInvoiceAsync(year, month, newAmount);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(newAmount);

        var invoiceCount = await _dbContext.Invoices
            .CountAsync(i => i.Year == year && i.Month == month);
        invoiceCount.Should().Be(1);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
