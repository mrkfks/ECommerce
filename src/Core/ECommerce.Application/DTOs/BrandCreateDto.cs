namespace ECommerce.Application.DTOs
{
    public class BrandCreateDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class BrandUpdateDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public bool IsActive { get; set; }
    }
}
