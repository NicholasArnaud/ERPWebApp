using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.NirfForms;
using ERPWebApp.Services.IServices;
namespace ERPWebApp.Services
{
    public class NirfInventoryService : Service<NirfInventory>, INirfInventoryService
    {
        IUnitOfWork _unitOfWork;
        public NirfInventoryService(IUnitOfWork unitOfWork):base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
  
    }
}