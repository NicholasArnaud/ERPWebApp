using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Data;
using System.Linq.Expressions;
using ERPWebApp.Models.Reports;

namespace ERPWebApp.Services
{
    public class FinancialsService : IFinancialsService
    {
        IUnitOfWork _unitOfWork;
        public FinancialsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<TrendsInfoDto>> TrendsTable(DateTime startDate, DateTime endDate)
        {
            var financialsInformation = await _unitOfWork.Financials.TrendsTable(startDate, endDate);
            return financialsInformation;
        }

        public async Task<List<ProductSalesInfoDto>> ProductSalesTable(int days)
        {
            var financialsInformation = await _unitOfWork.Financials.ProductSalesTable(days);
            return financialsInformation;
        }
        public async Task<List<FulfillmentInfoDto>> FulfillmentTable()
        {
            var financialsInformation = await _unitOfWork.Financials.FulfillmentTable();
            return financialsInformation;
        }

        public async Task<List<FinancialsViewModel>> GetListByFilterAsync(Expression<Func<FinancialsViewModel, bool>>[] predicates, Expression<Func<FinancialsViewModel, string>>[] orderSelectors)
        {
            var financialsListFilterAsync = await _unitOfWork.Financials.GetListByFilterAsync(predicates, orderSelectors);
            return financialsListFilterAsync.OfType<FinancialsViewModel>().ToList();
        }

        public List<FinancialsViewModel> GetListByFilter(Expression<Func<FinancialsViewModel, bool>>[] predicates, params Expression<Func<FinancialsViewModel, string>>[] orderSelectors)
        {
            var financialsListFilter = _unitOfWork.Financials.GetListByFilter(predicates, orderSelectors);
            return financialsListFilter.OfType<FinancialsViewModel>().ToList();
        }

        public async Task<List<YearlyProfitInfoDto>> GetYearlyProfitsData(DateTime startDate, DateTime endDate)
        {
            var yearlyProfits = await _unitOfWork.Financials.GetYearlyProfits(startDate, endDate);
            return yearlyProfits;
        }

        public async Task<List<WeeklyProfit>> GetWeeklyProfits(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.Financials.GetWeeklyProfits(startDate,endDate);
        } 
    }
}
