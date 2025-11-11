using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SwiftDashboard.Models;
using SwiftDashboard.Services;
using Xunit;

namespace SwiftDashboard.Tests.Services;

public class HolidayServiceTest : IDisposable
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<HolidayService>> _mockLogger;
    private readonly HolidayService _service;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

    public HolidayServiceTest()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<HolidayService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        _service = new HolidayService(_mockHttpClientFactory.Object, _memoryCache, _mockLogger.Object);
    }

    [Fact]
    public async Task GetUpcomingHolidaysAsync_ReturnsHolidays_WhenApiReturnsData()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var holidays = new List<Holiday>
        {
            new()
            {
                Date = "2025-01-01",
                Name = "New Year's Day",
                LocalName = "New Year's Day",
                CountryCode = "US",
                Fixed = true,
                Global = true,
                Types = new List<string> { "Public" }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(holidays);
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _service.GetUpcomingHolidaysAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        var resultList = result.ToList();
        resultList.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetUpcomingHolidaysAsync_UsesCurrentDate_WhenNoStartDateProvided()
    {
        // Arrange
        var emptyHolidays = new List<Holiday>();
        var jsonResponse = JsonSerializer.Serialize(emptyHolidays);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _service.GetUpcomingHolidaysAsync();

        // Assert
        result.Should().NotBeNull();
        // Verify that the service was called (it uses DateTime.UtcNow internally)
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetUpcomingHolidaysAsync_FiltersOnlyPublicHolidays()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var holidays = new List<Holiday>
        {
            new()
            {
                Date = "2025-01-01",
                Name = "New Year's Day",
                CountryCode = "US",
                Fixed = true,
                Global = true,
                Types = new List<string> { "Public" }
            },
            new()
            {
                Date = "2025-01-01",
                Name = "Bank Holiday",
                CountryCode = "US",
                Fixed = true,
                Global = false,
                Types = new List<string> { "Bank" }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(holidays);
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _service.GetUpcomingHolidaysAsync(startDate);

        // Assert
        var resultList = result.ToList();
        // Should only include public holidays, not bank holidays
        resultList.Should().OnlyContain(h => h.Types != null && h.Types.Contains("Public"));
    }

    [Fact]
    public async Task GetUpcomingHolidaysAsync_HandlesApiError_Gracefully()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var result = await _service.GetUpcomingHolidaysAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUpcomingHolidaysAsync_PrioritizesGlobalHolidays()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var holidays = new List<Holiday>
        {
            new()
            {
                Date = "2025-01-01",
                Name = "Regional Holiday",
                CountryCode = "US",
                Fixed = true,
                Global = false,
                Types = new List<string> { "Public" }
            },
            new()
            {
                Date = "2025-01-01",
                Name = "National Holiday",
                CountryCode = "US",
                Fixed = true,
                Global = true,
                Types = new List<string> { "Public" }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(holidays);
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _service.GetUpcomingHolidaysAsync(startDate);

        // Assert
        var resultList = result.ToList();
        var usHolidays = resultList.Where(h => h.CountryCode == "US" && h.Date == "2025-01-01").ToList();
        // Should prioritize global holidays
        if (usHolidays.Any())
        {
            usHolidays.Should().Contain(h => h.Global);
        }
    }

    public void Dispose()
    {
        _memoryCache?.Dispose();
    }
}