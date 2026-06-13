using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Data.Repositories
{
    public class OrderAdvancedOptionsRepository : Repository<OrderAdvancedOptions>, IOrderAdvancedOptionsRepository
    {
        public OrderAdvancedOptionsRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
