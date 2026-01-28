using ECommerce.Domain.Entities;

namespace Dashboard.Web.Models
{
    public class GlobalAttributeCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Value { get; set; }
        public AttributeType AttributeType { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public List<GlobalAttributeValueFormDto>? Values { get; set; }
    }

    public class GlobalAttributeValueFormDto
    {
        public string Value { get; set; } = string.Empty;
    }
}
