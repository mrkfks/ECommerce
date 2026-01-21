namespace ECommerce.Application.DTOs
{
    public class ProductCreateDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int CompanyId { get; set; }
        public int? ModelId { get; set; }
        public string? ImageUrl { get; set; }
    }
}
