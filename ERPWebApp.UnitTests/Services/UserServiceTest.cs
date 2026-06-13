using ERPWebApp.Models.Company;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ERPWebApp.Models.Mappings;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
    private readonly Mock<ITriggerEmailAlertService> _mockTriggerEmailAlertService;
    private readonly UserService _userService;
    private readonly Mock<IDepartmentRoleMappingService> _mockRoleMappingService;
    private readonly Mock<ILogger<UserService>> _logger;

    public UserServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserManager = new Mock<UserManager<IdentityUser>>(
            new Mock<IUserStore<IdentityUser>>().Object,
            null, null, null, null, null, null, null, null);
        _mockTriggerEmailAlertService = new Mock<ITriggerEmailAlertService>();
        _mockRoleMappingService = new Mock<IDepartmentRoleMappingService>();
        _logger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_mockUnitOfWork.Object, _mockUserManager.Object, _mockTriggerEmailAlertService.Object,_mockRoleMappingService.Object,_logger.Object);
    }


    [Fact]
    public void GeneratePassword_ShouldReturnValidPassword()
    {
        // Act
        var password = _userService.GeneratePassword();

        // Assert
        Assert.Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()]).{10,}$", password);
    }

    [Fact]
    public async Task CreateUserForEmployee_ShouldMapRolesToUser()
    {
        // Arrange
        string companyEmail = "test@example.com";
        int departmentId = 1;

        var departmentRoles = new List<DepartmentRoleMapping>
        {
            new() { Role = new IdentityRole { Name = "Role1" } },
            new() { Role = new IdentityRole { Name = "Role2" } }
        };

        _ = _mockUserManager.Setup(static x => x.Users).Returns(new List<ApplicationUser>().AsQueryable());
        _ = _mockUserManager.Setup(static x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);
        _ = _mockUserManager.Setup(static x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);

        // Setup the GetListAsync method with all arguments explicitly, even the optional ones
        _ = _mockRoleMappingService.Setup(static x => x.GetListAsync(
            It.IsAny<Expression<Func<DepartmentRoleMapping, bool>>>(),
            It.IsAny<Expression<Func<DepartmentRoleMapping, string>>[]>(), // orderSelectors
            It.IsAny<Expression<Func<DepartmentRoleMapping, object>>[]>() // includes
        )).ReturnsAsync(departmentRoles);

        // Act
        var result = await _userService.CreateUserForEmployee(companyEmail, departmentId);

        // Assert
        Assert.True(result.Succeeded);
        _mockUserManager.Verify(static x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Role1"), Times.Once);
        _mockUserManager.Verify(static x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Role2"), Times.Once);
    }
}

