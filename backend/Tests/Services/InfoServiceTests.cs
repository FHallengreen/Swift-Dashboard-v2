using System;
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

public class InfoServiceTests : IDisposable
{
    private readonly SwiftDbContext _dbContext;
    private readonly InfoService _service;
    private readonly Mock<IHubContext<InfoUpdateHub>> _mockHubContext;

    public InfoServiceTests()
    {
        var options = new DbContextOptionsBuilder<SwiftDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new SwiftDbContext(options);
        _mockHubContext = new Mock<IHubContext<InfoUpdateHub>>();
        
        // Setup mock for SignalR
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);
        mockClients.Setup(x => x.All).Returns(mockClientProxy.Object);
        
        _service = new InfoService(_dbContext, _mockHubContext.Object);
    }

    [Fact]
    public async Task GetInfoAsync_ReturnsInfo_WhenExists()
    {
        // Arrange
        var info = new Info { Id = 1, Text = "Test information" };
        _dbContext.Info.Add(info);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.GetInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Text.Should().Be("Test information");
    }

    [Fact]
    public async Task GetInfoAsync_ReturnsNull_WhenDoesNotExist()
    {
        // Act
        var result = await _service.GetInfoAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateInfoAsync_CreatesNewInfo_WhenDoesNotExist()
    {
        // Arrange
        var text = "New information text";

        // Act
        await _service.UpdateInfoAsync(text);

        // Assert
        var info = await _dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1, cancellationToken: TestContext.Current.CancellationToken);
        info.Should().NotBeNull();
        info!.Text.Should().Be(text);
    }

    [Fact]
    public async Task UpdateInfoAsync_UpdatesExistingInfo_WhenExists()
    {
        // Arrange
        var existingInfo = new Info { Id = 1, Text = "Old text" };
        _dbContext.Info.Add(existingInfo);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var newText = "Updated text";

        // Act
        await _service.UpdateInfoAsync(newText);

        // Assert
        var info = await _dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1, cancellationToken: TestContext.Current.CancellationToken);
        info.Should().NotBeNull();
        info!.Text.Should().Be(newText);

        var infoCount = await _dbContext.Info.CountAsync(cancellationToken: TestContext.Current.CancellationToken);
        infoCount.Should().Be(1);
    }

    [Fact]
    public async Task UpdateInfoAsync_HandlesNullText()
    {
        // Act
        await _service.UpdateInfoAsync(null);

        // Assert
        var info = await _dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1, cancellationToken: TestContext.Current.CancellationToken);
        info.Should().NotBeNull();
        info!.Text.Should().BeNull();
    }

    [Fact]
    public async Task UpdateInfoAsync_HandlesEmptyText()
    {
        // Act
        await _service.UpdateInfoAsync("");

        // Assert
        var info = await _dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1, cancellationToken: TestContext.Current.CancellationToken);
        info.Should().NotBeNull();
        info!.Text.Should().BeEmpty();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
