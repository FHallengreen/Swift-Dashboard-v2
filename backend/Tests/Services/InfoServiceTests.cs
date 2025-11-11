using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SwiftDashboard.Data;
using SwiftDashboard.Models;
using SwiftDashboard.Services;
using Xunit;

namespace SwiftDashboard.Tests.Services;

public class InfoServiceTests : IDisposable
{
    private readonly SwiftDbContext _dbContext;
    private readonly InfoService _service;

    public InfoServiceTests()
    {
        var options = new DbContextOptionsBuilder<SwiftDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new SwiftDbContext(options);
        _service = new InfoService(_dbContext);
    }

    [Fact]
    public async Task GetInfoAsync_ReturnsInfo_WhenExists()
    {
        // Arrange
        var info = new Info { Id = 1, Text = "Test information" };
        _dbContext.Info.Add(info);
        await _dbContext.SaveChangesAsync();

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
        var info = await _dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1);
        info.Should().NotBeNull();
        info!.Text.Should().Be(text);
    }

    [Fact]
    public async Task UpdateInfoAsync_UpdatesExistingInfo_WhenExists()
    {
        // Arrange
        var existingInfo = new Info { Id = 1, Text = "Old text" };
        _dbContext.Info.Add(existingInfo);
        await _dbContext.SaveChangesAsync();

        var newText = "Updated text";

        // Act
        await _service.UpdateInfoAsync(newText);

        // Assert
        var info = await _dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1);
        info.Should().NotBeNull();
        info!.Text.Should().Be(newText);

        var infoCount = await _dbContext.Info.CountAsync();
        infoCount.Should().Be(1);
    }

    [Fact]
    public async Task UpdateInfoAsync_HandlesNullText()
    {
        // Act
        await _service.UpdateInfoAsync(null);

        // Assert
        var info = await _dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1);
        info.Should().NotBeNull();
        info!.Text.Should().BeNull();
    }

    [Fact]
    public async Task UpdateInfoAsync_HandlesEmptyText()
    {
        // Act
        await _service.UpdateInfoAsync("");

        // Assert
        var info = await _dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1);
        info.Should().NotBeNull();
        info!.Text.Should().BeEmpty();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
