using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs;

public record OrderUpdateDto(
    int Id,
    OrderStatus OrderStatus,
    int? AddressId = null
);
