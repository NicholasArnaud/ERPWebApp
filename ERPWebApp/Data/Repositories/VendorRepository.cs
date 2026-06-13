using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Data.Repositories
{
    public class VendorRepository : Repository<Vendor>, IVendorRepository
    {
        public VendorRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}