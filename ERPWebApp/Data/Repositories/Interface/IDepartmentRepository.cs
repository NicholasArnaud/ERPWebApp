using ERPWebApp.Models.Company;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        Task DeleteDepartmentProduct(int productId);
    }
}