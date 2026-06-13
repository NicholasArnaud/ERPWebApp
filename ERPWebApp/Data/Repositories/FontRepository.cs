using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
namespace ERPWebApp.Data.Repositories
{
    public class FontRepository : Repository<Fonts>, IFontRepository
    {
        public FontRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}