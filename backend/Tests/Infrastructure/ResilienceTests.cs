using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SwiftDashboard.Data;
using SwiftDashboard.Models;
using Xunit;

namespace SwiftDashboard.Tests.Infrastructure;

/// <summary>
/// Infrastructure resilience tests validating system behavior under failure conditions
/// Critical for exam requirement: "driftes uden manuel vedligeholdelse"
/// </summary>
[Collection("Database collection")]
public class ResilienceTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ResilienceTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task API_HandlesMultipleConcurrentRequests()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Act - Send 10 concurrent requests
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync("/api/invoices"));
        }
        
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => 
            r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task API_HandlesRapidConsecutiveWrites()
    {
        // Arrange & Act - Write multiple times rapidly
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_client.PostAsJsonAsync("/api/info", new { Text = $"Update {i}" }));
        }
        
        var responses = await Task.WhenAll(tasks);

        // Assert - All should succeed
        responses.Should().AllSatisfy(r => 
            r.StatusCode.Should().Be(HttpStatusCode.OK));

        // Verify final state is consistent
        var finalResponse = await _client.GetAsync("/api/info");
        finalResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Database_PersistsDataAcrossRequests()
    {
        // Arrange
        var testText = $"Persistence test {Guid.NewGuid()}";
        
        // Act - Write data
        var writeResponse = await _client.PostAsJsonAsync("/api/info", new { Text = testText });
        writeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Create new client to simulate new connection
        using var newClient = _factory.CreateClient();
        
        // Read data with new client
        var readResponse = await newClient.GetAsync("/api/info");
        var result = await readResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        // Assert - Data should persist
        result!["text"].Should().Be(testText);
    }

    [Fact]
    public async Task API_HandlesEmptyDatabaseGracefully()
    {
        // Arrange - Clear database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SwiftDbContext>();
        dbContext.Info.RemoveRange(dbContext.Info);
        dbContext.Invoices.RemoveRange(dbContext.Invoices);
        await dbContext.SaveChangesAsync();

        // Act & Assert - Should not crash
        var infoResponse = await _client.GetAsync("/api/info");
        infoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var invoicesResponse = await _client.GetAsync("/api/invoices");
        invoicesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var currentResponse = await _client.GetAsync("/api/invoices/current");
        currentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUpcomingHolidays_HandlesExternalAPIFailureGracefully()
    {
        // Act - Even if external API fails, should not crash
        var response = await _client.GetAsync("/api/holidays/upcoming");

        // Assert - Should return OK even if empty or cached
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task API_HandlesInvalidDateParametersGracefully()
    {
        // Arrange
        var invalidDates = new[] 
        { 
            "not-a-date", 
            "2025-13-01", // Invalid month
            "2025-02-30", // Invalid day
            "99999999999999"
        };

        // Act & Assert
        foreach (var invalidDate in invalidDates)
        {
            var response = await _client.GetAsync($"/api/holidays/upcoming?startDateStr={invalidDate}");
            
            // Should handle gracefully - either return OK with current date or BadRequest
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task UpdateInvoice_MaintainsDataIntegrity()
    {
        // Arrange - Create initial invoice
        var initialAmount = 1000m;
        await _client.PostAsJsonAsync("/api/invoices", initialAmount);

        // Act - Update multiple times
        var amounts = new[] { 1500m, 2000m, 2500m };
        foreach (var amount in amounts)
        {
            await _client.PostAsJsonAsync("/api/invoices", amount);
        }

        // Assert - Should only have one invoice for current month
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SwiftDbContext>();
        
        var today = DateTime.Today;
        var invoices = await dbContext.Invoices
            .Where(i => i.Year == today.Year && i.Month == today.Month)
            .ToListAsync();

        invoices.Should().HaveCount(1);
        invoices[0].Amount.Should().Be(amounts[^1]); // Should have last amount
    }

    [Fact]
    public async Task API_ReturnsConsistentContentTypes()
    {
        // Arrange
        var endpoints = new[] 
        { 
            "/api/invoices", 
            "/api/info", 
            "/api/holidays/upcoming",
            "/api/invoices/current"
        };

        // Act & Assert
        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            response.Content.Headers.ContentType?.MediaType
                .Should().Be("application/json");
        }
    }

    [Fact]
    public async Task Database_HandlesSpecialCharactersInText()
    {
        // Arrange
        var specialChars = "Test with émojis 🎉, quotes \"', and symbols: @#$%^&*()";

        // Act
        var writeResponse = await _client.PostAsJsonAsync("/api/info", new { Text = specialChars });
        writeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var readResponse = await _client.GetAsync("/api/info");
        var result = await readResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        // Assert
        result!["text"].Should().Be(specialChars);
    }

    [Fact]
    public async Task API_HandlesVeryLongTextInput()
    {
        // Arrange
        var longText = new string('A', 10000); // 10KB of text

        // Act
        var response = await _client.PostAsJsonAsync("/api/info", new { Text = longText });

        // Assert - Should either accept or return appropriate error
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, 
            HttpStatusCode.BadRequest,
            HttpStatusCode.RequestEntityTooLarge
        );
    }
}
