using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs;

public record OrderDto(
    int Id,
    int CustomerId,
    string CustomerName,
    int AddressId,
    int CompanyId,
    string CompanyName,
    DateTime OrderDate,
    decimal TotalAmount,
    OrderStatus Status,
    string StatusText,
    AddressDto? Address = null,
    List<OrderItemDto>? Items = null
);
