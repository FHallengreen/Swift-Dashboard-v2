using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SwiftDashboard.Data;
using SwiftDashboard.Hubs;
using SwiftDashboard.Interfaces;
using SwiftDashboard.Models;

namespace SwiftDashboard.Services;

public class InfoService : IInfoService
{
    private readonly SwiftDbContext _dbContext;
    private readonly IHubContext<InfoUpdateHub> _hubContext;

    public InfoService(SwiftDbContext dbContext, IHubContext<InfoUpdateHub> hubContext)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
    }

    public async Task<Info?> GetInfoAsync()
    {
        return await _dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1);
    }

    public async Task UpdateInfoAsync(string? text)
    {
        var info = await _dbContext.Info.FirstOrDefaultAsync(i => i.Id == 1);
        
        if (info != null)
        {
            info.Text = text;
            _dbContext.Update(info);
        }
        else
        {
            _dbContext.Info.Add(new Info { Id = 1, Text = text });
        }
        
        await _dbContext.SaveChangesAsync();
        
        // Broadcast the update to all connected clients
        await _hubContext.Clients.All.SendAsync("ReceiveInfoUpdate", new { Text = text });
    }
}
