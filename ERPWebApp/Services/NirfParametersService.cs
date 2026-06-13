using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
using ERPWebApp.Services.IServices;
namespace ERPWebApp.Services
{
    public class NirfParametersService : Service<NirfParameters> ,INirfParametersService
    {
        IUnitOfWork _unitOfWork;
        public NirfParametersService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}