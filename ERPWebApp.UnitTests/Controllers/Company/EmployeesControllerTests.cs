using ERPWebApp.Controllers.Company;
using ERPWebApp.Models.Company;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers.Company
{
    [Trait("Category", "execute")]
    public class EmployeesControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock = new(
            Mock.Of<IUserStore<IdentityUser>>(),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );
        private readonly Mock<IEmployeeService> _employeeServiceMock = new();
        private readonly Mock<IDepartmentService> _departmentServiceMock = new();
        private readonly Mock<IUserService> _userServiceMock= new();
        private EmployeesController _controller;

        public EmployeesControllerTests()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.Administrator),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller = new EmployeesController(
                _userManagerMock.Object,
                _employeeServiceMock.Object,
                _departmentServiceMock.Object,
                _userServiceMock.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
                }
            };
        }

        #region Unit Tests for Index

        [Fact]
        public async Task Index_ReturnsViewResult_WithAListOfActiveEmployees()
        {
            //prepare test employee list
            var activeEmployees = EmployeeFixtures.GetTestEmployeeices();
            _employeeServiceMock.Setup(s => s.GetListAsync(
                    It.IsAny<Expression<Func<Employee, bool>>>(),
                    It.IsAny<Expression<Func<Employee, string>>[]>(),
                    It.IsAny<Expression<Func<Employee, object>>[]>())
                )
                .ReturnsAsync(activeEmployees);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Employee>>(viewResult.ViewData.Model);
            Assert.Equal(activeEmployees.Count, model.Count());
            Assert.All(model, e => Assert.True(e.JobStatus!=JobStatus.Terminated));
            Assert.All(model, e => Assert.NotNull(e.Department));
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithEmptyListWhenNoActiveEmployees()
        {
            _ = _employeeServiceMock.Setup(static s => s.GetListAsync(
                    It.IsAny<Expression<Func<Employee, bool>>>(),
                    It.IsAny<Expression<Func<Employee, string>>[]>(),
                    It.IsAny<Expression<Func<Employee, object>>[]>()))
                .ReturnsAsync([]);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Employee>>(viewResult.ViewData.Model);
            Assert.Empty(model);
        }

        #endregion


        #region Unit Tests for ToggleActive

        [Fact]
        public void ToggleActive_ShouldReturnActiveEmployees_WhenIdIsTrue()
        {
            //prepare test employee list
            var activeEmployees = EmployeeFixtures.GetTestEmployeeices().Where(static e => e.JobStatus!=JobStatus.Terminated);
            _ = _employeeServiceMock.Setup(static s => s.GetList(It.IsAny<Func<IQueryable<Employee>, IQueryable<Employee>>>()))
                                .Returns(activeEmployees.ToList());

            var result = _controller.ToggleActive(true);

            Assert.NotNull(result);
            Assert.All(result, static employee => Assert.True(employee.JobStatus != JobStatus.Terminated));
        }

        [Fact]
        public void ToggleActive_ShouldReturnAllEmployees_WhenIdIsFalse()
        {
            //prepare test employee list
            var allEmployees = EmployeeFixtures.GetTestEmployeeices();
            _ = _employeeServiceMock.Setup(static s => s.GetList(It.IsAny<Func<IQueryable<Employee>, IQueryable<Employee>>>()))
                                .Returns(allEmployees.ToList());

            var result = _controller.ToggleActive(false);

            Assert.NotNull(result);
            Assert.Equal(allEmployees.Count, result.Count);
        }

        #endregion


        #region Unit Tests for PartialViewTableShow

        [Fact]
        public void PartialViewTableShow_ReturnsCorrectViewAndModel()
        {
            var employees = EmployeeFixtures.GetTestEmployeeices().Where(static _ => _.JobStatus != JobStatus.Terminated).ToList();
            //using reflection to set value to _employeeDbFull variable
            var employeeDbFullField = typeof(EmployeesController).GetField("_employeeDbFull", BindingFlags.Static | BindingFlags.NonPublic);
            employeeDbFullField.SetValue(null, employees);

            var result = _controller.PartialViewTableShow() as PartialViewResult;

            Assert.NotNull(result);
            Assert.Equal("PartialIndex", result.ViewName);
            Assert.Equal(employees, result.Model);
        }

        #endregion


        #region Unit Tests for Details

        [Fact]
        public async Task Details_ValidId_ReturnsViewResultWithEmployee()
        {
            var mockEmployee = EmployeeFixtures.GetTestEmployeeices().First();
            _ = _employeeServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<Employee, bool>>>(), It.IsAny<Expression<Func<Employee, object>>[]>()))
                                .ReturnsAsync(mockEmployee);

            var result = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Employee>(viewResult.ViewData.Model);
            Assert.Equal(mockEmployee, model);
        }

        [Fact]
        public async Task Details_ValidIdNoEmployee_ReturnsNotFound()
        {
            _ = _employeeServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<Employee, bool>>>(), It.IsAny<Expression<Func<Employee, object>>[]>()))
                                .ReturnsAsync((Employee)null);

            var result = await _controller.Details(1);

            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var result = await _controller.Details(null);

            _ = Assert.IsType<NotFoundResult>(result);
        }

        #endregion


        #region Unit Tests for Create Get

        [Fact]
        public void Create_ShouldPopulateViewDataCorrectly()
        {
            var departmentList = DepartmentsFixtures.GetTestDepartments();
            _ = _departmentServiceMock.Setup(
                static s => s.GetList(It.IsAny<Func<IQueryable<Department>, IQueryable<SelectListItem>>>())
            ).Returns(
                departmentList.Select(static x => new SelectListItem
                {
                    Value = x.DepartmentId.ToString(),
                    Text = x.DepartmentName
                }
                ).ToList()
            );

            var result = _controller.Create() as ViewResult;

            Assert.NotNull(result);

            Assert.NotNull(result.ViewData["UserRolesViewModel"]);
            _ = Assert.IsType<SelectList>(result.ViewData["UserRolesViewModel"]);

            Assert.NotNull(result.ViewData["Department"]);
            _ = Assert.IsType<SelectList>(result.ViewData["Department"]);
        }

        #endregion


        #region Unit Tests for Create Post

        [Fact]
        public async Task Create_Post_ValidModel_ReturnsRedirectToActionResult()
        {
            var employee = EmployeeFixtures.GetTestEmployeeices().First();

            var result = await _controller.Create(employee);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            _employeeServiceMock.Verify(static s => s.AddAsync(It.IsAny<Employee>()), Times.Once);
        }
        [Fact]
        public async Task Create_Post_InvalidEmailDomain_ReturnsViewWithError()
        {
            var employee = EmployeeFixtures.GetTestEmployeeices().FirstOrDefault(static employee => employee.EmployeeId == 5);
            var result = await _controller.Create(employee);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState, static ms => ms.Value.Errors.Any(static e => e.ErrorMessage == "The Email needs to be part of the @ERP.com domain."));
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewResultWithModel()
        {
            var employee = EmployeeFixtures.GetTestEmployeeices().First();
            _controller.ModelState.AddModelError("Error", "Model State is Invalid");

            //set department list
            var departments = DepartmentsFixtures.GetTestDepartments();
            _ = _departmentServiceMock.Setup(
                static s => s.GetList(It.IsAny<Func<IQueryable<Department>, IQueryable<SelectListItem>>>())
            ).Returns(
                departments.Select(static x => new SelectListItem
                {
                    Value = x.DepartmentId.ToString(),
                    Text = x.DepartmentName
                }
                ).ToList()
            );

            var result = await _controller.Create(employee);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(employee, viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_EmployeeExists_ReturnsViewResultWithModel()
        {
            var employee = EmployeeFixtures.GetTestEmployeeices().First();

            _controller.ControllerContext.HttpContext = new DefaultHttpContext();

            //set employee exists to true
            _ = _employeeServiceMock.Setup(static e => e.IsExists(It.IsAny<Expression<Func<Employee, bool>>>())).Returns(true);

            //set department list
            var departments = DepartmentsFixtures.GetTestDepartments();
            _ = _departmentServiceMock.Setup(
                static s => s.GetList(It.IsAny<Func<IQueryable<Department>, IQueryable<SelectListItem>>>())
            ).Returns(
                departments.Select(static x => new SelectListItem
                {
                    Value = x.DepartmentId.ToString(),
                    Text = x.DepartmentName
                }
                ).ToList()
            );

            var result = await _controller.Create(employee);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(employee, viewResult.Model);
        }

        #endregion


        #region Unit Tests for Edit Get

        [Fact]
        public async Task Edit_WithValidId_ReturnsViewResultWithEmployee()
        {
            var validId = 1;
            var employee = EmployeeFixtures.GetTestEmployeeices().First();

            var departmentList = DepartmentsFixtures.GetTestDepartments();
            _ = _departmentServiceMock.Setup(
                static s => s.GetList(It.IsAny<Func<IQueryable<Department>, IQueryable<SelectListItem>>>())
            ).Returns(
                departmentList.Select(static x => new SelectListItem
                {
                    Value = x.DepartmentId.ToString(),
                    Text = x.DepartmentName
                }
                ).ToList()
            );

            _ = _employeeServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<Employee, bool>>>(), null))
                                .ReturnsAsync(employee);

            var result = await _controller.Edit(validId);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(employee, viewResult.Model);
            Assert.NotNull(viewResult.ViewData["Department"]);
            Assert.NotNull(viewResult.ViewData["UserRolesViewModel"]);
        }

        [Fact]
        public async Task Edit_WithNullId_ReturnsNotFoundResult()
        {
            int? nullId = null;

            var result = await _controller.Edit(nullId);

            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WithNonExistingId_ReturnsNotFoundResult()
        {
            // Arrange
            var nonExistingId = 999;
            _ = _employeeServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<Employee, bool>>>(), null))
                                .ReturnsAsync((Employee?)null);

            var result = await _controller.Edit(nonExistingId);

            _ = Assert.IsType<NotFoundResult>(result);
        }

        #endregion


        #region Unit Tests for Edit Post

        [Fact]
        public async Task Edit_ValidData_ReturnsRedirectToActionResult()
        {
            var validEmployee = EmployeeFixtures.GetTestEmployeeices().First();

            _ = _employeeServiceMock.Setup(static service => service.UpdateAsync(It.IsAny<Employee>()))
                                .ReturnsAsync(1);
            _ = _employeeServiceMock.Setup(static service => service.IsExists(It.IsAny<Expression<Func<Employee, bool>>>()))
                                .Returns(true);

            var result = await _controller.Edit(validEmployee.EmployeeId, validEmployee);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Edit_InvalidData_ReturnsViewWithEmployeeModel()
        {
            var invalidEmployee = EmployeeFixtures.GetTestEmployeeices().First();
            invalidEmployee.FirstName = "";

            _controller.ModelState.AddModelError("Error", "Sample model error");

            _ = _employeeServiceMock.Setup(static service => service.UpdateAsync(It.IsAny<Employee>()))
                                .ReturnsAsync(1);
            _ = _employeeServiceMock.Setup(static service => service.IsExists(It.IsAny<Expression<Func<Employee, bool>>>()))
                                .Returns(true);

            var result = await _controller.Edit(invalidEmployee.EmployeeId, invalidEmployee);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Employee>(viewResult.Model);
            Assert.Equal(invalidEmployee, model);
        }

        [Fact]
        public async Task Edit_MismatchedEmployeeId_ReturnsNotFound()
        {
            var invalidEmployeeId = 3;
            var mismatchedEmployee = EmployeeFixtures.GetTestEmployeeices().First();

            _ = _employeeServiceMock.Setup(static service => service.UpdateAsync(It.IsAny<Employee>()))
                                .ReturnsAsync(1);
            _ = _employeeServiceMock.Setup(static service => service.IsExists(It.IsAny<Expression<Func<Employee, bool>>>()))
                                .Returns(true);

            var result = await _controller.Edit(invalidEmployeeId, mismatchedEmployee);

            _ = Assert.IsType<NotFoundResult>(result);
        }


        [Fact]
        public async Task Edit_NoMiddleName_FullNameConstructedCorrectly()
        {
            var validEmployee = EmployeeFixtures.GetTestEmployeeices().First();
            validEmployee.MiddleName = "";

            _ = _employeeServiceMock.Setup(static service => service.UpdateAsync(It.IsAny<Employee>()))
                                .ReturnsAsync(1);
            _ = _employeeServiceMock.Setup(static service => service.IsExists(It.IsAny<Expression<Func<Employee, bool>>>()))
                                .Returns(true);

            var result = await _controller.Edit(validEmployee.EmployeeId, validEmployee);

            Assert.Equal(validEmployee.FirstName + " " + validEmployee.LastName, validEmployee.FullName);
        }

        [Fact]
        public async Task Edit_UserRoleReset_ApsuIdSetToNull()
        {
            var validEmployee = EmployeeFixtures.GetTestEmployeeices().First();
            validEmployee.UserRolesViewModelId = "Reset";

            _ = _employeeServiceMock.Setup(static service => service.UpdateAsync(It.IsAny<Employee>()))
                                .ReturnsAsync(1);
            _ = _employeeServiceMock.Setup(static service => service.IsExists(It.IsAny<Expression<Func<Employee, bool>>>()))
                                .Returns(true);

            _ = await _controller.Edit(validEmployee.EmployeeId, validEmployee);

            Assert.Null(validEmployee.ApsuId);
        }

        #endregion


        #region Unit Tests for Delete Get

        [Fact]
        public async Task Delete_WithValidIdAndExistingEmployee_ReturnsView()
        {
            var employee = EmployeeFixtures.GetTestEmployeeices().First();
            _ = _employeeServiceMock.Setup(static service => service.GetAsync(It.IsAny<Expression<Func<Employee, bool>>>(), It.IsAny<Expression<Func<Employee, object>>[]>()))
                       .ReturnsAsync(employee);

            var result = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            _ = Assert.IsType<Employee>(viewResult.Model);
            Assert.Equal(viewResult.Model, employee);
        }

        [Fact]
        public async Task Delete_WithValidIdAndNonExistingEmployee_ReturnsNotFound()
        {
            var mockService = new Mock<IEmployeeService>();
            _ = mockService.Setup(static service => service.GetAsync(It.IsAny<Expression<Func<Employee, bool>>>(), It.IsAny<Expression<Func<Employee, object>>[]>()))
                       .ReturnsAsync((Employee?)null);


            var result = await _controller.Delete(1);

            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WithNullId_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);

            _ = Assert.IsType<NotFoundResult>(result);
        }

        #endregion


        #region Unit Tests for DeleteConfirmed

        [Fact]
        public async Task DeleteConfirmed_WithValidId_RedirectsToIndex()
        {
            _ = _employeeServiceMock.Setup(static service => service.RemoveAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            var result = await _controller.DeleteConfirmed(1);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_WithInvalidId_HandlesGracefully()
        {
            _ = _employeeServiceMock.Setup(service => service.RemoveAsync(It.IsAny<int>()))
                .Throws(new Exception("Not found"));

            var exception = await Record.ExceptionAsync(() => _controller.DeleteConfirmed(-1));

            Assert.NotNull(exception);
            _ = Assert.IsType<Exception>(exception);
        }

        [Fact]
        public async Task DeleteConfirmed_OnDatabaseFailure_HandlesException()
        {
            _ = _employeeServiceMock.Setup(service => service.RemoveAsync(It.IsAny<int>()))
                .Throws(new Exception("Database error"));

            var exception = await Record.ExceptionAsync(() => _controller.DeleteConfirmed(1));

            Assert.NotNull(exception);
            _ = Assert.IsType<Exception>(exception);
        }

        #endregion
    }
}