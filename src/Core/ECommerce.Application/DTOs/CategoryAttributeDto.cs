namespace ECommerce.Application.DTOs
{
    public class CategoryAttributeDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; } = true;
        public List<CategoryAttributeValueDto> Values { get; set; } = new();
    }

    public class CategoryAttributeValueDto
    {
        public int Id { get; set; }
        public int CategoryAttributeId { get; set; }
        public required string Value { get; set; }
        public string? ColorCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CategoryAttributeFormDto
    {
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public List<string> Values { get; set; } = new(); // Basit değer listesi
    }
}
