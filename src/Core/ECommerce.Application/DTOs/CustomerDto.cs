namespace ECommerce.Application.DTOs;

public record CustomerDto(
    int Id,
    string FirstName,
    string LastName,
    string Name, // Full name
    string Email,
    string PhoneNumber,
    DateTime DateOfBirth,
    int CompanyId,
    string? CompanyName = null,
    int? UserId = null,
    int TotalOrders = 0,
    decimal TotalSpent = 0,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null
);