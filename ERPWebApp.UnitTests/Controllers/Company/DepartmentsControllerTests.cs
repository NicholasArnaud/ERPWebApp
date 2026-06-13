using ERPWebApp.Controllers.Company;
using ERPWebApp.Models.Company;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers.Company
{
    [Trait("Category", "execute")]
    public class DepartmentsControllerTests
    {
        private readonly Mock<IDepartmentService> _departmentServiceMock = new();
        private DepartmentsController _controller;
        public DepartmentsControllerTests()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.Administrator),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller = new DepartmentsController(
                      _departmentServiceMock.Object)
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


        [Fact]
        public async Task Index_ReturnsAViewResult_WithListOfDepartments()
        {
            // Arrange
            var departments = DepartmentsFixtures.GetTestDepartments();

            _ = _departmentServiceMock.Setup(static x => x.GetAllAsync(
               It.IsAny<Expression<Func<Department, string>>[]>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
            )).ReturnsAsync(departments);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Department>>(viewResult.Model);
            Assert.Equal(departments.Count, model.Count());
            
        }

        [Fact]
        public async Task Details_Returns_View_With_Valid_Id()
        {
            // Arrange
            int validId = 1;
            var expectedDepartment = new Department { DepartmentId = validId, };
            _ = _departmentServiceMock.Setup(static x => x.GetAsync(
                It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
            )).ReturnsAsync(expectedDepartment);

            // Act
            var result = await _controller.Details(validId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDepartment, result.Model);
        }

        [Fact]
        public async Task Details_Returns_NotFound_With_Null_Id()
        {
            // Arrange
            int? nullId = null;

            // Act
            var result = await _controller.Details(nullId) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Details_Returns_NotFound_With_Invalid_Id()
        {
            // Arrange
            int invalidId = 100;
            _ = _departmentServiceMock.Setup(static x => x.GetAsync(
                It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
            )).ReturnsAsync((Department?)null);

            // Act
            var result = await _controller.Details(invalidId) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var department = DepartmentsFixtures.GetTestDepartments().First();
            _ = _departmentServiceMock.Setup(static x => x.AddAsync(It.IsAny<Department>())).ReturnsAsync(department);

            // Act
            var result = await _controller.Create(department) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async Task Create_InvalidModel_ReturnsView()
        {
            // Arrange
            var department = DepartmentsFixtures.GetTestDepartments().First();
            _controller.ModelState.AddModelError("DepartmentName", "Department name is required");

            // Act
            var result = await _controller.Create(department) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(department, result.Model);
        }

        [Fact]
        public async Task Edit_Returns_ViewResult_With_Department()
        {
            // Arrange
            int departmentId = 1;
            var expectedDepartment = new Department { DepartmentId = departmentId, DepartmentName = "TestDepartment" };
            _ = _departmentServiceMock.Setup(static x => x.GetAsync(
                It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
            )).ReturnsAsync(expectedDepartment);

            // Act
            var result = await _controller.Edit(departmentId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Department>(viewResult.ViewData.Model);
            Assert.Equal(expectedDepartment.DepartmentId, model.DepartmentId);
            Assert.Equal(expectedDepartment.DepartmentName, model.DepartmentName);
        }

        [Fact]
        public async Task Edit_Returns_NotFoundResult_When_IdIsNull()
        {
            // Arrange
            int? departmentId = null;

            // Act
            var result = await _controller.Edit(departmentId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Returns_NotFoundResult_When_DepartmentNotFound()
        {
            // Arrange
            int departmentId = 999; // Assuming this ID does not exist
            Department? nullDepartment = null;
            _ = _departmentServiceMock.Setup(static x => x.GetAsync(
                It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
            )).ReturnsAsync(nullDepartment);


            // Act
            var result = await _controller.Edit(departmentId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            int? id = null;

            // Act
            var result = await _controller.Edit(id);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenDepartmentNotFound()
        {
            // Arrange
            _ = _departmentServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
            )).ReturnsAsync((Department?)null);

            int id = 1; 

            // Act
            var result = await _controller.Edit(id);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ReturnsView_WhenDepartmentExists()
        {
            // Arrange
            var department = DepartmentsFixtures.GetTestDepartments().First();
            _ = _departmentServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
            )).ReturnsAsync(department);

            int id = 1; 

            // Act
            var result = await _controller.Edit(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(department, viewResult.Model); 
        }
    }
}