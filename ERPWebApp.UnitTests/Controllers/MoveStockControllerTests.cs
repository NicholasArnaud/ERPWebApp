using ERPWebApp.Controllers;
using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using System.Linq.Expressions;
using System.Security.Claims;
using ERPWebApp.Data.DTOModels.StockDto;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Mappings;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERPWebApp.UnitTests.Controllers;

[Trait("Category", "execute")]
public class MoveStockControllerTests
{
    private readonly Mock<IMoveStockHistoryService> _moveStockHistoryServiceMock = new();
    private readonly Mock<ILocationService> _locationServiceMock = new();
    private readonly Mock<IStocksService> _stocksServiceMock = new();
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly Mock<ISiteService> _siteServiceMock = new();
    private readonly Mock<IUserSiteMappingService> _userSiteMappingServiceMock = new();
    private readonly MoveStockController _controller;

    public MoveStockControllerTests()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.Role, RoleList.Administrator),
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var mockHttpContext = new Mock<HttpContext>();
        var mockTempDataProvider = new Mock<ITempDataProvider>();
        _controller = new MoveStockController(
            _moveStockHistoryServiceMock.Object,
            _locationServiceMock.Object,
            _stocksServiceMock.Object,
            _productServiceMock.Object,
            _siteServiceMock.Object,
            _userSiteMappingServiceMock.Object
        )
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            },
            TempData = new TempDataDictionary(mockHttpContext.Object, mockTempDataProvider.Object)
        };
    }

    [Fact]
    public void Index_SuccessfulScenario_AdminRole()
    {
        ResetViewBags();
        
        var result = _controller.Index() as ViewResult;

        Assert.NotNull(result);
        _ = Assert.IsType<MoveStock>(result.Model);
    }

    [Theory]
    [InlineData(RoleList.ExternalViewer)]
    [InlineData(RoleList.Administrator)]
    [InlineData(RoleList.ExternalUser)]
    public void Index_FailedScenario_UnauthorizedRole(string Role)
    {
        MoveStockController controller = _controller;
        if (Role != RoleList.Administrator)
        {
            controller = new MoveStockController(
                _moveStockHistoryServiceMock.Object,
                _locationServiceMock.Object,
                _stocksServiceMock.Object,
                _productServiceMock.Object,
                _siteServiceMock.Object,
                _userSiteMappingServiceMock.Object
            );

            var userMock = new ClaimsPrincipal();
            userMock.AddIdentity(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "")
            ], "mock"));
            Thread.CurrentPrincipal = userMock;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock }
            };
        }

        ResetViewBags();

        // Act - invoke the Index method
        var result = controller.Index() as ViewResult;

        // Assert - check if the result is a failure or redirected to an error page
        Assert.NotNull(result);
        _ = Assert.IsType<MoveStock>(result.Model);
    }

    [Fact]
    public async Task GetTableData_ReturnsExpectedJsonResult()
    {
        // Arrange
        var productSku = "SKU123";
        List<StockMovementHistory> mockHistory = MoveStockHistoryFixtures.GetStockMovementHistory();
        var mockCount = mockHistory.Count;

        _ = _moveStockHistoryServiceMock
            .Setup(s => s.GetStockMovementHistoryAsync(It.IsAny<SearchParameters>(), It.IsAny<bool?>(), productSku))
            .ReturnsAsync((mockHistory, mockCount));
            
        _controller.HttpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>
        {
            { "draw", "1" },
            { "start", "0" },
            { "length", "10" },
            { "order[0][column]", "0" },
            { "columns[0][name]", "Date" },
            { "order[0][dir]", "asc" },
            { "search[value]", "test" }
        });

        // Act
        var result = await _controller.GetTableData(productSku) as OkObjectResult;
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }

    [Theory]
    [InlineData(RoleList.ExternalViewer)]
    [InlineData(RoleList.Administrator)]
    [InlineData(RoleList.ExternalUser)]
    public async Task LocationsBySiteId_Returns_AdministratorLocations(string Role)
    {
        // Arrange - Mock HttpContext, set user as Administrator
        MoveStockController controller = _controller;
        if (Role != RoleList.Administrator)
        {
            controller = new MoveStockController(
                _moveStockHistoryServiceMock.Object,
                _locationServiceMock.Object,
                _stocksServiceMock.Object,
                _productServiceMock.Object,
                _siteServiceMock.Object,
                _userSiteMappingServiceMock.Object
            );
            var userMock = new Mock<ClaimsPrincipal>();
            _ = userMock.Setup(static u => u.IsInRole(RoleList.Administrator)).Returns(false);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };
        }

        _ = _locationServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Expression<Func<Location, string>>[]>(),
                It.IsAny<Expression<Func<Location, object>>[]>()))
            .ReturnsAsync(LocationFixtures.GetTestLocations());

        // Act
        var result = await controller.LocationsBySiteId(123) as JsonResult;

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(RoleList.ExternalViewer)]
    [InlineData(RoleList.Administrator)]
    [InlineData(RoleList.ExternalUser)]
    public async Task LocationsBySiteId_Returns_ExternalUserLocations(string Role)
    {
        // Arrange - Mock HttpContext, set user as Administrator
        MoveStockController controller = _controller;
        if (Role != RoleList.ExternalUser)
        {
            controller = new MoveStockController(
                _moveStockHistoryServiceMock.Object,
                _locationServiceMock.Object,
                _stocksServiceMock.Object,
                _productServiceMock.Object,
                _siteServiceMock.Object,
                _userSiteMappingServiceMock.Object
            );
            var userMock = new Mock<ClaimsPrincipal>();
            _ = userMock.Setup(static u => u.IsInRole(RoleList.ExternalUser)).Returns(false);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };
        }

        _ = _locationServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Expression<Func<Location, string>>[]>(),
                It.IsAny<Expression<Func<Location, object>>[]>()))
            .ReturnsAsync(LocationFixtures.GetTestLocations());

        // Act
        var result = await controller.LocationsBySiteId(123) as JsonResult;

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(RoleList.ExternalViewer)]
    [InlineData(RoleList.Administrator)]
    [InlineData(RoleList.ExternalUser)]
    public async Task LocationsBySiteId_Returns_InternalUserLocations(string Role)
    {
        MoveStockController controller = _controller;
        if (Role != RoleList.BasicUser)
        {
            controller = new MoveStockController(
                _moveStockHistoryServiceMock.Object,
                _locationServiceMock.Object,
                _stocksServiceMock.Object,
                _productServiceMock.Object,
                _siteServiceMock.Object,
                _userSiteMappingServiceMock.Object
            );
            var userMock = new Mock<ClaimsPrincipal>();
            _ = userMock.Setup(static u => u.IsInRole(RoleList.BasicUser)).Returns(false);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };
        }

        _ = _locationServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Expression<Func<Location, string>>[]>(),
                It.IsAny<Expression<Func<Location, object>>[]>()))
            .ReturnsAsync(LocationFixtures.GetTestLocations());

        // Act
        var result = await controller.LocationsBySiteId(123) as JsonResult;

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(RoleList.ExternalViewer)]
    [InlineData(RoleList.Administrator)]
    [InlineData(RoleList.ExternalUser)]
    public void SitesByProduct_Returns_For_ExternalUser(string Role)
    {
        // Arrange - Set up a user with ExternalUser role
        MoveStockController controller = _controller;
        if (Role != RoleList.ExternalUser)
        {
            controller = new MoveStockController(
                _moveStockHistoryServiceMock.Object,
                _locationServiceMock.Object,
                _stocksServiceMock.Object,
                _productServiceMock.Object,
                _siteServiceMock.Object,
                _userSiteMappingServiceMock.Object
            );
            var userMock = new Mock<ClaimsPrincipal>();
            _ = userMock.Setup(static u => u.IsInRole(RoleList.ExternalUser)).Returns(false);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };
        }

        // Act
        var result = controller.SitesByProduct(1) as JsonResult;

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(RoleList.ExternalViewer)]
    [InlineData(RoleList.Administrator)]
    [InlineData(RoleList.ExternalUser)]
    public void SitesByProduct_Returns_For_Administrator_Or_ExternalViewer(string Role)
    {
        // Arrange - Set up a user with Administrator or ExternalViewer role
        MoveStockController controller = _controller;
        if (Role != RoleList.Administrator)
        {
            controller = new MoveStockController(
                _moveStockHistoryServiceMock.Object,
                _locationServiceMock.Object,
                _stocksServiceMock.Object,
                _productServiceMock.Object,
                _siteServiceMock.Object,
                _userSiteMappingServiceMock.Object
            );
            var userMock = new Mock<ClaimsPrincipal>();
            _ = userMock.Setup(static u => u.IsInRole(RoleList.Administrator)).Returns(false);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };
        }

        // Act
        var result = controller.SitesByProduct(1) as JsonResult;

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(RoleList.ExternalViewer)]
    [InlineData(RoleList.Administrator)]
    [InlineData(RoleList.ExternalUser)]
    public void SitesByProduct_Returns_For_Other_Roles(string Role)
    {
        // Arrange - Set up a user without Administrator, ExternalViewer, or ExternalUser role (Implied InternalUser)
        MoveStockController controller = _controller;
        if (Role != RoleList.BasicUser)
        {
            controller = new MoveStockController(
                _moveStockHistoryServiceMock.Object,
                _locationServiceMock.Object,
                _stocksServiceMock.Object,
                _productServiceMock.Object,
                _siteServiceMock.Object,
                _userSiteMappingServiceMock.Object
            );
            var userMock = new Mock<ClaimsPrincipal>();
            _ = userMock.Setup(static u => u.IsInRole(RoleList.BasicUser)).Returns(false);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userMock.Object }
            };
        }

        // Act
        var result = controller.SitesByProduct(1) as JsonResult;

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task MoveStockForm_Valid_ModelState_Successful_Move()
    {
        // Arrange
        var moveStock = new MoveStock
        {
            FromStock = StockFixtures.GetTestStocks().FirstOrDefault(),
            ToStock = StockFixtures.GetTestStocks().FirstOrDefault(),
        };
        ResetViewBags();
        _controller.ModelState.Clear();
        _ = _moveStockHistoryServiceMock.Setup(x => x.MoveStock(moveStock)).ReturnsAsync(moveStock);

        // Act
        var result = await _controller.MoveStockForm(moveStock) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("Move Successful.", _controller.TempData["successMessage"]);
    }

    [Fact]
    public async Task MoveStockForm_Invalid_ModelState_Returns_View()
    {
        // Arrange
        var moveStock = new MoveStock
        {
            FromStock = StockFixtures.GetTestStocks().FirstOrDefault(),
            ToStock = StockFixtures.GetTestStocks().FirstOrDefault(),
        };
        ResetViewBags();
        _controller.ModelState.Clear();
        _ = _moveStockHistoryServiceMock.Setup(x => x.MoveStock(moveStock)).ReturnsAsync(moveStock);
        _controller.ModelState.AddModelError("quantity", "Invalid quantity");

        // Act
        var result = await _controller.MoveStockForm(moveStock) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ViewName);
    }
        
    [Fact]
    public async Task MoveStockForm_Exception_Returns_Error()
    {
        // Arrange
        var moveStock = new MoveStock
        {
            FromStock = StockFixtures.GetTestStocks().FirstOrDefault(),
            ToStock = StockFixtures.GetTestStocks().FirstOrDefault(),
        };
        ResetViewBags();
        _controller.ModelState.Clear();
        _ = _moveStockHistoryServiceMock.Setup(x => x.MoveStock(moveStock)).Throws<Exception>();

        // Act
        var result = await _controller.MoveStockForm(moveStock) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);

        // Assert

    }

    [Fact]
    public async Task AddStockForm_Valid_ModelState_Successful_Add()
    {
        // Arrange
        var moveStock = new MoveStock
        {
            FromStock = StockFixtures.GetTestStocks().FirstOrDefault(),
            ToStock = StockFixtures.GetTestStocks().FirstOrDefault(),
        };
        ResetViewBags();
        _controller.ModelState.Clear();
        _ = _moveStockHistoryServiceMock.Setup(x => x.AddStock(moveStock)).ReturnsAsync(moveStock);

        // Act
        var result = await _controller.AddStockForm(moveStock) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("Add Successful.", _controller.TempData["successMessage"]);
    }

    [Fact]
    public async Task AddStockForm_Exception_Returns_Error()
    {
        // Arrange
        var moveStock = new MoveStock
        {
            FromStock = StockFixtures.GetTestStocks().FirstOrDefault(),
            ToStock = StockFixtures.GetTestStocks().FirstOrDefault(),
        };
        ResetViewBags();
        _controller.ModelState.Clear();
        _ = _moveStockHistoryServiceMock.Setup(x => x.AddStock(moveStock)).Throws<Exception>();

        // Act
        var result = await _controller.AddStockForm(moveStock) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);

        // Assert

    }

    [Fact]
    public async Task RemoveStockForm_Valid_ModelState_Successful_Remove()
    {
        // Arrange
        var moveStock = new MoveStock
        {
            FromStock = StockFixtures.GetTestStocks().FirstOrDefault(),
            ToStock = StockFixtures.GetTestStocks().FirstOrDefault(),
        };
        ResetViewBags();
        _controller.ModelState.Clear();
        _ = _moveStockHistoryServiceMock.Setup(x => x.RemoveStock(moveStock)).ReturnsAsync(moveStock);

        // Act
        var result = await _controller.RemoveStockForm(moveStock) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("Remove Successful.", _controller.TempData["successMessage"]);
    }

    [Fact]
    public async Task RemoveStockForm_Exception_Returns_Error()
    {
        // Arrange
        var moveStock = new MoveStock
        {
            FromStock = StockFixtures.GetTestStocks().FirstOrDefault(),
            ToStock = StockFixtures.GetTestStocks().FirstOrDefault(),
        };
        ResetViewBags();
        _controller.ModelState.Clear();
        _ = _moveStockHistoryServiceMock.Setup(x => x.RemoveStock(moveStock)).Throws<Exception>();

        // Act
        var result = await _controller.RemoveStockForm(moveStock) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);

        // Assert
    }
        
    private void ResetViewBags()
    {
        var product = ProductFixtures.GetTestProducts()
            .Where(static x => x.IsActive)
            .Select(static x => new SelectListItem{ Value = x.ProductId.ToString(), Text = x.Sku + " : " + x.Description })
            .ToList();

        var location = LocationFixtures.GetTestLocations()
            .Where(static x => x.IsActive)
            .Select(static x => new SelectListItem{ Value = x.LocationId.ToString(), Text = x.LocationName })
            .ToList();

        _ = _userSiteMappingServiceMock.Setup(static x => x.GetList(
            It.IsAny<Expression<Func<UserSiteMapping, bool>>>(),
            It.IsAny<Expression<Func<UserSiteMapping, string>>[]>(),
            It.IsAny<Expression<Func<UserSiteMapping, object>>[]>())
        ).Returns([]);

        _ = _productServiceMock.Setup(static x => x.GetList(
                It.IsAny<Func<IQueryable<Product>, IQueryable<SelectListItem>>>()))
            .Returns(product);

        _ = _locationServiceMock.Setup(static x => x.GetList(
                It.IsAny<Func<IQueryable<Location>, IQueryable<SelectListItem>>>()))
            .Returns(location);

        _ = _siteServiceMock.Setup(static x => x.GetList(It.IsAny<Func<IQueryable<Site>, IQueryable<SelectListItem>>>())).
            Returns([]);
    }
}