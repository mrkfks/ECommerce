namespace ECommerce.Application.DTOs;

public class ProductBulkUpdateDto
{
    public List<int> ProductIds { get; set; } = new();
    public decimal PriceIncreasePercentage { get; set; }
}
