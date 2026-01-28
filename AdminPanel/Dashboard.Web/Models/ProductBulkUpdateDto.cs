namespace Dashboard.Web.Models
{
    /// <summary>
    /// Toplu ürün güncelleme DTO'su
    /// </summary>
    public class ProductBulkUpdateDto
    {
        public List<int> ProductIds { get; set; } = new();
        public decimal? PriceIncreasePercentage { get; set; }
        public decimal? NewPrice { get; set; }
        public int? StockQuantity { get; set; }
        public bool? IsActive { get; set; }
    }
}
