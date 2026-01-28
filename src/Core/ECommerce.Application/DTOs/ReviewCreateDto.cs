namespace ECommerce.Application.DTOs;

/// <summary>
/// Yorum oluşturma DTO
/// </summary>
public record ReviewCreateDto
{
    public int ProductId { get; init; }
    public int CustomerId { get; init; }
    public int CompanyId { get; init; }
    public int Rating { get; init; }
    public string Comment { get; init; } = string.Empty;
    public string? ReviewerName { get; init; }
}

/// <summary>
/// Yorum güncelleme DTO
/// </summary>
public record ReviewUpdateDto
{
    public int Id { get; init; }
    public int Rating { get; init; }
    public string Comment { get; init; } = string.Empty;
}

/// <summary>
/// Yorum form DTO
/// </summary>
public record ReviewFormDto
{
    public int? Id { get; init; }
    public int ProductId { get; init; }
    public int CustomerId { get; init; }
    public int CompanyId { get; init; }
    public int Rating { get; init; }
    public string Comment { get; init; } = string.Empty;
    public string? ReviewerName { get; init; }
}
