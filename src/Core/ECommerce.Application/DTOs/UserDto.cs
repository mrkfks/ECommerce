namespace ECommerce.Application.DTOs;

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
