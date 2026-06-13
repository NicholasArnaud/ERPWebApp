using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
namespace ERPWebApp.Data.Repositories
{
    public class NirfInventoryRepository : Repository<NirfInventory>, INirfInventoryRepository
    {
        public NirfInventoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}