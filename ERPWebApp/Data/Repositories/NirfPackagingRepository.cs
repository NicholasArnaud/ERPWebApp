using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;

namespace ERPWebApp.Data.Repositories
{
    public class NirfPackagingRepository : Repository<NirfPackaging>, INirfPackagingRepository
    {
        public NirfPackagingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}