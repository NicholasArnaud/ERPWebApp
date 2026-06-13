using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.PurchaseOrders;

namespace ERPWebApp.Data.Repositories
{
    public class ShippingProviderRepository : Repository<ShippingProvider>, IShippingProviderRepository
    {
        public ShippingProviderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}