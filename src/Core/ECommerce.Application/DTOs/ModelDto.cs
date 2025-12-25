namespace ECommerce.Application.DTOs
{
    public class ModelDto
    {
        public int Id { get; set; }
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ModelCreateDto
    {
        public int BrandId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ModelUpdateDto
    {
        public int Id { get; set; }
        public int BrandId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
