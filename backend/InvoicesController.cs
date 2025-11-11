using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace SwiftDashboard;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController(SwiftDbContext dbContext, IHubContext<InvoiceUpdateHub> hubContext) : ControllerBase
{
    private readonly IHubContext<InvoiceUpdateHub> _hubContext = hubContext; // Store hub context

    [HttpPost]
    public async Task<IActionResult> PostInvoice([FromBody] decimal amount)
    {
        var now = DateTime.Today;
        var year = now.Year;
        var month = now.Month;

        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Year == year && i.Month == month);

        if (invoice != null)
        {
            invoice.Amount = amount;
            dbContext.Update(invoice);
        }
        else
        {
            invoice = new Invoice // Assign to invoice to use its properties later
            {
                Year = year,
                Month = month,
                Amount = amount
            };
            dbContext.Invoices.Add(invoice);
        }

        await dbContext.SaveChangesAsync();
        // Send update to all connected clients
        await _hubContext.Clients.All.SendAsync("ReceiveInvoiceUpdate", new { year, month, amount });
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetInvoices()
    {
        var invoices = await dbContext.Invoices
            .Select(i => new
            {
                i.Year,
                i.Month,
                i.Amount
            })
            .ToListAsync();
        return Ok(invoices);
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentMonthInvoice()
    {
        var now = DateTime.Today;
        var year = now.Year;
        var month = now.Month;

        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Year == year && i.Month == month);

        return Ok(new { Year = year, Month = month, Amount = invoice?.Amount ?? 0m });
    }

    [HttpPut("{year}/{month}")]
    public async Task<IActionResult> UpdateInvoice(int year, int month, [FromBody] UpdateInvoiceModel model)
    {
        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Year == year && i.Month == month);

        if (invoice != null)
        {
            invoice.Amount = model.Amount;
            dbContext.Update(invoice);
        }
        else
        {
            // If updating a non-existent record, create it.
            invoice = new Invoice 
            {
                Year = year,
                Month = month,
                Amount = model.Amount
            };
            dbContext.Invoices.Add(invoice);
        }

        await dbContext.SaveChangesAsync();
        // Send update to all connected clients
        await _hubContext.Clients.All.SendAsync("ReceiveInvoiceUpdate", new { year, month, amount = model.Amount });
        return Ok();
    }
}