using Microsoft.AspNetCore.Mvc;
using SwiftDashboard.Interfaces;

namespace SwiftDashboard.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HolidaysController : ControllerBase
{
    private readonly IHolidayService _holidayService;

    public HolidaysController(IHolidayService holidayService)
    {
        _holidayService = holidayService;
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingHolidays([FromQuery] string? startDateStr = null)
    {
        try
        {
            var startDate = startDateStr != null && DateTime.TryParse(startDateStr, out var parsedDate)
                ? parsedDate
                : DateTime.UtcNow;

            var holidays = await _holidayService.GetUpcomingHolidaysAsync(startDate);
            return Ok(holidays);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to fetch public holidays.", details = ex.Message });
        }
    }
}
