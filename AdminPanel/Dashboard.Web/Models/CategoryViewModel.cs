using ECommerce.Application.DTOs;

namespace Dashboard.Web.Models
{
    /// <summary>
    /// Kategori ekleme/düzenleme için ViewModel - tüm ilişkili verileri içerir
    /// </summary>
    public class CategoryViewModel
    {
        // Kategori Bilgileri
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        // Markalar (kategoriye özel)
        public List<BrandItemVm> Brands { get; set; } = new();

        // Özellikler (kategoriye özel)
        public List<AttributeItemVm> Attributes { get; set; } = new();

        // Dropdown için veri
        public List<CategoryDto> AvailableParentCategories { get; set; } = new();
    }

    public class BrandItemVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public List<ModelItemVm> Models { get; set; } = new();
    }

    public class ModelItemVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class AttributeItemVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; } = true;
        public List<AttributeValueItemVm> Values { get; set; } = new();
    }

    public class AttributeValueItemVm
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public string? ColorCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
