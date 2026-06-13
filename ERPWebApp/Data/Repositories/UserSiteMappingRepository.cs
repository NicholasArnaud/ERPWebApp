using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Mappings;
namespace ERPWebApp.Data.Repositories;
public class UserSiteMappingRepository(ApplicationDbContext context) : Repository<UserSiteMapping>(context), IUserSiteMappingRepository { }