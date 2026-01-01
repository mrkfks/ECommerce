namespace ECommerce.Domain.Entities
{
    public class CustomerMessage : BaseEntity, ITenantEntity
    {
        private CustomerMessage() { }

        public int CustomerId { get; private set; }
        public int CompanyId { get; private set; }
        public string Subject { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;
        public bool IsRead { get; private set; }
        public bool IsReplied { get; private set; }
        public string? Reply { get; private set; }
        public DateTime? RepliedAt { get; private set; }
        public int? RepliedByUserId { get; private set; }
        public MessageCategory Category { get; private set; } = MessageCategory.General;

        // İlişkiler
        public virtual Customer? Customer { get; private set; }
        public virtual Company? Company { get; private set; }
        public virtual User? RepliedBy { get; private set; }

        public static CustomerMessage Create(int customerId, int companyId, string subject, string message, MessageCategory category = MessageCategory.General)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Konu boş olamaz.", nameof(subject));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Mesaj boş olamaz.", nameof(message));

            return new CustomerMessage
            {
                CustomerId = customerId,
                CompanyId = companyId,
                Subject = subject,
                Message = message,
                Category = category,
                IsRead = false,
                IsReplied = false
            };
        }

        public void MarkAsRead()
        {
            IsRead = true;
            MarkAsModified();
        }

        public void SendReply(string reply, int repliedByUserId)
        {
            if (string.IsNullOrWhiteSpace(reply))
                throw new ArgumentException("Yanıt boş olamaz.", nameof(reply));

            Reply = reply;
            IsReplied = true;
            RepliedAt = DateTime.UtcNow;
            RepliedByUserId = repliedByUserId;
            MarkAsModified();
        }
    }

    public enum MessageCategory
    {
        General,
        Order,
        Product,
        Return,
        Complaint,
        Suggestion
    }
}
