using ERPWebApp.Models.Company;

namespace ERPWebApp.Models
{
    public class MessageEmployeeFilter
    {
        public IEnumerable<Employee> Employees { get; set; }
        public List<MessageEmployee> MessageEmployeeList { get; set; }
    }
}
