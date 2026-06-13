using ERPWebApp.Data.DTOModels;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
namespace ERPWebApp.Services.IServices
{
    public interface IUserService
    {
        Task<List<UserRoleModel>> GetUsersInRole();
        IdentityUser Get(Expression<Func<IdentityUser, bool>> expression = null);
        Task<List<IdentityUser>> GetList(Expression<Func<IdentityUser, bool>> expression);
        Task<IdentityResult> CreateUserForEmployee(string companyEmail, int departmentId);
    }
}