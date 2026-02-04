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

/// <summary>
/// Ürün değerlendirme özeti DTO
/// </summary>
public record ReviewSummaryDto
{
    public int ProductId { get; init; }
    public double AverageRating { get; init; }
    public int TotalReviews { get; init; }
    public Dictionary<int, int> RatingDistribution { get; init; } = new();
}

/// <summary>
/// Kullanıcının yorum yapabilirlik durumu DTO
/// </summary>
public record CanReviewDto
{
    public bool CanReview { get; init; }
    public bool HasPurchased { get; init; }
    public bool HasReviewed { get; init; }
}
