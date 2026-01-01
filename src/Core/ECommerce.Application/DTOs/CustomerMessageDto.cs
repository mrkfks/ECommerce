using ECommerce.Domain.Entities;

namespace ECommerce.Application.DTOs
{
    public class CustomerMessageDto
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
        public MessageCategory Category { get; set; }
        public string CategoryText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerMessageCreateDto
    {
        public int CustomerId { get; set; }
        public int CompanyId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public MessageCategory Category { get; set; } = MessageCategory.General;
    }

    public class CustomerMessageReplyDto
    {
        public int MessageId { get; set; }
        public string Reply { get; set; } = string.Empty;
        public int RepliedByUserId { get; set; }
    }

    public class CustomerMessageSummaryDto
    {
        public int TotalMessages { get; set; }
        public int UnreadMessages { get; set; }
        public int PendingReplies { get; set; }
        public int RepliedMessages { get; set; }
        public List<CustomerMessageDto> RecentMessages { get; set; } = new();
    }
}
