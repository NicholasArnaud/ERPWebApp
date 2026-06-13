using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    public class MyDash
    {
        [Key]
        public string UserId { get; set; }
        [Display(Name = "Speedo Meter")]
        public bool SpeedOMeter { get; set; }
        [Display(Name = "Department Order History")]
        public bool DepartmentOrderHistory { get; set; }
        [Display(Name = "Top Department")]
        public bool TopDepartment { get; set; }
        [Display(Name = "Yearly Profit")]
        public bool YearlyProfit { get; set; }
        [Display(Name = "Historical Trends")]
        public bool HistoricalTrends { get; set; }
        [Display(Name = "Total Fulfillment Sales")]
        public bool TotalFulfillmentSales { get; set; }
        [Display(Name = "Top Product Sales")]
        public bool TopProductSales { get; set; }
        [Display(Name = "Site Volumetrics")]
        public bool SiteVolumetrics { get; set; }
        [Display(Name = "Product CyleCount")]
        public bool ProductCyleCount { get; set; }
        [Display(Name = "Top Requested Products")]
        public bool TopRequestedProducts { get; set; }
        [Display(Name = "Top Moved Products")]
        public bool TopMovedProducts { get; set; }
        [Display(Name = "Top Reason Request")]
        public bool TopReasonRequest { get; set; }
    }
}
