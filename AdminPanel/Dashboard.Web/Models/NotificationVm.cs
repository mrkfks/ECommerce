namespace Dashboard.Web.Models
{
    public class NotificationVm
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int Type { get; set; }
        public string TypeText { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string PriorityText { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? Data { get; set; }
        public string? ActionUrl { get; set; }
        public string? ActionText { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;

        public string TypeIcon => Type switch
        {
            1 => "fa-box-open",
            2 => "fa-shopping-cart",
            3 => "fa-undo",
            4 => "fa-credit-card",
            5 => "fa-truck",
            _ => "fa-bell"
        };

        public string TypeColor => Type switch
        {
            1 => "danger",
            2 => "success",
            3 => "warning",
            4 => "danger",
            5 => "info",
            _ => "secondary"
        };
    }
}
