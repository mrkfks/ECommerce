namespace Dashboard.Web.Models
{
    public class CustomerMessageVm
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsReplied { get; set; }
        public string? Reply { get; set; }
        public DateTime? RepliedAt { get; set; }
        public int? RepliedByUserId { get; set; }
        public string? RepliedByName { get; set; }
        public int Category { get; set; }
        public string CategoryText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string TimeAgo
        {
            get
            {
                var diff = DateTime.Now - CreatedAt;
                if (diff.TotalMinutes < 1) return "Az önce";
                if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} dk önce";
                if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} saat önce";
                if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} gün önce";
                return CreatedAt.ToString("dd MMM yyyy");
            }
        }

        public string StatusBadgeClass => IsReplied ? "bg-success" : (IsRead ? "bg-warning" : "bg-danger");
        public string StatusText => IsReplied ? "Yanıtlandı" : (IsRead ? "Okundu" : "Yeni");
        public string CategoryIcon => Category switch
        {
            0 => "fa-envelope",
            1 => "fa-shopping-cart",
            2 => "fa-box",
            3 => "fa-rotate-left",
            4 => "fa-triangle-exclamation",
            5 => "fa-lightbulb",
            _ => "fa-envelope"
        };
        public string CategoryBadgeClass => Category switch
        {
            0 => "bg-secondary",
            1 => "bg-primary",
            2 => "bg-info",
            3 => "bg-warning",
            4 => "bg-danger",
            5 => "bg-success",
            _ => "bg-secondary"
        };
        public string CustomerInitials => CustomerName.Length > 0
            ? string.Join("", CustomerName.Split(' ').Where(n => n.Length > 0).Take(2).Select(n => n[0])).ToUpper()
            : "?";
    }
}
