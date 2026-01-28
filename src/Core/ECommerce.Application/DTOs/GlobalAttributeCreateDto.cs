using ECommerce.Domain.Entities;

namespace ECommerce.Application.DTOs;

/// <summary>
/// Global özellik oluşturma DTO
/// </summary>
public record GlobalAttributeCreateDto
{
    public string Name { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public string? Value { get; init; }
    public AttributeType AttributeType { get; init; } = AttributeType.Text;
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
    public List<GlobalAttributeValueFormDto>? Values { get; init; }
}
