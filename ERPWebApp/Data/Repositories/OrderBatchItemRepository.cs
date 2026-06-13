using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Data.Repositories
{
    public class OrderBatchItemRepository : Repository<OrderBatchItem>, IOrderBatchItemRepository
    {
        public OrderBatchItemRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
