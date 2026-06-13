using ERPWebApp.Data;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class DepartmentRoleMappingService  : Service<DepartmentRoleMapping>, IDepartmentRoleMappingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentRoleMappingService(IUnitOfWork unitOfWork):base(unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }
    }
} 
