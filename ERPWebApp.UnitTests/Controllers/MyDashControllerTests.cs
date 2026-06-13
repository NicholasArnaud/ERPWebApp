using ERPWebApp.Controllers;
using ERPWebApp.Data.DTOModels;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers
{
    public class MyDashControllerTests
    {
        private readonly Mock<IMyDashService> _myDashServiceMock;
        private readonly Mock<IInventoryService> _inventoryServiceMock;
        private readonly Mock<IStocksService> _stocksServiceMock;
        private readonly Mock<IUserPreferencesService> _userPreferencesServiceMock;
        private readonly Mock<IFinancialsService> _financialsServiceMock;
        private readonly Mock<IDepartmentService> _departmentServiceMock;
        private readonly Mock<ISpeedOMeterGoalService> _speedOMeterGoalServiceMock;
        private readonly Mock<IEmployeeService> _employeeServiceMock;
        private readonly MyDashController _controller;

        public MyDashControllerTests()
        {
            _myDashServiceMock = new Mock<IMyDashService>();
            _inventoryServiceMock = new Mock<IInventoryService>();
            _stocksServiceMock = new Mock<IStocksService>();
            _userPreferencesServiceMock = new Mock<IUserPreferencesService>();
            _financialsServiceMock = new Mock<IFinancialsService>();
            _departmentServiceMock = new Mock<IDepartmentService>();
            _speedOMeterGoalServiceMock = new Mock<ISpeedOMeterGoalService>();
            _employeeServiceMock = new Mock<IEmployeeService>();

            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.Role, RoleList.BasicUser),
        };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller = new MyDashController(
                _myDashServiceMock.Object,
                _inventoryServiceMock.Object,
                _stocksServiceMock.Object,
                _userPreferencesServiceMock.Object,
                _financialsServiceMock.Object,
                _departmentServiceMock.Object,
                _speedOMeterGoalServiceMock.Object,
                _employeeServiceMock.Object
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
        [Fact]
        public async Task ProductSales_WhenSucceed_ReturnsProductsList()
        {
            // Arrange
            var days = 30;
            var expectedData = new List<ProductSalesInfoDto>();
            _ = _financialsServiceMock.Setup(s => s.ProductSalesTable(days)).ReturnsAsync(expectedData);

            // Act
            var result = await _controller.ProductSales(days);

            // Assert
            Assert.Equal(expectedData, result);
        }
        [Fact]
        public async Task TotalFulfillmentSales_WhenSucceed_ReturnsFulfillmentInfoList()
        {
            // Arrange
            var expectedData = new List<FulfillmentInfoDto>();
            _ = _financialsServiceMock.Setup(static s => s.FulfillmentTable()).ReturnsAsync(expectedData);

            // Act
            var result = await _controller.TotalFulfillmentSales();

            // Assert
            Assert.Equal(expectedData, result);
        }    

    }
}
