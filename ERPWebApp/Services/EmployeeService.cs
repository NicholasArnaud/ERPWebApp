using ERPWebApp.Data;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using ERPWebApp.Services.IServices;
using Newtonsoft.Json.Linq;

namespace ERPWebApp.Services
{
    public class EmployeeService : Service<Employee>, IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime _now;
        public EmployeeService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _now = TimeZoneInfo.ConvertTime(
                DateTime.Now,
                TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
            );
        }
        public async Task<JObject> GetEmployeeErrors()
        {
            return await _unitOfWork.Employees.GetEmployeeErrorsByDate(_now);
        }

        public async Task<ProductionVsLaborCostPrice> GetLastProductionVsLaborCostPrice()
        {
            return await _unitOfWork.ProductionVsLaborCostPrices.GetLastProductionVsLaborCostPrice();
        }

        private decimal GetPartialCost(DateTime partialHour, decimal Income, bool isStarthour)
        {
            decimal cost = 0;

            // If statement that returns the same value 
            //  start hour resembles the start of shift then calculates cost of start partial hour 
            //   if not start function calculates cost of end partial hour 
            if (isStarthour)
            {
                cost = (60 - partialHour.Minute) * (Income / 60);
            }
            else
            {
                cost = (partialHour.Minute) * (Income / 60);
            }
            return cost;
        }

    }
}