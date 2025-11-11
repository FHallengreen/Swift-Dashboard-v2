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
}
