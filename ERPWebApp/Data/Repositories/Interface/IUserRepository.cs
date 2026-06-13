using ERPWebApp.Data.DTOModels;
using Microsoft.AspNetCore.Identity;
namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IUserRepository : IRepository<IdentityUser>
    {
        Task<List<UserRoleModel>> GetAllUsersInRole();
    }
}