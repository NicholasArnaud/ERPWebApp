using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models
{
    public class ShipStationOrderedHistory
    {
        public int ShipStationOrderedHistoryId { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        [Display(Name = "Total Available")]
        public int TotalFromAllLocations { get; set; }
        [Display(Name = "On Order")]
        public int OnOrder { get; set; }
        [Display(Name = "Lead Time")]
        public int LeadTime { get; set; }
        [Display(Name = "In 24 Hours")]
        public int OrderedIn24Hours { get; set; }
        [Display(Name = "In 3 Days")]
        public int OrderedIn3Days { get; set; }
        [Display(Name = "In 7 Days")]
        public int OrderedIn7Days { get; set; }
        [Display(Name = "In 15 Days")]
        public int OrderedIn15Days { get; set; }
        [Display(Name = "In 30 Days")]
        public int OrderedIn30Days { get; set; }
        [Display(Name = "In 90 Days")]
        public int OrderedIn90Days { get; set; }
        //[Display(Name = "In 120 Days")]
        //public int OrderedIn120Days { get; set; }
        [Display(Name = "Sales Trend")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalesTrend { get; set; }
    }
}
