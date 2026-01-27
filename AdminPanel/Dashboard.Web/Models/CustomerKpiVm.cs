namespace Dashboard.Web.Models
{
    public class CustomerKpiVm
    {
        public int TotalCustomers { get; set; }
        public int DailyNewCustomers { get; set; }
        public int MonthlyNewCustomers { get; set; }
        public double CustomerGrowthRate { get; set; }
    }
}
