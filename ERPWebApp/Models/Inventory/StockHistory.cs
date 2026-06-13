using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models.Inventory
{
    public class StockHistory
    {

        [Display(Name = "Modify Date")]
        [DataType(DataType.DateTime)]
        public DateTime ModifyDate { get; set; }

        [Display(Name = "Modified By User")]
        public string ModifyByUser { get; set; }

        [Display(Name = "Stock Id")]
        public int StockId { get; set; }

        [Display(Name = "Product Id")]
        public int ProductId { get; set; }

        [Display(Name = "Product Name")]
        public string Description { get; set; }

        [Display(Name = "Product Sku")]
        public string Sku { get; set; }

        [Display(Name = "Location Id")]
        public int LocationId { get; set; }

        [Display(Name = "Location")]
        public string Location { get; set; }

        [Display(Name = "Total Available")]
        public int TotalAvailable { get; set; }
        public List<StockHistory> Stock { get; set; }
    }
}
