namespace ECommerce.Application.DTOs;

/// <summary>
/// Global özellik bilgisi DTO
/// </summary>
public record GlobalAttributeDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public int? CompanyId { get; init; }
    public string AttributeType { get; init; } = "Text";
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<GlobalAttributeValueDto>? Values { get; init; }
}

/// <summary>
/// Global özellik değeri DTO
/// </summary>
public record GlobalAttributeValueDto
{
    public int Id { get; init; }
    public int GlobalAttributeId { get; init; }
    public string Value { get; init; } = string.Empty;
    public string? DisplayValue { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
    public string? ColorCode { get; init; }
}

/// <summary>
/// Global özellik form DTO
/// </summary>
public record GlobalAttributeFormDto
{
    public int? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public string AttributeType { get; init; } = "Text";
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
    public List<GlobalAttributeValueFormDto>? Values { get; init; }
}

/// <summary>
/// Global özellik değeri form DTO
/// </summary>
public record GlobalAttributeValueFormDto
{
    public int? Id { get; init; }
    public string Value { get; init; } = string.Empty;
    public string? DisplayValue { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
    public string? ColorCode { get; init; }
}
