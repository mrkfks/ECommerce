using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs;

/// <summary>
/// Sipariş güncelleme DTO
/// </summary>
public record OrderUpdateDto
{
    public int Id { get; init; }
    public int AddressId { get; init; }
    public OrderStatus Status { get; init; }
}
