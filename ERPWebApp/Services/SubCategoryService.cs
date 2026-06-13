using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class SubCategoryService : Service<SubCategory>, ISubCategoryService
    {
        IUnitOfWork _unitOfWork;
        public SubCategoryService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}