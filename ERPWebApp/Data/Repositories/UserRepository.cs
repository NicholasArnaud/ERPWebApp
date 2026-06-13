using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace ERPWebApp.Data.Repositories
{
    public class UserRepository : Repository<IdentityUser>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<UserRoleModel>> GetAllUsersInRole()
        {
            var usersInRole = await (
                 from user in _context.Users
                 join userRole in _context.UserRoles on user.Id equals userRole.UserId
                 join role in _context.Roles on userRole.RoleId equals role.Id
                 where
                     role.Name == RoleList.ProductionManager
                     || role.Name == RoleList.ShippingManager
                     || role.Name == RoleList.CustomerSupportManager
                     || role.Name == RoleList.FinancialManager
                     || role.Name == RoleList.InventoryManager
                     || role.Name == RoleList.Administrator
                 select new UserRoleModel { UserName = user.UserName, RoleName = role.Name, Id = user.Id }).ToListAsync();

            return usersInRole;
        }
    }
}