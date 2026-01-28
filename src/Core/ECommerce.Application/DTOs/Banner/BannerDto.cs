namespace ECommerce.Application.DTOs.Banner;

/// <summary>
/// Banner bilgisi DTO
/// </summary>
public record BannerDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? Link { get; init; }
    public int Order { get; init; }
    public bool IsActive { get; init; } = true;
    public int CompanyId { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Banner form DTO
/// </summary>
public record BannerFormDto
{
    public int? Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Link { get; init; }
    public int Order { get; init; }
    public bool IsActive { get; init; } = true;
    public int? CompanyId { get; init; }
}

/// <summary>
/// Banner oluşturma DTO
/// </summary>
public record CreateBannerDto
{
    public string Title { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Link { get; init; }
    public int Order { get; init; }
    public bool IsActive { get; init; } = true;
    public int? CompanyId { get; init; }
}

/// <summary>
/// Banner güncelleme DTO
/// </summary>
public record UpdateBannerDto
{
    public string Title { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Link { get; init; }
    public int Order { get; init; }
    public bool IsActive { get; init; } = true;
}
