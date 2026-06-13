using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Models.Reports;
using System.Linq.Expressions;

namespace ERPWebApp.Services.IServices
{
    public interface IFinancialsService
    {
        public Task<List<TrendsInfoDto>> TrendsTable(DateTime startDate, DateTime endDate);
        public Task<List<ProductSalesInfoDto>> ProductSalesTable(int days);
        public Task<List<FulfillmentInfoDto>> FulfillmentTable();
        Task<List<FinancialsViewModel>> GetListByFilterAsync(Expression<Func<FinancialsViewModel, bool>>[] predicates, params Expression<Func<FinancialsViewModel, string>>[] orderSelectors);
        List<FinancialsViewModel> GetListByFilter(Expression<Func<FinancialsViewModel, bool>>[] predicates, params Expression<Func<FinancialsViewModel, string>>[] orderSelectors);
        Task<List<YearlyProfitInfoDto>> GetYearlyProfitsData(DateTime startDate, DateTime endDate);
        Task<List<WeeklyProfit>> GetWeeklyProfits(DateTime startDate, DateTime endDate);
    }
}
