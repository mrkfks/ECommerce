namespace ECommerce.Application.DTOs;

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
