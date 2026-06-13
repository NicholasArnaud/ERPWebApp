using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Mappings;

namespace ERPWebApp.Data.Repositories
{
    public class PurchaseOrderFilesMappingRepository : Repository<PurchaseOrderFilesMapping>, IPurchaseOrderFilesMappingRepository
    {
        public PurchaseOrderFilesMappingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}