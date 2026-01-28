using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs;

/// <summary>
/// Sipariş durumu güncelleme için DTO (Swagger uyumlu)
/// </summary>
public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}
