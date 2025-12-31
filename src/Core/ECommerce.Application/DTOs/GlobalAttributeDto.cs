namespace ECommerce.Application.DTOs
{
    public class GlobalAttributeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public int AttributeType { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<GlobalAttributeValueDto> Values { get; set; } = new();
    }

    public class GlobalAttributeValueDto
    {
        public int Id { get; set; }
        public int GlobalAttributeId { get; set; }
        public string Value { get; set; } = string.Empty;
        public string? ColorCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class GlobalAttributeCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AttributeType { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public List<GlobalAttributeValueCreateDto> Values { get; set; } = new();
    }

    public class GlobalAttributeValueCreateDto
    {
        public string Value { get; set; } = string.Empty;
        public string? ColorCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class GlobalAttributeUpdateDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AttributeType { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public List<GlobalAttributeValueUpdateDto> Values { get; set; } = new();
    }

    public class GlobalAttributeValueUpdateDto
    {
        public int? Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public string? ColorCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
