using ERPWebApp.Models.Company;
using System.ComponentModel.DataAnnotations;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Models
{
    public class BatchViewModel
    {
        public class BatchView
        {
            public string batchNumber { get; set; }
            public int totalQuantity { get; set; }
            public string status { get; set; }
            public string createDate { get; set; }
            public List<Department> departments { get; set; }
        }

        public class ProductDetail
        {
            public string productSku { get; set; }
            public int quantity { get; set; }
        }

        public class ProductDetailWithOrderBatchItem
        {
            [Display(Name = "Product ID")]
            public int productId { get; set; }
            [Display(Name = "Product SKU")]
            public string productSku { get; set; }
            [Display(Name = "Quantity")]
            public int quantity { get; set; }
            [Display(Name = "Order Batch Item")]
            public int orderBatchItem { get; set; }
            [Display(Name = "Cost")]
            public decimal cost { get; set; }
            [Display(Name = "Custom Cost")]
            public decimal customCost { get; set; }
            [Display(Name = "Discount")]

            public decimal discount { get; set; }
            [Display(Name = "Product")]
            public virtual Product Product { get; set; }
        }
        public class OrderDetail
        {
            public string orderNumber { get; set; }
            public int ERPOrderId { get; set; }
        }
    }
}
