using Microsoft.AspNetCore.Mvc;
using SwiftDashboard.Interfaces;
using SwiftDashboard.Models.DTOs;

namespace SwiftDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfoController : ControllerBase
{
    private readonly IInfoService _infoService;

    public InfoController(IInfoService infoService)
    {
        _infoService = infoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetInfo()
    {
        var info = await _infoService.GetInfoAsync();
        return Ok(new { Text = info?.Text ?? "" });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateInfo([FromBody] UpdateInfoRequest request)
    {
        await _infoService.UpdateInfoAsync(request.Text);
        return Ok();
    }
}
