namespace ECommerce.Application.DTOs;

/// <summary>
/// Adres form DTO
/// </summary>
public record AddressFormDto
{
    public int? Id { get; init; }
    public int CustomerId { get; init; }
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}
