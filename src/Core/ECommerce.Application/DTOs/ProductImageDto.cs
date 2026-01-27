namespace ECommerce.Application.DTOs;

public record ProductImageDto(
    int Id,
    int ProductId,
    string ImageUrl,
    int Order,
    bool IsPrimary
);
