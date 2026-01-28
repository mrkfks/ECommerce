namespace ECommerce.Application.DTOs;

/// <summary>
/// Sipariş oluşturma DTO
/// </summary>
public record OrderCreateDto
{
    public int CustomerId { get; init; }
    public int AddressId { get; init; }
    public int CompanyId { get; init; }
    public List<OrderItemCreateDto>? Items { get; init; }
    
    // Kargo adresi (opsiyonel - Address kullanılabilir)
    public ShippingAddressDto? ShippingAddress { get; init; }
    
    // Ödeme bilgileri (opsiyonel)
    public string? CardNumber { get; init; }
    public string? CardExpiry { get; init; }
    public string? CardCvv { get; init; }
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
