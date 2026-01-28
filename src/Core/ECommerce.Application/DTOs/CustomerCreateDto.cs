namespace ECommerce.Application.DTOs;

/// <summary>
/// Müşteri oluşturma DTO
/// </summary>
public record CustomerCreateDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public int CompanyId { get; init; }
    public int? UserId { get; init; }
}
