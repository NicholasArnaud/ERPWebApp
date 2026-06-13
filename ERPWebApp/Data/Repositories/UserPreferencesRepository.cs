using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Company;

namespace ERPWebApp.Data.Repositories
{
    public class UserPreferencesRepository : Repository<UserPreferences>, IUserPreferencesRepository
    {
        public UserPreferencesRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}