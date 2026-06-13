using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Services.IServices
{
    public interface IDepartmentService : IService<Department>
    {
        List<AssignedDepartmentViewModel> PopulateAssignedDepartment(Product product);
        Product UpdateProductDepartments(string[] selectedDepartments, Product productToUpdate);
        Task DeleteDepartmentProduct(int productId);
    }
}