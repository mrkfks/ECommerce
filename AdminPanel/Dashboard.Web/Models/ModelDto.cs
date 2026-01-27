namespace Dashboard.Web.Models
{
    public class ModelDto
    {
        public int Id { get; set; }
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
