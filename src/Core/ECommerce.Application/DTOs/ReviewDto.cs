namespace ECommerce.Application.DTOs;

public record ReviewDto(
    int Id,
    int ProductId,
    string? ProductName = null,
    int CustomerId,
    string? CustomerName = null,
    int CompanyId,
    string? ReviewerName = null,
    int Rating,
    string Comment,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null
);
