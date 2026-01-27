namespace Dashboard.Web.Models
{
    public class CustomerSegmentationVm
    {
        public int NewCustomers { get; set; }
        public int ReturningCustomers { get; set; }
        public decimal NewCustomersRevenue { get; set; }
        public decimal ReturningCustomersRevenue { get; set; }
        public double NewCustomerPercent { get; set; }
        public double ReturningCustomerPercent { get; set; }
    }
}
