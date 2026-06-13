using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories
{
    public class SkulabsImportRepository : Repository<SkulabsImport>, ISkulabsImportRepository
    {
        public SkulabsImportRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
