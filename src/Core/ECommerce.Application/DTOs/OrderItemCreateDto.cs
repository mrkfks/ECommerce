namespace ECommerce.Application.DTOs;

/// <summary>
/// Sipariş kalemi oluşturma DTO
/// </summary>
public record OrderItemCreateDto
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
