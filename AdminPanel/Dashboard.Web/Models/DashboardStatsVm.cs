using System.Text.Json;
using System.Collections.Generic;

namespace Dashboard.Web.Models
{
    public class DashboardStatsVm
    {
        public List<TopProductVm> TopProducts { get; set; } = new();
        public List<LowStockProductVm> LowStockProducts { get; set; } = new();
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewSignUps { get; set; }
        public int TotalRevenue { get; set; }

        // Dashboard cards
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalSales { get; set; }


        public OrderKpiVm Orders { get; set; } = new();
        public CustomerKpiVm Customers { get; set; } = new();
        public SalesKpiVm Sales { get; set; } = new();
        public CustomerSegmentationVm CustomerSegmentation { get; set; } = new();

        // JSON chart data for view
        public string OrderStatusJson { get; set; } = "[]";
        public string CustomerSegmentJson { get; set; } = "[]";
        public string RevenueTrendJson { get; set; } = "[]";
    }
}