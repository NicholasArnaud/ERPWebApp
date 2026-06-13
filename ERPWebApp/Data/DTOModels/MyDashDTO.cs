namespace ERPWebApp.Data.DTOModels
{
    public class MyDashDTO
    {
        public bool SpeedOMeter { get; set; }
        public bool DepartmentOrderHistory { get; set; }
        public bool TopDepartment { get; set; }
        public bool YearlyProfit { get; set; }
        public bool HistoricalTrends { get; set; }
        public bool TotalFulfillmentSales { get; set; }
        public bool TopProductSales { get; set; }
        public bool SiteVolumetrics { get; set; }
        public bool ProductCyleCount { get; set; }
        public bool TopRequestedProducts { get; set; }
        public bool TopMovedProducts { get; set; }
        public bool TopReasonRequest { get; set; }
    }


    public class OperationsDTO
    {
        public bool SpeedOMeter { get; set; }
        public bool DepartmentOrderHistory { get; set; }
        public bool TopDepartment { get; set; }
    }
}
