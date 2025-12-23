namespace Dashboard.Web.Models
{
    public class DashboardStatsVm
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewSignUps { get; set; }
        public int TotalRevenue { get; set; }

        // Dashboard cards
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalSales { get; set; }
    }
}