using ERPWebApp.Data.Repositories.Interface;

namespace ERPWebApp.Data.Repositories
{
    public class UserEmailAlertMappingRepository : Repository<UserEmailAlertMappingRepository>, IUserEmailAlertMappingRepository
    {
        public UserEmailAlertMappingRepository(ApplicationDbContext context) : base(context)
        {

        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}