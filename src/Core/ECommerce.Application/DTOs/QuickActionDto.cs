namespace ECommerce.Application.DTOs;

public record OrderStatusUpdateDto(
    int OrderId,
    string Status
);

public record QuickOrderInfoDto(
    int Id,
    string CustomerName,
    string StatusText,
    decimal TotalAmount,
    DateTime OrderDate,
    int ItemCount
);
