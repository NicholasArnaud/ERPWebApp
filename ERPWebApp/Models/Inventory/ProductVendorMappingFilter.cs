using ERPWebApp.Models.Mappings;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace ERPWebApp.Models.Inventory
{
    public class ProductVendorMappingFilter
    {
        public List<ProductVendorMapping> ProductVendorMappingsProduct { get; set; }
        public List<ProductVendorMapping> ProductVendorMappingsVendor { get; set; }
        public SelectList Sku { get; set; }
        public SelectList Vendor { get; set; }
    }
}

