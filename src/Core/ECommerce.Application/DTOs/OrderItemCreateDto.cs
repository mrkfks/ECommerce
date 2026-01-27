namespace ECommerce.Application.DTOs;

public record OrderItemCreateDto(
    int ProductId,
    int Quantity
);
