using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;
using Microsoft.AspNetCore.Mvc.Rendering;
using ERPWebApp.Models.PurchaseOrders;
using System.ComponentModel.DataAnnotations;


namespace ERPWebApp.Models.PurchaseOrders
{
    public class PurchaseOrderViewModel
    {
        public IEnumerable<PurchaseOrder> PurchaseOrders { get; set; }
        public PurchaseOrder PurchaseOrderSingle { get; set; }
        public IEnumerable<ProductPurchaseOrder> ProductPurchaseOrders { get; set; }
        public ProductPurchaseOrder ProductPurchaseOrderSingle { get; set; }
        public SelectList Vendor { get; set; }
        public IEnumerable<Stock> productStock { get; set; }
        public List<ProductPurchaseOrderStockMapping> ProductPurchaseOrderStockMappings { get; set; }
        public IEnumerable<PurchaseOrderFilesMapping> purchaseOrderFilesMappings { get; set; }
        public IEnumerable<DetailPurchaseOrderModel> detailPurchaseOrderModels { get; set; }
        public List<MiscProduct> MiscProducts { get; set; } = new List<MiscProduct>();
        [Display(Name = "Batch Number")]
        public string BatchNumber { get; set; }

        public class DetailPurchaseOrderModel
        {
            public int ProductId { get; set; }
            public string Sku { get; set; }
            public int? Quantity { get; set; }
            public decimal? VendorCost { get; set; }
            public decimal? Discount { get; set; }
            public decimal? Shipping { get; set; }
            public decimal? Tax { get; set; }
            public decimal? OtherCost { get; set; }
            public decimal? CostPerItem { get; set; }
            public decimal? TotalCost { get; set; }
            public int? Open { get; set; }
            public int? TotalReceived { get; set; }
            public decimal? AverageCost { get; set; }
            public DateTime? ExpectedDate { get; set; }

        }
    }
}
