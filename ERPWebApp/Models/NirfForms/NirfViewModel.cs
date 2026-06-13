using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.PurchaseOrders;

namespace ERPWebApp.Models.NirfForms
{
    public class NirfViewModel
    {
        public NirfForm NirfForms { get; set; }
        public NirfForecasting NirfForecastings { get; set; }
        public NirfInventory NirfInventories { get; set; } = new();
        public NirfPackaging NirfPackagings { get; set; }
        public NirfParameters NirfParameters { get; set; }
        public NirfShipping NirfShippings { get; set; }
        public NirfVendorMapping NirfVendorMapping { get; set; }
        public Vendor Vendor { get; set; }
        public IEnumerable<NirfImageMapping> NirfImageMapping { get; set; }
        public NirfProductMapping NirfProductMapping { get; set; }
        public IList<NirfShippingProdivder> NirfShippingProvider { get; set; }
        public IEnumerable<ShippingProvider> ShippingProviders { get; set; }
        public List<Product> NirfProducts { get; set; }
    }
}
