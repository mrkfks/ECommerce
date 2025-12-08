namespace ECommerce.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int StockQuantity { get; set; }

        public required virtual Category Category { get; set; }
        public required virtual Brand Brand { get; set; }
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}