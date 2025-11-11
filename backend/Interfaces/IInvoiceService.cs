using SwiftDashboard.Models;

namespace SwiftDashboard.Interfaces;

public interface IInvoiceService
{
    Task<Invoice> CreateOrUpdateInvoiceAsync(decimal amount);
    Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
    Task<Invoice?> GetCurrentMonthInvoiceAsync();
    Task<Invoice> UpdateInvoiceAsync(int year, int month, decimal amount);
}
