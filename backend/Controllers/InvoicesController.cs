using Microsoft.AspNetCore.Mvc;
using SwiftDashboard.Interfaces;
using SwiftDashboard.Models.DTOs;

namespace SwiftDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    public async Task<IActionResult> PostInvoice([FromBody] decimal amount)
    {
        var invoice = await _invoiceService.CreateOrUpdateInvoiceAsync(amount);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetInvoices()
    {
        var invoices = await _invoiceService.GetAllInvoicesAsync();
        var result = invoices.Select(i => new
        {
            i.Year,
            i.Month,
            i.Amount
        });
        return Ok(result);
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentMonthInvoice()
    {
        var invoice = await _invoiceService.GetCurrentMonthInvoiceAsync();
        var now = DateTime.Today;
        return Ok(new 
        { 
            Year = now.Year, 
            Month = now.Month, 
            Amount = invoice?.Amount ?? 0m 
        });
    }

    [HttpPut("{year}/{month}")]
    public async Task<IActionResult> UpdateInvoice(int year, int month, [FromBody] UpdateInvoiceModel model)
    {
        var invoice = await _invoiceService.UpdateInvoiceAsync(year, month, model.Amount);
        return Ok();
    }
}
