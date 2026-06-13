using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.PurchaseOrders;

namespace ERPWebApp.Data.Repositories
{
    public class ShippingMethodRepository : Repository<ShippingMethod>, IShippingMethodRepository
    {
        public ShippingMethodRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}