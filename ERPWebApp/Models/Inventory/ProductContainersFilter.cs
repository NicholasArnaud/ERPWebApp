using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERPWebApp.Models.Inventory
{
    public class ProductContainersFilter
    {
        public List<ProductContainer> ProductContainers { get; set; }
        public SelectList Sku { get; set; }
        public SelectList Vendor { get; set; }
    }
}
