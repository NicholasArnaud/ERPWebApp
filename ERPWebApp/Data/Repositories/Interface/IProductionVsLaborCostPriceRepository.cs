using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IProductionVsLaborCostPriceRepository : IRepository<ProductionVsLaborCostPrice>
    {
        Task<ProductionVsLaborCostPrice> GetLastProductionVsLaborCostPrice();
    }
}