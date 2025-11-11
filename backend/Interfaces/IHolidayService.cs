using SwiftDashboard.Models;

namespace SwiftDashboard.Interfaces;

public interface IHolidayService
{
    Task<IEnumerable<Holiday>> GetUpcomingHolidaysAsync(DateTime? startDate = null);
}
