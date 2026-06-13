using ERPWebApp.Controllers.Company;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Mappings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace ERPWebApp.UnitTests.Controllers.Company
{
    public class DepartmentRoleControllerTest
    {
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IDepartmentService> _departmentServiceMock;
        private readonly Mock<IDepartmentRoleMappingService> _departmentRoleMappingServiceMock;
        private readonly DepartmentRoleMappingController _roleMappingControllerMock;

        public DepartmentRoleControllerTest()
        {
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                new Mock<IRoleStore<IdentityRole>>().Object,
                new IRoleValidator<IdentityRole>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<ILogger<RoleManager<IdentityRole>>>().Object
            );
            _departmentRoleMappingServiceMock = new Mock<IDepartmentRoleMappingService>();
            _departmentServiceMock = new Mock<IDepartmentService>();

            _roleMappingControllerMock = new DepartmentRoleMappingController(
                _roleManagerMock.Object,
                _departmentServiceMock.Object,
                _departmentRoleMappingServiceMock.Object
            );
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithDepartmentRoleMappings()
        {
            // Arrange
            var mockMapping = new List<DepartmentRoleMapping>
    {
        new() {
            DepartmentRoleId = 1,
            DepartmentId = 1,
            UserRoleId = "Admin",
            Department = new Department { DepartmentId = 1, DepartmentName = "HR" },
            Role = new IdentityRole { Id = "Admin", Name = "Admin" }
        },
        new() {
            DepartmentRoleId = 2,
            DepartmentId = 1,
            UserRoleId = "Developer",
            Department = new Department { DepartmentId = 1, DepartmentName = "HR" },
            Role = new IdentityRole { Id = "Developer", Name = "Developer" }
        },
        new() {
            DepartmentRoleId = 3,
            DepartmentId = 2,
            UserRoleId = "Manager",
            Department = new Department { DepartmentId = 2, DepartmentName = "Engineering" },
            Role = new IdentityRole { Id = "Manager", Name = "Manager" }
        }
    };

            // Mocking the GetAllAsync method to return the mock data
            _ = _departmentRoleMappingServiceMock
                .Setup(static ser => ser.GetAllAsync(null, It.IsAny<Expression<Func<DepartmentRoleMapping, object>>[]>()))
                .ReturnsAsync(mockMapping);

            // Act
            var result = await _roleMappingControllerMock.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<DepartmentRoleMappingViewModel>>(viewResult.Model);
            Assert.Equal(2, model.Count); // Should return two departments: "HR" and "Engineering"
            Assert.Contains(model, static m => m.DepartmentId == 1 && m.UserRoleIds.Count == 2); // HR department with 2 roles
            Assert.Contains(model, static m => m.DepartmentId == 2 && m.UserRoleIds.Count == 1); // Engineering department with 1 role
        }


        [Fact]
        public async Task Create_Post_ReturnsRedirectToAction_WhenModelIsValid()
        {
            // Arrange
            var mockDepartmentRoleMappingViewModel = new DepartmentRoleMappingViewModel
            {
                DepartmentId = 1,
                UserRoleIds = ["Admin", "Developer"]
            };

            _ = _departmentRoleMappingServiceMock
                .Setup(static service => service.IsExistsAsync(It.IsAny<Expression<Func<DepartmentRoleMapping, bool>>>()))
                .ReturnsAsync(false);

            _ = _departmentRoleMappingServiceMock
                .Setup(static service => service.AddAsync(It.IsAny<DepartmentRoleMapping>()))
                .ReturnsAsync(new DepartmentRoleMapping());

            // Act
            var result = await _roleMappingControllerMock.Create(mockDepartmentRoleMappingViewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_DepartmentExists_ReturnsViewWithDuplicateError()
        {
        // Arrange
        var departments = new List<Department>
        {
            new() { DepartmentId = 1, DepartmentName = "HR" },
            new() { DepartmentId = 2, DepartmentName = "Engineering" }
        };

        var roles = new List<IdentityRole>
        {
            new() { Id = "1", Name = "Admin" },
            new() { Id = "2", Name = "Developer" }
        };

            _ = _departmentServiceMock
                .Setup(static service => service.GetAllAsync(null, It.IsAny<Expression<Func<Department, object>>[]>()))
                .ReturnsAsync(departments);

            _ = _roleManagerMock
                .Setup(static rm => rm.Roles)
                .Returns(roles.AsQueryable());

            var model = new DepartmentRoleMappingViewModel
            {
                DepartmentId = 1,
                UserRoleIds = ["1"]
            };

            _ = _departmentRoleMappingServiceMock
                .Setup(static service => service.IsExistsAsync(It.IsAny<Expression<Func<DepartmentRoleMapping, bool>>>()))
                .ReturnsAsync(true); // Simulate that the department already has the default role

            // Act
            var result = await _roleMappingControllerMock.Create(model) as ViewResult;

            // Assert
            Assert.NotNull(result); // Ensure the result is not null

            // Ensure that ModelState is invalid as expected
            Assert.False(_roleMappingControllerMock.ModelState.IsValid);

            // Retrieve the ModelState entry for the key string.Empty
            var modelStateEntry = _roleMappingControllerMock.ModelState[string.Empty];

            // Assert that the ModelState entry is not null
            Assert.NotNull(modelStateEntry);


            Assert.NotNull(modelStateEntry);

            // Ensure that the Errors collection is not null and not empty
            Assert.NotNull(modelStateEntry.Errors);  // Check that Errors is not null
            Assert.NotEmpty(modelStateEntry.Errors);


            // Assert that the specific error message is in the first error entry
            Assert.Contains("Unable to create mapping. Department already have default user role.",
                modelStateEntry.Errors[0].ErrorMessage);
        }


        [Fact]
        public async Task Edit_ReturnsNotFound_WhenDepartmentRoleMappingDoesNotExist()
        {
            // Arrange
            _ = _departmentRoleMappingServiceMock
            .Setup(static service => service.GetListAsync(
                It.IsAny<Expression<Func<DepartmentRoleMapping, bool>>>(),
                It.IsAny<Expression<Func<DepartmentRoleMapping, string>>[]>(),
                It.IsAny<Expression<Func<DepartmentRoleMapping, object>>[]>()
            ))
            .ReturnsAsync((List<DepartmentRoleMapping>?)null);


            // Act
            var result = await _roleMappingControllerMock.Edit(1);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ReturnsRedirectToAction_WhenModelIsValid()
        {
            // Arrange
            var model = new DepartmentRoleMappingViewModel
            {
                DepartmentId = 1,
                UserRoleIds = ["Admin", "Manager"]
            };

            // Mocking services to reflect successful operations
            _ = _departmentServiceMock
                .Setup(static service => service.IsExistsAsync(It.IsAny<Expression<Func<Department, bool>>>()))
                .ReturnsAsync(true);

            _ = _departmentRoleMappingServiceMock
                .Setup(static service => service.GetListAsync(
                    It.IsAny<Expression<Func<DepartmentRoleMapping, bool>>>(),
                    It.IsAny<Expression<Func<DepartmentRoleMapping, string>>[]>(),
                    It.IsAny<Expression<Func<DepartmentRoleMapping, object>>[]>()
                ))
                .ReturnsAsync([]); // Return an empty list instead of null

            _ = _departmentRoleMappingServiceMock
                .Setup(static service => service.RemoveAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(1)); // Return a Task<int> to indicate success

            _ = _departmentRoleMappingServiceMock
                .Setup(static service => service.AddAsync(It.IsAny<DepartmentRoleMapping>()))
                .ReturnsAsync(new DepartmentRoleMapping());

            // To simulate that the model is valid, ensure ModelState is valid
            _roleMappingControllerMock.ModelState.Clear();

            // Act
            var result = await _roleMappingControllerMock.Edit(1, model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

    }
}
