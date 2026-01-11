using ECommerce.Application.DTOs.Common;

namespace ECommerce.Application.DTOs.Banner
{
    public class BannerDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Link { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateBannerDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Link { get; set; }
        public int Order { get; set; }
    }

    public class UpdateBannerDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Link { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }
}
