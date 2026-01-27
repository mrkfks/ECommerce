namespace Dashboard.Web.Models
{
    public class RecentOrderVm
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public DateTime OrderDate { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
}
