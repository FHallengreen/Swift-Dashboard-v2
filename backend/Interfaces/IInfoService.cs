using SwiftDashboard.Models;

namespace SwiftDashboard.Interfaces;

public interface IInfoService
{
    Task<Info?> GetInfoAsync();
    Task UpdateInfoAsync(string? text);
}
