using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
using ERPWebApp.Services.IServices;
namespace ERPWebApp.Services
{
    public class NirfVendorMappingService : Service<NirfVendorMapping>, INirfVendorMappingService
    {
        IUnitOfWork _unitOfWork;
        public NirfVendorMappingService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}