namespace ECommerce.Application.DTOs
{
    public class CategoryCreateDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }

    public class CategoryUpdateDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
