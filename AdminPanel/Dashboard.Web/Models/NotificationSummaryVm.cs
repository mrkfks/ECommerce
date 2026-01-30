namespace Dashboard.Web.Models
{
    public class NotificationSummaryVm
    {
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public int LowStockCount { get; set; }
        public int NewOrderCount { get; set; }
        public int ReturnRequestCount { get; set; }
        public int PaymentFailedCount { get; set; }
        public List<NotificationDto> RecentNotifications { get; set; } = new();
    }
}
