using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using Newtonsoft.Json.Linq;

namespace ERPWebApp.Services.IServices
{
    public interface IEmployeeService : IService<Employee>
    {
        Task<ProductionVsLaborCostPrice> GetLastProductionVsLaborCostPrice();
        Task<JObject> GetEmployeeErrors();
    }
}