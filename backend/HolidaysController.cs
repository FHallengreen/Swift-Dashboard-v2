using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace SwiftDashboard;

[Route("api/[controller]")]
[ApiController]
public class HolidaysController(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
    : ControllerBase
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    private static readonly Dictionary<string, string> CountryNames = new()
    {
        { "US", "United States" },
        { "GB", "United Kingdom" },
        { "DE", "Germany" },
        { "JP", "Japan" },
        { "AU", "Australia" },
        { "DK", "Denmark" },
        { "FR", "France" },
        { "CA", "Canada" },
        { "CN", "China" },
        { "BE", "Belgium" },
        { "AE", "United Arab Emirates" },
        { "IT", "Italy" },
        { "KR", "South Korea" },
        { "ES", "Spain" },
        { "IN", "India" },
        { "BR", "Brazil" },
        { "ZA", "South Africa" },
        { "MX", "Mexico" },
        { "NL", "Netherlands" },
        { "SE", "Sweden" },
        { "NO", "Norway" },
        { "FI", "Finland" },
        { "PL", "Poland" },
        { "SG", "Singapore" },
        { "PT", "Portugal" },
        { "AR", "Argentina" },
        { "CY", "Cyprus" },
        { "EG", "Egypt" },
        { "GR", "Greece" },
        { "TR", "Turkey" },
    };

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingHolidays([FromQuery] string? startDateStr = null)
    {
        try
        {
            var startDate = startDateStr != null && DateTime.TryParse(startDateStr, out var parsedDate)
                ? parsedDate
                : DateTime.UtcNow;

            var allUpcomingHolidays = new List<Holiday>();

            var countryCodes = new[]
            {
                "US", "GB", "DE", "JP", "AU", "DK", "FR", "CA", "CN", "BE", "AE", "IT", "KR", "ES", "IN", "BR", "ZA",
                "MX", "NL", "SE", "NO", "FI", "PL", "PT", "SG", "AR", "CY", "EG", "GR", "TR"
            };

            for (int i = 0; i < 7; i++)
            {
                var currentDate = startDate.AddDays(i);
                var currentYear = currentDate.Year;
                var currentDateString = currentDate.ToString("yyyy-MM-dd");
                Console.WriteLine($"Fetching holidays for {currentDateString} (Year: {currentYear})");

                foreach (var countryCode in countryCodes)
                {
                    var cacheKey = $"Holidays_{currentYear}_{countryCode}";
                    if (!memoryCache.TryGetValue(cacheKey, out List<Holiday>? yearHolidaysForCountry))
                    {
                        var url = $"https://date.nager.at/api/v3/PublicHolidays/{currentYear}/{countryCode}";
                        Console.WriteLine($"Fetching holidays for {countryCode} for year {currentYear}: {url}");

                        var response = await _httpClient.GetAsync(url);

                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine(
                                $"Failed to fetch holidays for {countryCode} year {currentYear}: {response.StatusCode} - {response.ReasonPhrase}");
                            continue;
                        }
                        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                        {
                            Console.WriteLine($"No holiday data available for {countryCode} year {currentYear} (204 No Content)");
                            yearHolidaysForCountry = new List<Holiday>();
                        }
                        else
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            if (string.IsNullOrWhiteSpace(content))
                            {
                                Console.WriteLine($"Empty response body for {countryCode} year {currentYear}, skipping deserialization");
                                yearHolidaysForCountry = new List<Holiday>();
                            }
                            else
                            {
                                try
                                {
                                    yearHolidaysForCountry = JsonSerializer.Deserialize<List<Holiday>>(content, new JsonSerializerOptions
                                    {
                                        PropertyNameCaseInsensitive = true
                                    });
                                }
                                catch (JsonException jsonEx)
                                {
                                    Console.WriteLine($"Failed to deserialize holidays for {countryCode} year {currentYear}: {jsonEx.Message}");
                                    yearHolidaysForCountry = new List<Holiday>();
                                }
                            }
                        }
                        memoryCache.Set(cacheKey, yearHolidaysForCountry, TimeSpan.FromDays(1));
                    }
                    else
                    {
                         Console.WriteLine($"Using cached holidays for {countryCode} year {currentYear}");
                    }
                    
                    if (yearHolidaysForCountry != null)
                    {
                        // Filter for public holidays on the current date for the specific country
                        var publicHolidaysOnDateForCountry = yearHolidaysForCountry
                            .Where(h => h.Date == currentDateString && 
                                         h.Types != null && 
                                         h.Types.Contains("Public"))
                            .ToList();

                        if (publicHolidaysOnDateForCountry.Any())
                        {
                            Holiday? selectedHoliday = null;

                            // Prioritize global public holidays
                            selectedHoliday = publicHolidaysOnDateForCountry.FirstOrDefault(h => h.Global);

                            // If no global public holiday, take the first available public holiday
                            if (selectedHoliday == null)
                            {
                                selectedHoliday = publicHolidaysOnDateForCountry.FirstOrDefault();
                            }

                            if (selectedHoliday != null)
                            {
                                selectedHoliday.CountryName = CountryNames.TryGetValue(selectedHoliday.CountryCode ?? string.Empty, out var name) ? name : selectedHoliday.CountryCode;
                                allUpcomingHolidays.Add(selectedHoliday);
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Total upcoming holidays found: {allUpcomingHolidays.Count}");
            return Ok(allUpcomingHolidays.OrderBy(h => h.Date).ThenBy(h => h.CountryName).ThenBy(h => h.Name));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching holidays: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return StatusCode(500, new { error = "Failed to fetch public holidays.", details = ex.Message });
        }
    }
}