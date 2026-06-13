using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.PurchaseOrders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Mappings
{
    public class ProductPurchaseOrderStockMapping
    {
        [Key]
        public int ProductPurchaseOrderStockMappingId { get; set; }
        [Display(Name = "ProductPurchaseOrder")]
        public int ProductPurchaseOrderId { get; set; }
        [ForeignKey("ProductPurchaseOrderId")]
        public virtual ProductPurchaseOrder ProductPurchaseOrder { get; set; }
        [Display(Name = "Stock")]
        public int StockId { get; set; }
        [ForeignKey("StockId")]
        public virtual Stock Stock { get; set; }
        [Display(Name = "Quantity Recieved")]
        public int QtyRecieved { get; set; }
    }
}
