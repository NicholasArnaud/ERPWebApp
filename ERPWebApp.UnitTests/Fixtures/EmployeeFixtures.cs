using ERPWebApp.Models.Company;

namespace ERPWebApp.UnitTests.Fixtures
{
    public static class EmployeeFixtures
    {
        public static List<Employee> GetTestEmployeeices() => [
            new Employee
            {
                EmployeeId = 1,
                FirstName = "John",
                MiddleName = "",
                LastName = "Doe",
                FullName = "John Doe",
                UserRolesViewModelId = "user1",
                Position = "Manager",
                DepartmentId = 1,
                PhoneNumber = "(123) 456-7890",
                PersonalEmail = "john.doe@gmail.com",
                CompanyEmail = "jdoe@completeful.com",
                JobStatus = JobStatus.FullTime,
                IncomePerHour = 25.00M,
                ModifyDate = DateTime.Now,
                ModifyBy = "admin",
                EmployeeReferenceNumber = "1234",
                ApsuId = "apsu123",
                Department = new Department()
                {
                    DepartmentId = 5, DepartmentName = "Business Analysis", IsActive = true, IsProduction = true
                }
            },
            new Employee
            {
                EmployeeId = 2,
                FirstName = "Jane",
                MiddleName = "E.",
                LastName = "Smith",
                FullName = "Jane E. Smith",
                UserRolesViewModelId = "user2",
                Position = "Supervisor",
                DepartmentId = 2,
                PhoneNumber = "(555) 555-1212",
                PersonalEmail = "jane.smith@hotmail.com",
                CompanyEmail = "jsmith@company.com",
                JobStatus = JobStatus.PartTime,
                IncomePerHour = 15.50M,
                ModifyDate = DateTime.Now,
                ModifyBy = "admin",
                EmployeeReferenceNumber = "5678",
                ApsuId = "apsu456",
                Department = new Department()
                {
                    DepartmentId = 5, DepartmentName = "Business Analysis", IsActive = true, IsProduction = true
                }
            },
            new Employee
            {
                EmployeeId = 3,
                FirstName = "Mike",
                MiddleName = "J.",
                LastName = "Brown",
                FullName = "Mike J. Brown",
                UserRolesViewModelId = "user3",
                Position = "Worker",
                DepartmentId = 3,
                PhoneNumber = "(555) 555-5555",
                PersonalEmail = "mike.brown@gmail.com",
                CompanyEmail = "mbrown@company.com",
                JobStatus = JobStatus.FullTime,
                IncomePerHour = 20.00M,
                ModifyDate = DateTime.Now,
                ModifyBy = "admin",
                EmployeeReferenceNumber = "9012",
                ApsuId ="apsu456",
                Department = new Department()
                {
                    DepartmentId = 5, DepartmentName = "Business Analysis", IsActive = true, IsProduction = true
                }
            },
            new Employee
            {
                EmployeeId = 4,
                FirstName = "John",
                MiddleName = "Nolan",
                LastName = "White",
                FullName = "John Nolan White",
                UserRolesViewModelId = "user4",
                Position = "Worker",
                DepartmentId = 4,
                PhoneNumber = "(555) 555-5555",
                PersonalEmail = "john.nolan@gmail.com",
                CompanyEmail = "white@company.com",
                JobStatus = JobStatus.FullTime,
                IncomePerHour = 20.00M,
                ModifyDate = DateTime.Now,
                ModifyBy = "admin",
                EmployeeReferenceNumber = "9012",
                ApsuId ="apsu456",
                Department = new Department()
                {
                    DepartmentId = 5, DepartmentName = "Business Analysis", IsActive = true, IsProduction = true
                }
            },
            new Employee
            {
                EmployeeId = 5,
                FirstName = "John",
                MiddleName = "",
                LastName = "Doe",
                FullName = "John Doe",
                UserRolesViewModelId = "user1",
                Position = "Manager",
                DepartmentId = 1,
                PhoneNumber = "(123) 456-7890",
                PersonalEmail = "john.doe@gmail.com",
                CompanyEmail = "jdoe@invalid.com",
                JobStatus = JobStatus.FullTime,
                IncomePerHour = 25.00M,
                ModifyDate = DateTime.Now,
                ModifyBy = "admin",
                EmployeeReferenceNumber = "1234",
                ApsuId = "apsu123",
                Department = new Department()
                {
                    DepartmentId = 5, DepartmentName = "Business Analysis", IsActive = true, IsProduction = true
                }
            },
        ];
    }
}