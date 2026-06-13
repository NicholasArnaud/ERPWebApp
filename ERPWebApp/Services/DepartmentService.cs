using ERPWebApp.Data;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class DepartmentService : Service<Department>, IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DepartmentService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public List<AssignedDepartmentViewModel> PopulateAssignedDepartment(Product product)
        {
            var viewModel = new List<AssignedDepartmentViewModel>();
            try
            {
                var allDepartments = _unitOfWork.Departments.GetAll();

                foreach (var dept in product.Departments)
                {
                    if (dept == null)
                    {
                        product.Departments.Remove(dept);
                    }
                }

                if (product.Departments != null)
                {
                    var productDepartments = new HashSet<int>(
                        product.Departments.Select(c => c.DepartmentId)
                    );

                    foreach (var dept in allDepartments.Where(x => x.IsActive))
                    {
                        viewModel.Add(
                            new AssignedDepartmentViewModel
                            {
                                DepartmentId = dept.DepartmentId,
                                DepartmentName = dept.DepartmentName,
                                isProduction = dept.IsProduction,
                                Assigned = productDepartments.Contains(dept.DepartmentId)
                            }
                        );
                    }
                }

                return viewModel;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return viewModel;
            }
        }

        public Product UpdateProductDepartments(string[] selectedDepartments, Product productToUpdate)
        {
            var allDepartments = _unitOfWork.Departments.GetAll();
            if (selectedDepartments == null)
            {
                productToUpdate.Departments = new List<Department>();
                return productToUpdate;
            }

            var selectedDepartmentsHS = new HashSet<string>(selectedDepartments);
            if (productToUpdate.Departments != null)
            {
                var productDepartments = new HashSet<int>(
                    productToUpdate.Departments.Select(c => c.DepartmentId)
                );

                foreach (var department in allDepartments)
                {
                    if (selectedDepartmentsHS.Contains(department.DepartmentId.ToString()))
                    {
                        if (!productDepartments.Contains(department.DepartmentId))
                        {
                            productToUpdate.Departments.Add(department);
                            
                        }
                    }
                    else
                    {
                        if (productDepartments.Contains(department.DepartmentId))
                        {
                            productToUpdate.Departments.Remove(department);
                        }
                    }
                }
            }
            else
            {
                productToUpdate.Departments = new List<Department>();
                foreach (var department in allDepartments)
                {
                    if (selectedDepartmentsHS.Contains(department.DepartmentId.ToString()))
                    {
                        productToUpdate.Departments.Add(department);
                        
                    }
                }
            }

            return productToUpdate;
        }

        public async Task DeleteDepartmentProduct(int productId)
        {
           await _unitOfWork.Departments.DeleteDepartmentProduct(productId);
        }
    }
}