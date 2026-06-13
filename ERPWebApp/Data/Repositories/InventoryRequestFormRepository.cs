using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Data.Repositories
{
    public class InventoryRequestFormRepository : Repository<InventoryRequestForm>, IInventoryRequestFormRepository
    {
        public InventoryRequestFormRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}