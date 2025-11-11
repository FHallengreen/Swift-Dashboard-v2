using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace SwiftDashboard;

[ApiController]
[Route("api/[controller]")]
public class InfoController(SwiftDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetInfo()
    {
        var info = await dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1);
        return Ok(new { Text = info?.Text ?? "" });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateInfo([FromBody] UpdateInfoRequest request)
    {
        var info = await dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1);
        if (info != null)
        {
            info.Text = request.Text;
            dbContext.Update(info);
        }
        else
        {
            dbContext.Info.Add(new Info { Id = 1, Text = request.Text });
        }
        await dbContext.SaveChangesAsync();
        return Ok();
    }
}