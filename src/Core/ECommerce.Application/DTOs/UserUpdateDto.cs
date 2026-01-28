namespace ECommerce.Application.DTOs;

/// <summary>
/// Kullanıcı güncelleme DTO
/// </summary>
public record UserUpdateDto
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public List<string>? Roles { get; init; }
    public int? CompanyId { get; init; }
    public bool IsActive { get; init; } = true;
}
