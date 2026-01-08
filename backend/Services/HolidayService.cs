using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using SwiftDashboard.Interfaces;
using SwiftDashboard.Models;

namespace SwiftDashboard.Services;

public class HolidayService : IHolidayService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<HolidayService> _logger;

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

    private static readonly string[] CountryCodes =
    {
        "US", "GB", "DE", "JP", "AU", "DK", "FR", "CA", "CN", "BE", "AE", "IT", "KR", "ES", "IN", "BR", "ZA",
        "MX", "NL", "SE", "NO", "FI", "PL", "PT", "SG", "AR", "CY", "EG", "GR", "TR"
    };

    public HolidayService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, ILogger<HolidayService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<IEnumerable<Holiday>> GetUpcomingHolidaysAsync(DateTime? startDate = null)
    {
        var effectiveStartDate = startDate ?? DateTime.UtcNow;
        var allUpcomingHolidays = new List<Holiday>();

        try
        {
            for (int i = 0; i < 7; i++)
            {
                var currentDate = effectiveStartDate.AddDays(i);
                var currentYear = currentDate.Year;
                var currentDateString = currentDate.ToString("yyyy-MM-dd");
                _logger.LogInformation("Fetching holidays for {Date} (Year: {Year})", currentDateString, currentYear);

                foreach (var countryCode in CountryCodes)
                {
                    var holidaysForCountry = await GetHolidaysForCountryAndYearAsync(countryCode, currentYear);
                    
                    if (holidaysForCountry != null)
                    {
                        var publicHolidaysOnDate = holidaysForCountry
                            .Where(h => h.Date == currentDateString && 
                                       h.Types != null && 
                                       h.Types.Contains("Public"))
                            .ToList();

                        if (publicHolidaysOnDate.Any())
                        {
                            // Prioritize global public holidays
                            var selectedHoliday = publicHolidaysOnDate.FirstOrDefault(h => h.Global) 
                                                ?? publicHolidaysOnDate.FirstOrDefault();

                            if (selectedHoliday != null)
                            {
                                selectedHoliday.CountryName = CountryNames.TryGetValue(
                                    selectedHoliday.CountryCode ?? string.Empty, 
                                    out var name) ? name : selectedHoliday.CountryCode;
                                allUpcomingHolidays.Add(selectedHoliday);
                            }
                        }
                    }
                }
            }

            return allUpcomingHolidays.OrderBy(h => h.Date).ThenBy(h => h.CountryName).ThenBy(h => h.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching holidays");
            throw;
        }
    }

    private async Task<List<Holiday>?> GetHolidaysForCountryAndYearAsync(string countryCode, int year)
    {
        var cacheKey = $"Holidays_{year}_{countryCode}";
        
        if (_memoryCache.TryGetValue(cacheKey, out List<Holiday>? cachedHolidays))
        {
            _logger.LogInformation("Using cached holidays for {Country} year {Year}", countryCode, year);
            return cachedHolidays;
        }

        var httpClient = _httpClientFactory.CreateClient();
        var url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/{countryCode}";

        try
        {
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new List<Holiday>();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                cachedHolidays = new List<Holiday>();
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("Empty response body for {Country} year {Year}", countryCode, year);
                    cachedHolidays = new List<Holiday>();
                }
                else
                {
                    try
                    {
                        cachedHolidays = JsonSerializer.Deserialize<List<Holiday>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<Holiday>();
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Failed to deserialize holidays for {Country} year {Year}", countryCode, year);
                        cachedHolidays = new List<Holiday>();
                    }
                }
            }

            _memoryCache.Set(cacheKey, cachedHolidays, TimeSpan.FromDays(30));
            return cachedHolidays;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching holidays for {Country} year {Year}", countryCode, year);
            return new List<Holiday>();
        }
    }
}
