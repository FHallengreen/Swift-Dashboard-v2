using Microsoft.AspNetCore.SignalR;

namespace SwiftDashboard
{
    public class InvoiceUpdateHub : Hub
    {
        // We don't need methods here for server-to-client push
        // Clients will listen to messages sent by the server
    }
}
