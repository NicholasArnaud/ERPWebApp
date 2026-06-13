using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class SiteService : Service<Site>, ISiteService
    {
        IUnitOfWork _unitOfWork;
        public SiteService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

    }
}