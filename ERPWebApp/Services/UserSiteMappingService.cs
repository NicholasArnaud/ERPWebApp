using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Services.IServices;
namespace ERPWebApp.Services;
public class UserSiteMappingService(IUnitOfWork unitOfWork) : Service<UserSiteMapping>(unitOfWork), IUserSiteMappingService { }