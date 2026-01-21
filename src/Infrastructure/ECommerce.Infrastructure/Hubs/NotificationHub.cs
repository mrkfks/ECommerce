using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ECommerce.Infrastructure.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var companyId = user.FindFirst("CompanyId")?.Value;
                if (!string.IsNullOrEmpty(companyId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Company_{companyId}");
                }
            }
            await base.OnConnectedAsync();
        }
    }
}
