using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Data.Repositories
{
    public class BatchItemStatusRepository : Repository<BatchItemStatus>, IBatchItemStatusRepository
    {
        public BatchItemStatusRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
