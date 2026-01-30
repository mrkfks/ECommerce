namespace Dashboard.Web.Models
{
    public class TopProductDto
    {
        public int Id { get; set; }
        public int ProductId => Id;
        public string Name { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        // Razor view için eklenen propertyler
        public string ProductName => Name;
        public string RevenueFormatted => Revenue.ToString("C");

        // Ürün görseli için eklenen property
        public string ImageUrl { get; set; } = string.Empty;
    }
}
