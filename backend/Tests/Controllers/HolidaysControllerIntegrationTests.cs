using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SwiftDashboard.Models;
using Xunit;

namespace SwiftDashboard.Tests.Controllers;

[Collection("Database collection")]
public class HolidaysControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HolidaysControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUpcomingHolidays_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/holidays/upcoming", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var holidays = await response.Content.ReadFromJsonAsync<List<Holiday>>(cancellationToken: TestContext.Current.CancellationToken);
        holidays.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUpcomingHolidays_WithStartDate_ReturnsOk()
    {
        // Arrange
        var startDate = "2025-01-01";

        // Act
        var response = await _client.GetAsync($"/api/holidays/upcoming?startDateStr={startDate}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var holidays = await response.Content.ReadFromJsonAsync<List<Holiday>>(cancellationToken: TestContext.Current.CancellationToken);
        holidays.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUpcomingHolidays_WithInvalidDate_ReturnsOk()
    {
        // Arrange
        var startDate = "invalid-date";

        // Act
        var response = await _client.GetAsync($"/api/holidays/upcoming?startDateStr={startDate}", TestContext.Current.CancellationToken);

        // Assert
        // Should fallback to current date and still return OK
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
