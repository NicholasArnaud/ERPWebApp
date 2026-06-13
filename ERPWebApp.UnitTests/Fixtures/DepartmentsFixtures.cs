using ERPWebApp.Models.Company;

namespace ERPWebApp.UnitTests.Fixtures
{
    public class DepartmentsFixtures
    {
        public static List<Department> GetTestDepartments() =>
        [
            new Department()
            {
                 DepartmentId = 1, DepartmentName = "Sales", IsActive = true, IsProduction = false
            },
            new Department()
            {
                DepartmentId = 5, DepartmentName = "Business Analysis", IsActive = true, IsProduction = true
            },
            new Department()
            {
                DepartmentId = 2, DepartmentName = "Marketing", IsActive = true, IsProduction = false
            },
            new Department()
            {
                DepartmentId = 4, DepartmentName = "Human Resources", IsActive = true, IsProduction = false
            },
            new Department()
            {
                DepartmentId = 3, DepartmentName = "IT", IsActive = true, IsProduction = true
            },
            new Department()
            {
                DepartmentId = 6, DepartmentName = "External", IsActive = true, IsProduction = false
            }
        ];
    }
}