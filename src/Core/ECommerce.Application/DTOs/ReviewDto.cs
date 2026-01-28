namespace ECommerce.Application.DTOs;

/// <summary>
/// Yorum bilgisi DTO
/// </summary>
public record ReviewDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public int CustomerId { get; init; }
    public int CompanyId { get; init; }
    public int Rating { get; init; }
    public string Comment { get; init; } = string.Empty;
    public string? ProductName { get; init; }
    public string? CustomerName { get; init; }
    public string? ReviewerName { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Yorum oluşturma/güncelleme form DTO
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
