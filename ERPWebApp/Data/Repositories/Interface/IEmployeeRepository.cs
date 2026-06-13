using ERPWebApp.Models.Company;
using Newtonsoft.Json.Linq;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<JObject> GetEmployeeErrorsByDate(DateTime date);
    }
}