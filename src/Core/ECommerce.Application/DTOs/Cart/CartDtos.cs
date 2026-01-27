namespace ECommerce.Application.DTOs;

public record CartDto(
    int Id,
    decimal TotalAmount,
    List<CartItemDto> Items
);

public record CartItemDto(
    int Id,
    int ProductId,
    string ProductName,
    string ProductImage,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    int CompanyId
);

public record AddToCartDto(
    int ProductId,
    int Quantity
);

public record UpdateCartItemDto(
    int Quantity
);
