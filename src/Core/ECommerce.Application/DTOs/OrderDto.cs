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

/// <summary>
/// Sipariş oluşturma/güncelleme form DTO
/// </summary>
public record OrderFormDto
{
    public int? Id { get; init; }
    public int CustomerId { get; init; }
    public int AddressId { get; init; }
    public int? CompanyId { get; init; }
    public OrderStatus Status { get; init; }
    public List<OrderItemFormDto>? Items { get; init; }

    // Kargo adresi (opsiyonel - Address kullanılabilir)
    public ShippingAddressDto? ShippingAddress { get; init; }

    // Ödeme bilgileri (opsiyonel)
    public string? CardNumber { get; init; }
    public string? CardExpiry { get; init; }
    public string? CardCvv { get; init; }
}

/// <summary>
/// Sipariş kalemi form DTO
/// </summary>
public record OrderItemFormDto
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

/// <summary>
/// Kargo adresi DTO
/// </summary>
public record ShippingAddressDto
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}
