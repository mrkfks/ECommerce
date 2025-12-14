namespace ECommerce.Domain.Entities;

public class Request
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public string? Feedback { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Company? Company { get; set; }
}

public enum RequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    InProgress = 3,
    Completed = 4
}
