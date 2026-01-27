namespace ECommerce.Application.DTOs;

public record AddressFormDto(
    int? Id,
    int CustomerId,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country = "Turkey",
    int? CompanyId = null
);
