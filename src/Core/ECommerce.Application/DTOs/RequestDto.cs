namespace ECommerce.Application.DTOs;

/// <summary>
/// Talep/İstek bilgisi DTO
/// </summary>
public record RequestDto
{
    public int Id { get; init; }
    public int CompanyId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Status { get; init; }
    public string? Response { get; init; }
    public string? Feedback { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Talep form DTO
/// </summary>
public record RequestFormDto
{
    public int? Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Status { get; init; }
    public string? Response { get; init; }
    public string? Feedback { get; init; }
    public int? CompanyId { get; init; }
    public int? CreatedByUserId { get; init; }
}

/// <summary>
/// Talep oluşturma DTO (RequestFormDto'nun alias'ı - geriye uyumluluk için)
/// </summary>
public record RequestCreateDto
{
    public int CompanyId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int? CreatedByUserId { get; init; }
    public DateTime? CreatedAt { get; init; }
}
