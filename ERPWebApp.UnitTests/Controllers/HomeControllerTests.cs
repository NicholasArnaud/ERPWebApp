using ERPWebApp.Controllers;
using ERPWebApp.Models.Company;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers;

[Trait("Category", "execute")]
public class HomeControllerTests
{
    private readonly Mock<IOrderShippingService> _orderShippingServiceMock = new();
    private readonly Mock<IDepartmentService> _departmentServiceMock = new();
    private readonly Mock<ISpeedOMeterGoalService> _speedOMeterGoalServiceMock = new();
    private readonly Mock<IEmployeeService> _employeeServiceMock = new();
    private readonly Mock<IStocksService> _stocksServiceMock = new();
    private readonly Mock<IShipStationStoreService> _shipStationStoreServiceMock = new();
    private readonly Mock<ISiteService> _siteServiceMock = new();
    private readonly Mock<IProductionVsLaborCostPriceService> _productionVsLaborCostPriceServiceMock = new();
    private readonly Mock<IHomeService> _homeServiceMock = new();
    private readonly Mock<IUserPreferencesService> _userPreferencesServiceMock = new();
    private readonly Mock<IMyDashService> _myDashServiceMock = new();
    private readonly HomeController _controller;
    public HomeControllerTests()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.Role, RoleList.BasicUser),
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

         _controller = new HomeController(
                   _orderShippingServiceMock.Object,
                   _departmentServiceMock.Object,
                   _speedOMeterGoalServiceMock.Object,
                   _employeeServiceMock.Object,
                   _stocksServiceMock.Object,
                   _shipStationStoreServiceMock.Object,
                   _siteServiceMock.Object,
                   _productionVsLaborCostPriceServiceMock.Object,
                   _homeServiceMock.Object,
                   _userPreferencesServiceMock.Object,
                   _myDashServiceMock.Object
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

    [Theory]
    [InlineData(RoleList.SellerBasic)]
    [InlineData(RoleList.RestrictedUser)]
    public async Task IndexAsync_WhenUserRoleInvalid_ShoudReturnActionResult(string userRole)
    {
        //Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, userRole),
        }));
        var httpContext = new DefaultHttpContext()
        {
            User = user,
        };

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        _ = _userPreferencesServiceMock.Setup(static s => s.GetDashboardLayoutByDashboardAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync([]);

        // Act
        var result = await _controller.IndexAsync() as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("WelcomeSeller", result.ControllerName);
    }

    [Fact]
    public async Task IndexAsync_WhenSucceed_ShoudReturnActionResult()
    {
        // Arrange
        var departments = new List<Department> { new() { DepartmentId = 1, DepartmentName = "Department 1" } };
        var speedOMeterGoal = new SpeedOMeterGoal();
        var productionVsLaborCostPrice = new ProductionVsLaborCostPrice();

        _ = _departmentServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Expression<Func<Department, bool>>>(),
            It.IsAny<Expression<Func<Department, string>>[]>(),
             It.IsAny<Expression<Func<Department, object>>[]>()
        )).ReturnsAsync(departments);

        _ = _speedOMeterGoalServiceMock.Setup(static x => x.GetLastSpeedOMeterGoalAsync())
            .ReturnsAsync(speedOMeterGoal);

        _ = _employeeServiceMock.Setup(static x => x.GetLastProductionVsLaborCostPrice())
            .ReturnsAsync(productionVsLaborCostPrice);

        _ = _userPreferencesServiceMock.Setup(static s => s.GetDashboardLayoutByDashboardAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync([]);

        // Act
        var result = await _controller.IndexAsync();

        // Assert
        _ = Assert.IsType<ViewResult>(result);

        var viewResult = result as ViewResult;
        Assert.NotNull(viewResult);
        Assert.NotNull(viewResult.ViewData["Department"]);

        var departmentList = viewResult.ViewData["Department"] as SelectList;
        Assert.NotNull(departmentList);
        Assert.Equal(departments.Count, departmentList.Count());

        Assert.NotNull(viewResult.Model);

        var model = viewResult.Model as Home;
        Assert.NotNull(model);
        Assert.Equal(speedOMeterGoal, model.SpeedOMeterGoal);
        Assert.Equal(productionVsLaborCostPrice, model.ProductionVsLaborCostPrice);
    }
    /*
    [Fact]
    public async void SetupItemTally_WhenSucceed_ShoudReturnJsonResult()
    {
        //Arrange
        var data = new Dictionary<string, int>();
        data.Add("ElectroplatingTally", 1);
        data.Add("EmbroideryTally", 2);
        data.Add("EngravingTally", 3);
        data.Add("MetalTally", 4);
        data.Add("UnknownTally", 5);
        data.Add("UvpTally", 6);
        data.Add("PlankTally", 7);

        _speedOMeterGoalServiceMock.Setup(x => x.GetLastSpeedOMeterGoalAsync()).ReturnsAsync(new SpeedOMeterGoal());
        _orderShippingServiceMock.Setup(x => x.SetupItemTally()).ReturnsAsync(data);

        //Act
        var result = await _controller.SetupItemTally() as JsonResult;

        //Assert
        Assert.NotNull(result);
        Assert.IsType<JsonResult>(result);
        dynamic json = JObject.Parse(JsonConvert.SerializeObject(result.Value));
        Assert.Equal(data["ElectroplatingTally"], json.ElectroplatingTally.Value);
    }
    */

    [Fact]
    public void SetupPieCharts_WhenSucceed_ShoudReturnJsonResult()
    {
        //Arrange
        List<int> electroplatingPlot = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        List<String> electroplatingSkus = ["A", "B", "C", "D", "F", "G", "H", "I", "J", "K", "L", "M"];
        List<int> embroideryPlot = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        List<String> embroiderySkus = ["A", "B", "C", "D", "F", "G", "H", "I", "J", "K", "L", "M"];
        List<int> engravingPlot = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        List<String> engravingSkus = ["A", "B", "C", "D", "F", "G", "H", "I", "J", "K", "L", "M"];
        List<int> metalPlot = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        List<String> metalSkus = ["A", "B", "C", "D", "F", "G", "H", "I", "J", "K", "L", "M"];
        List<int> uvpPlot = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        List<String> uvpSkus = ["A", "B", "C", "D", "F", "G", "H", "I", "J", "K", "L", "M"];
        List<int> unknownPlot = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        List<String> unknownSkus = ["A", "B", "C", "D", "F", "G", "H", "I", "J", "K", "L", "M"];

        var data = JObject.FromObject(
            new
            {
                electroplatingPlot,
                electroplatingSkus,
                embroideryPlot,
                embroiderySkus,
                engravingPlot,
                engravingSkus,
                metalPlot,
                metalSkus,
                uvpPlot,
                uvpSkus,
                unknownPlot,
                unknownSkus
            }
        );

        var expexted = new
        {
            electroplatingCountArray = electroplatingPlot.Take(10),
            electroplatingSKUArray = electroplatingSkus.Take(10),

            embroideryCountArray = embroideryPlot.Take(10),
            embroiderySKUArray = embroiderySkus.Take(10),

            engravingCountArray = engravingPlot.Take(10),
            engravingSKUarray = engravingSkus.Take(10),

            metalCountArray = metalPlot.Take(10),
            metalSKUArray = metalSkus.Take(10),

            uvpCountArray = uvpPlot.Take(10),
            uvpSKUArray = uvpSkus.Take(10),

            unknownCountArray = unknownPlot.Take(10),
            unknownSKUArray = unknownSkus.Take(10)
        };

        _ = _shipStationStoreServiceMock.Setup(static x => x.GetShipStationStorePieChartsData())
                .Returns(JObject.FromObject(data));

        //Act
        var result = _controller.SetupPieCharts();

        //Assert
        Assert.NotNull(result);
        _ = Assert.IsType<JsonResult>(result);

        var expectedJson = JsonConvert.SerializeObject(expexted);
        var actualJson = JsonConvert.SerializeObject(result.Value);
        Assert.Equal(expectedJson, actualJson);
    }

}