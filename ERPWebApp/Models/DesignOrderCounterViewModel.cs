using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    public class DesignOrderCounterViewModel
    {
        public int DesignOrderViewModelId { get; set; }
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; }
        public string Sku { get; set; }
        [Display(Name = "Item Name")]
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        [Display(Name = "Order Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime OrderDate { get; set; }
        [Display(Name = "Ship Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime ShipDate { get; set; }
        [Display(Name = "Design Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DesignDate { get; set; }
        [Display(Name = "Designer Name")]
        public string DesignerName { get; set; }
        [Display(Name = "Custom Field 3")]
        public string CustomField3 { get; set; }

    }
}
