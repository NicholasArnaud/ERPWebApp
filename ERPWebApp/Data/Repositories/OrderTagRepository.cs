using ERPWebApp.Data.Repositories.Interface;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Data.Repositories
{
    public class OrderTagRepository : Repository<OrderTag>, IOrderTagRepository
    {
        public OrderTagRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
