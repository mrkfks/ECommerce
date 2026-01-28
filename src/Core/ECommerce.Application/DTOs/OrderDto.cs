using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs;

/// <summary>
/// Sipariş bilgisi DTO
/// </summary>
public record OrderDto
{
    public int Id { get; init; }
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public int AddressId { get; init; }
    public int CompanyId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public DateTime OrderDate { get; init; }
    public decimal TotalAmount { get; init; }
    public OrderStatus Status { get; init; }
    public string StatusText { get; init; } = string.Empty;
    public AddressDto? Address { get; init; }
    public List<OrderItemDto>? Items { get; init; }
}
