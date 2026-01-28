namespace ECommerce.Application.DTOs;

/// <summary>
/// Müşteri oluşturma/güncelleme form DTO
/// </summary>
public record CustomerFormDto
{
    public int? Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public int CompanyId { get; init; }
    public int? UserId { get; init; }
}

/// <summary>
/// Müşteri özet bilgisi DTO
/// </summary>
public record CustomerSummaryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public int OrderCount { get; init; }
    public int ReviewCount { get; init; }
    public int TotalOrders { get; init; }
    public decimal TotalSpent { get; init; }
    public string? ImageUrl { get; init; }
}
