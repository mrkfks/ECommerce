namespace ECommerce.Application.Interfaces
{
    public interface IRealTimeNotificationService
    {
        Task SendNotificationAsync(string userId, string message, string type = "Info");
        Task SendToAllAsync(string message, string type = "Info");
        Task SendToCompanyAsync(int companyId, string message, string type = "Info");
    }
}
