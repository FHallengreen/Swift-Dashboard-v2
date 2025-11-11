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
public class InfoControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly IServiceScope _scope;
    private readonly SwiftDbContext _dbContext;

    public InfoControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<SwiftDbContext>();
    }

    [Fact]
    public async Task GetInfo_ReturnsOkWithInfo_WhenExists()
    {
        // Arrange
        var info = new Info { Id = 1, Text = "Test information" };
        _dbContext.Info.Add(info);
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        result.Should().NotBeNull();
        result!["text"].Should().Be("Test information");
    }

    [Fact]
    public async Task GetInfo_ReturnsEmptyText_WhenDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        result.Should().NotBeNull();
        result!["text"].Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateInfo_CreatesNewInfo()
    {
        // Arrange
        var request = new UpdateInfoRequest { Text = "New information" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var savedInfo = await _dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1);
        savedInfo.Should().NotBeNull();
        savedInfo!.Text.Should().Be("New information");
    }

    [Fact]
    public async Task UpdateInfo_UpdatesExistingInfo()
    {
        // Arrange - clear any existing data first
        var existing = await _dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1);
        if (existing != null)
        {
            _dbContext.Info.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }
        
        var existingInfo = new Info { Id = 1, Text = "Old text" };
        _dbContext.Info.Add(existingInfo);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateInfoRequest { Text = "Updated text" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/info", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Reload from database to get fresh data (not cached)
        _dbContext.ChangeTracker.Clear();
        var updatedInfo = await _dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1);
        updatedInfo.Should().NotBeNull();
        updatedInfo!.Text.Should().Be("Updated text");
    }

    public void Dispose()
    {
        // Clean up test data after each test
        _dbContext.Info.RemoveRange(_dbContext.Info);
        _dbContext.SaveChanges();
        _scope.Dispose();
        _client.Dispose();
    }
}
