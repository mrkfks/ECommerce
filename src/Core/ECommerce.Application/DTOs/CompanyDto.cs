namespace ECommerce.Application.DTOs;

public record CompanyDto(
    int Id,
    string Name,
    string? Description,
    string TaxNumber,
    string Email,
    string PhoneNumber,
    string Address,
    bool IsActive = true,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null
);

public record CompanyFormDto(
    int? Id,
    string Name,
    string? Description = null,
    string TaxNumber = "",
    string Email = "",
    string PhoneNumber = "",
    string Address = "",
    bool IsActive = true
);

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

public record CustomerFormDto(
    int? Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateTime DateOfBirth,
    int CompanyId,
    int? UserId = null
);

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

public record UserDto(
    int Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    int? CompanyId = null,
    string? CompanyName = null,
    List<string>? Roles = null,
    bool IsActive = true,
    DateTime? CreatedAt = null
);

public record UserFormDto(
    int? Id,
    string Username,
    string Email,
    string? Password = null, // Only for Create
    string FirstName = "",
    string LastName = "",
    string? RoleName = null, // For simplify single role assignment
    List<string>? Roles = null, // For multiple role management
    int? CompanyId = null,
    bool IsActive = true
);

public record UserProfileUpdateDto(
    string FirstName,
    string LastName,
    string? Email = null,
    string? PhoneNumber = null,
    string? ProfilePictureUrl = null
);

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

public record ReviewFormDto(
    int? Id,
    int? ProductId = null,
    int? CustomerId = null,
    int? CompanyId = null,
    string? ReviewerName = null,
    int Rating = 0,
    string Comment = ""
);