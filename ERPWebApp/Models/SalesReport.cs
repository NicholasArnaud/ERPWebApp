using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models
{
    public class SalesReport
    {
        [Key]
        public int SalesReportId { get; set; }
        public string Sku { get; set; }
        [Display(Name = "Quantity")]
        public int QuantitySold { get; set; }
        [Display(Name = "Cost Per Item")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPerItem { get; set; }
        [Display(Name = "Item Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }
        [Display(Name = "Shipping Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; }

    }
}
