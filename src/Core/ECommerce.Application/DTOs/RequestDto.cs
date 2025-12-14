namespace ECommerce.Application.DTOs
{
    public class RequestDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Status { get; set; } // 0: Pending, 1: Approved, 2: Rejected, 3: InProgress, 4: Completed
        public string? Feedback { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class RequestCreateDto
    {
        public int CompanyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class RequestUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? Feedback { get; set; }
    }

    public class RequestFeedbackDto
    {
        public string? Feedback { get; set; }
    }
}
