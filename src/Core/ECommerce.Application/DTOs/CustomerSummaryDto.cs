namespace ECommerce.Application.DTOs;

public record CustomerSummaryDto(
    int Id,
    string Name,
    string Email,
    string PhoneNumber,
    int OrderCount = 0,
    int ReviewCount = 0,
    int TotalOrders = 0,
    decimal TotalSpent = 0,
    string? ImageUrl = null
);
