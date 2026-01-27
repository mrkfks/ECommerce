namespace Dashboard.Web.Models
{
    public class ModelCreateDto
    {
        public int BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
