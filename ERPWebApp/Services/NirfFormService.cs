using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class NirfFormService : Service<NirfForm>, INirfFormService
    {
        IUnitOfWork _unitOfWork;

        public NirfFormService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IQueryable<NirfForm> GetAllNirfFormIdById(int nirfFormId)
        {
            return _unitOfWork.NirfForms.GetAllNirfFormIdById(nirfFormId);
        }
    }
}