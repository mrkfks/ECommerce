using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Hubs; // We will create this
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.Infrastructure.Services
{
    public class SignalRNotificationService : IRealTimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(string userId, string message, string type = "Info")
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message, type);
        }

        public async Task SendToAllAsync(string message, string type = "Info")
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message, type);
        }

        public async Task SendToCompanyAsync(int companyId, string message, string type = "Info")
        {
            // Assuming we have groups for companies
            await _hubContext.Clients.Group($"Company_{companyId}").SendAsync("ReceiveNotification", message, type);
        }
    }
}
