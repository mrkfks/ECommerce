namespace ECommerce.Application.DTOs;

public record OrderCreateDto(
    int CustomerId,
    int AddressId,
    int CompanyId,
    List<OrderItemCreateDto> Items,
    AddressFormDto? ShippingAddress = null,
    string? CardNumber = null,
    string? CardExpiry = null,
    string? CardCvv = null
);
