namespace Dashboard.Web.Models
{
    public class SalesKpiVm
    {
        public decimal DailySales { get; set; }
        public decimal MonthlySales { get; set; }
        public decimal YesterdaySales { get; set; }
        public decimal DailySalesChange { get; set; }
        public string DailySalesFormatted => DailySales.ToString("C");
        public string YesterdaySalesFormatted => YesterdaySales.ToString("C");

        public decimal AverageOrderValue { get; set; }
        public decimal AverageOrderValueChange { get; set; }
        public string AverageOrderValueFormatted => AverageOrderValue.ToString("C");
        public decimal MonthlyTarget { get; set; }
        public string MonthlySalesFormatted => MonthlySales.ToString("C");
        public string MonthlyTargetFormatted => MonthlyTarget.ToString("C");
        public decimal MonthlySalesChange { get; set; }
        public decimal WeeklySales { get; set; }
        public decimal LastWeekSales { get; set; }
        public string WeeklySalesFormatted => WeeklySales.ToString("C");
        public string LastWeekSalesFormatted => LastWeekSales.ToString("C");
        public decimal WeeklySalesChange { get; set; }
    }
}
