using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Company;

namespace ERPWebApp.Models
{
    public class FinancialsViewModel
    {
        public TrendsInfoDto TrendsInfo { get; set; }
        public FulfillmentInfoDto FulfillmentInfo { get; set;}
        public List<ProductSalesInfoDto> ProductSalesInfo { get; set; }
        public int PieChartDays { get; set; }
        public int ColumnChartDays { get; set; }
        public string PieChartDataJson { get; set; }
        public string ColumnChartDataJson { get; set; }
        public string ProductSalesDataJson { get; set; }
        public string CurrentYearProfitsDataJson { get; set; }
        public string LastYearProfitsDataJson { get; set; }
        public List<DashboardLayout> DashboardLayouts { get; set; }
        public bool YearlyProfit { get; set; }
        public bool HistoricalTrends { get; set; }
        public bool TotalFulfillmentSales { get; set; }
        public bool TopProductSales { get; set; }

        public FinancialsViewModel()
        {
            TrendsInfo = new TrendsInfoDto();
            FulfillmentInfo = new FulfillmentInfoDto();
            ProductSalesInfo = new List<ProductSalesInfoDto>();
            DashboardLayouts = new List<DashboardLayout>();
        }
    }
}
