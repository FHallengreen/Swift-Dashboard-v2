using SwiftDashboard.Interfaces;
using SwiftDashboard.Models;

namespace SwiftDashboard.Services;

public class NoOpHolidayService : IHolidayService
{
    public Task<IEnumerable<Holiday>> GetUpcomingHolidaysAsync(DateTime? startDate = null)
        => Task.FromResult<IEnumerable<Holiday>>(Array.Empty<Holiday>());
}