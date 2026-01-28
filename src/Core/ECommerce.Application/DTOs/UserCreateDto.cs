namespace ECommerce.Application.DTOs;

/// <summary>
/// Kullanıcı oluşturma DTO
/// </summary>
public record UserCreateDto
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? RoleName { get; init; }
    public List<string>? Roles { get; init; }
    public int? CompanyId { get; init; }
    public bool IsActive { get; init; } = true;
}
