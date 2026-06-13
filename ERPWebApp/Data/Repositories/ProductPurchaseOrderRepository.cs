using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.PurchaseOrders;

namespace ERPWebApp.Data.Repositories
{
    public class ProductPurchaseOrderRepository : Repository<ProductPurchaseOrder>, IProductPurchaseOrderRepository
    {
        public ProductPurchaseOrderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}