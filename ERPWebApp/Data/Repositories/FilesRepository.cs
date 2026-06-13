using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories
{
    public class FilesRepository : Repository<Files>, IFilesRepository
    {
        public FilesRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}