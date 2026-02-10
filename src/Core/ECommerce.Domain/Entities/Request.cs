namespace ECommerce.Domain.Entities;

public class Request : IAuditable, ITenantEntity
{
    private Request() { }

    public int Id { get; private set; }
    public int CompanyId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public RequestStatus Status { get; private set; } = RequestStatus.Pending;
    public string? Feedback { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public Company? Company { get; private set; }

    public static Request Create(int companyId, string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("İstek başlığı boş olamaz.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("İstek açıklaması boş olamaz.", nameof(description));

        return new Request
        {
            CompanyId = companyId,
            Title = title,
            Description = description,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Approve(string? feedback = null)
    {
        if (!string.IsNullOrWhiteSpace(feedback))
            Feedback = feedback;

        Status = RequestStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string? feedback = null)
    {
        if (!string.IsNullOrWhiteSpace(feedback))
            Feedback = feedback;

        Status = RequestStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkInProgress()
    {
        if (Status != RequestStatus.Approved)
            throw new InvalidOperationException("Yalnızca onaylanmış istekler devam eden duruma taşınabilir.");

        Status = RequestStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != RequestStatus.InProgress)
            throw new InvalidOperationException("Yalnızca devam eden istekler tamamlanabilir.");

        Status = RequestStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum RequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    InProgress = 3,
    Completed = 4
}
