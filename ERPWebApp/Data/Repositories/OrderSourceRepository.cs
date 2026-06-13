using ERPWebApp.Data.Repositories.Interface;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Data.Repositories
{
    public class OrderSourceRepository : Repository<OrderSource>, IOrderSourceRepository
    {
        public OrderSourceRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
