using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
namespace ERPWebApp.Data.Repositories
{
    public class SubCategoryRepository : Repository<SubCategory>, ISubCategoryRepository
    {
        public SubCategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}