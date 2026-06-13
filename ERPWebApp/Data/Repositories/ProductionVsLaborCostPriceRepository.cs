using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories
{
    public class ProductionVsLaborCostPriceRepository : Repository<ProductionVsLaborCostPrice>, IProductionVsLaborCostPriceRepository
    {
        public ProductionVsLaborCostPriceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ProductionVsLaborCostPrice> GetLastProductionVsLaborCostPrice()
        {
            return await _context.ProductionVsLaborCostPrice.OrderByDescending(x => x.ModifyDate).FirstOrDefaultAsync();
        }
    }
}