namespace ECommerce.Application.DTOs;

public record ProductBulkUpdateDto(
    List<int> ProductIds,
    decimal PriceIncreasePercentage
);
