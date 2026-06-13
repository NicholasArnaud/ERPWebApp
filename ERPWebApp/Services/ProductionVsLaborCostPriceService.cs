using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class ProductionVsLaborCostPriceService : Service<ProductionVsLaborCostPrice>, IProductionVsLaborCostPriceService
    {
        public ProductionVsLaborCostPriceService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}