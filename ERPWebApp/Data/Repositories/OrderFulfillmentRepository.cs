using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Data.Repositories
{
    public class OrderFulfillmentRepository : Repository<OrderFulfillment>, IOrderFulfillmentRepository
    {
        public OrderFulfillmentRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}