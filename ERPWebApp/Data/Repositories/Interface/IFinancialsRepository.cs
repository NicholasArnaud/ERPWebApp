using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Models.Reports;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IFinancialsRepository : IRepository<FinancialsViewModel>
    {
        Task<List<TrendsInfoDto>> TrendsTable(DateTime startDate, DateTime endDate);
        Task<List<ProductSalesInfoDto>> ProductSalesTable(int days);
        Task<List<FulfillmentInfoDto>> FulfillmentTable();
        Task<List<YearlyProfitInfoDto>> GetYearlyProfits(DateTime startDate, DateTime endDate);
        Task<List<WeeklyProfit>> GetWeeklyProfits(DateTime startDate, DateTime endDate);
    }
}
