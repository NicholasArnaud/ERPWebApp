using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
using ERPWebApp.Services.IServices;
namespace ERPWebApp.Services
{
    public class NirfShippingService : Service<NirfShipping>, INirfShippingService
    {
        IUnitOfWork _unitOfWork;
        public NirfShippingService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}