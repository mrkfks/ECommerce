namespace ECommerce.Application.DTOs;

public record AddressDto(
    int Id,
    int CustomerId,
    string? CustomerName,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    int? CompanyId = null,
    string? CompanyName = null
);
