using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Company;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class UserImageService : Service<UserImage>, IUserImageService
    {
        public UserImageService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}