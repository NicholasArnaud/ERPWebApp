using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class NirfImageMappingService : Service<NirfImageMapping>, INirfImageMappingService
    {
        IUnitOfWork _unitOfWork;
        public NirfImageMappingService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}