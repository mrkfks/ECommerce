namespace Dashboard.Web.Models
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
}
