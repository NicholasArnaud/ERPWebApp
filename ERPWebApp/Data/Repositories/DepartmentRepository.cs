using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Company;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories
{
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task DeleteDepartmentProduct(int productId)
        {
             await _context.Database.ExecuteSqlRawAsync("DELETE FROM DepartmentProduct WHERE ProductsProductId = {0}", productId);
        }    
    }
}