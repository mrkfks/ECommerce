namespace ECommerce.Application.DTOs;

/// <summary>
/// Müşteri güncelleme DTO
/// </summary>
public record CustomerUpdateDto
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
}
