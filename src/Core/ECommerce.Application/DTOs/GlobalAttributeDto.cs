namespace ECommerce.Application.DTOs;

public record GlobalAttributeDto(
    int Id,
    string Name,
    string DisplayName,
    string Description,
    int CompanyId,
    int AttributeType,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<GlobalAttributeValueDto> Values
);

public record GlobalAttributeValueDto(
    int Id,
    int GlobalAttributeId,
    string Value,
    string? ColorCode,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record GlobalAttributeFormDto(
    int? Id,
    string Name,
    string DisplayName,
    string Description,
    int AttributeType,
    int DisplayOrder,
    bool IsActive = true,
    List<GlobalAttributeValueFormDto>? Values = null
);

public record GlobalAttributeValueFormDto(
    int? Id,
    string Value,
    string? ColorCode = null,
    int DisplayOrder = 0,
    bool IsActive = true
);
