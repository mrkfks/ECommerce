namespace ECommerce.Application.DTOs
{
    public class BrandCreateDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }

    public class BrandUpdateDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
