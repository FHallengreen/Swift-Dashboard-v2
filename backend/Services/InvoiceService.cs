using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SwiftDashboard.Data;
using SwiftDashboard.Hubs;
using SwiftDashboard.Interfaces;
using SwiftDashboard.Models;

namespace SwiftDashboard.Services;

public class InvoiceService : IInvoiceService
{
    private readonly SwiftDbContext _dbContext;
    private readonly IHubContext<InvoiceUpdateHub> _hubContext;

    public InvoiceService(SwiftDbContext dbContext, IHubContext<InvoiceUpdateHub> hubContext)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
    }

    public async Task<Invoice> CreateOrUpdateInvoiceAsync(decimal amount)
    {
        var now = DateTime.Today;
        var year = now.Year;
        var month = now.Month;

        // Use retry logic to handle concurrent writes gracefully
        int maxRetries = 3;
        int retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            try
            {
                var invoice = await _dbContext.Invoices
                    .FirstOrDefaultAsync(i => i.Year == year && i.Month == month);

                if (invoice != null)
                {
                    invoice.Amount = amount;
                    _dbContext.Update(invoice);
                }
                else
                {
                    invoice = new Invoice
                    {
                        Year = year,
                        Month = month,
                        Amount = amount
                    };
                    _dbContext.Invoices.Add(invoice);
                }

                await _dbContext.SaveChangesAsync();
                
                // Send update to all connected clients
                await _hubContext.Clients.All.SendAsync("ReceiveInvoiceUpdate", new { year, month, amount });
                
                return invoice;
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlConnector.MySqlException mysqlEx && 
                                                mysqlEx.Number == 1062) // Duplicate entry error
            {
                // Reset context state for retry
                _dbContext.ChangeTracker.Clear();
                retryCount++;
                
                if (retryCount >= maxRetries)
                {
                    throw;
                }
                
                // Small delay before retry
                await Task.Delay(10 * retryCount);
            }
        }
        
        // This should never be reached due to throw in catch block
        throw new InvalidOperationException("Failed to create or update invoice");
    }

    public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
    {
        return await _dbContext.Invoices.ToListAsync();
    }

    public async Task<Invoice?> GetCurrentMonthInvoiceAsync()
    {
        var now = DateTime.Today;
        var year = now.Year;
        var month = now.Month;

        return await _dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Year == year && i.Month == month);
    }

    public async Task<Invoice> UpdateInvoiceAsync(int year, int month, decimal amount)
    {
        // Use retry logic to handle concurrent writes gracefully
        int maxRetries = 3;
        int retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            try
            {
                var invoice = await _dbContext.Invoices
                    .FirstOrDefaultAsync(i => i.Year == year && i.Month == month);

                if (invoice != null)
                {
                    invoice.Amount = amount;
                    _dbContext.Update(invoice);
                }
                else
                {
                    invoice = new Invoice
                    {
                        Year = year,
                        Month = month,
                        Amount = amount
                    };
                    _dbContext.Invoices.Add(invoice);
                }

                await _dbContext.SaveChangesAsync();
                
                // Send update to all connected clients
                await _hubContext.Clients.All.SendAsync("ReceiveInvoiceUpdate", new { year, month, amount });
                
                return invoice;
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlConnector.MySqlException mysqlEx && 
                                                mysqlEx.Number == 1062) // Duplicate entry error
            {
                // Reset context state for retry
                _dbContext.ChangeTracker.Clear();
                retryCount++;
                
                if (retryCount >= maxRetries)
                {
                    throw;
                }
                
                // Small delay before retry
                await Task.Delay(10 * retryCount);
            }
        }
        
        // This should never be reached due to throw in catch block
        throw new InvalidOperationException("Failed to update invoice");
    }
}
