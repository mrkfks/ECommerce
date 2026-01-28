namespace ECommerce.Application.DTOs;

/// <summary>
/// Adres bilgisi DTO
/// </summary>
public record AddressDto
{
    public int Id { get; init; }
    public int CustomerId { get; init; }
    public string? CustomerName { get; init; }
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public int? CompanyId { get; init; }
    public string? CompanyName { get; init; }
}

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
