using ERPWebApp.Controllers;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers
{
    [Trait("Category", "execute")]
    public class CycleCountControllerTests
    {
        private readonly Mock<ICycleCountFrequencyService> _cycleCoutFrequencyServiceMock = new();
        private readonly Mock<ISiteService> _siteServiceMock = new();
        private readonly Mock<IStocksService> _stockServiceMock = new();
        private readonly Mock<IProductService> _productServiceMock = new();
        private readonly Mock<ICycleCountService> _cycleCountServiceMock = new();
        private readonly Mock<IEmployeeService> _employeeServiceMock = new();
        private readonly CycleCountController _controller;
        private readonly ITempDataDictionary tempData;
        private readonly ClaimsPrincipal claimsPrincipal;
        private readonly Mock<ITriggerEmailAlertService > _triggerEmailAlertServiceMock = new();

        public CycleCountControllerTests()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.BasicUser),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            claimsPrincipal = new ClaimsPrincipal(identity);
            ITempDataProvider tempDataProvider = Mock.Of<ITempDataProvider>();
            TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
            tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());
            _controller = new CycleCountController(
                _cycleCoutFrequencyServiceMock.Object,
                _siteServiceMock.Object,
                _stockServiceMock.Object,
                _productServiceMock.Object,
                _cycleCountServiceMock.Object,
                _employeeServiceMock.Object,
                _triggerEmailAlertServiceMock.Object

            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
                },
                TempData = tempData
            };
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithSiteFilter()
        {
            // Arrange
            _ = _cycleCoutFrequencyServiceMock.Setup(static s => s.GenerateFrequenciesAsync()).Returns(Task.CompletedTask);
            _ = _siteServiceMock.Setup(static s => s.GetListAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, string>>[]>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            ))
            .ReturnsAsync([new() { SiteId = 1, SiteName = "Test Site", IsActive = true }]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewData = viewResult.ViewData["SiteFilter"];
            Assert.NotNull(viewData);
        }

        [Fact]
        public async Task GetCycleCountList_ReturnsOkResult_WithCycleCountData()
        {
            // Arrange
            _controller.HttpContext.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "draw", "1" },
                { "order[0][column]", "0" },
                { "columns[0][name]", "SiteId" },
                { "order[0][dir]", "asc" },
                { "start", "0" },
                { "length", "10" },
                { "search[value]", "" }
            });

            var stocks = StockFixtures.GetTestStocks();

            _ = _cycleCountServiceMock.Setup(static s => s.GetStockToCountAsync(It.IsAny<int>(), It.IsAny<SearchParameters>(), It.IsAny<bool>()))
                .ReturnsAsync((stocks, stocks.Count));

            // Act
            var result = await _controller.GetCycleCountList(1, false);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task PopulateFrequency_ReturnsJsonResult_WithFrequencyData()
        {
            // Arrange
            var mockFrequency = new CycleCountFrequency
            {
                SiteId = 1,
                BaseDays = 5,
                Over1000 = 10,
                Cost10 = 15,
                ModifyDate = DateTime.Now
            };
            _ = _cycleCoutFrequencyServiceMock.Setup(static s => s.GetLatestFrequencyAsync(It.IsAny<int>()))
                .ReturnsAsync(mockFrequency);

            // Act
            var result = await _controller.PopulateFrequency(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var data = jsonResult.Value;
            _ = data.Should().NotBeNull();
        }

        [Fact]
        public async Task StartCount_ReturnsOkResult()
        {
            // Arrange
            _ = _cycleCountServiceMock.Setup(static s => s.StartCycleCountAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.StartCount(1);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task StartBulk_ReturnsOkResult()
        {
            // Arrange
            _ = _cycleCountServiceMock.Setup(static s => s.StartCycleCountAsync(It.IsAny<List<int>>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.StartBulk([1, 2, 3]);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task StartCountBySite_ReturnsOkResult()
        {
            // Arrange
            _ = _cycleCountServiceMock.Setup(static s => s.StartCycleCountForSiteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.StartCountBySite(1);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void Edit_WithNullId_ReturnsNotFoundResult()
        {
            // Arrange
            int? id = null;

            // Act
            var result = _controller.Edit(id);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_WithInvalidCycleCount_ReturnsNotFoundResult()
        {
            // Arrange
            int stockId = 1;

            var cycleCountServiceMock = new Mock<ICycleCountService>();
            _ = cycleCountServiceMock
                .Setup(static x => x.Get(It.IsAny<Expression<Func<CycleCount, bool>>>(), It.IsAny<Expression<Func<CycleCount, object>>[]>()))
                .Returns((CycleCount?)null!);

            // Act
            var result = _controller.Edit(stockId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_UpdatesCycleCountAndStock_ReturnsViewResultWithIndexView()
        {
            // Arrange
            int cycleCountId = 1;
            int stockId = 2;
            int siteId = 4;
            int expectedTotalAvailable = 20;

            var cycleCount = new CycleCount
            {
                CycleCountId = cycleCountId,
                StockId = stockId,
                EnteredQuantity = expectedTotalAvailable
            };

            var stock = new Stock
            {
                StockId = stockId,
                BeingCounted = true,
                LastCounted = new DateTime(2023, 5, 6),
                TotalAvailable = expectedTotalAvailable,
                Products = new Product { /* Initialize product properties */ },
                Location = new Location { SiteId = siteId, Sites = new Site { /* Initialize site properties */ } }
            };

            var frequency = new CycleCountFrequency
            {
                SiteId = siteId,
                BaseDays = 5,
                Over1000 = 10,
                Cost10 = 2,
                ModifyDate = DateTime.Now
            };

            _ = _stockServiceMock
                .Setup(x => x.GetAsync(It.IsAny<Expression<Func<Stock, bool>>>(), It.IsAny<Expression<Func<Stock, object>>[]>()))
                .ReturnsAsync(stock);

            _ = _siteServiceMock
                .Setup(x => x.GetListAsync(
                    It.IsAny<Expression<Func<Site, bool>>>(),
                    It.IsAny<Expression<Func<Site, string>>[]>(),
                    It.IsAny<Expression<Func<Site, object>>[]>()
                    ))
                .ReturnsAsync(SiteFixtures.GetTestSites());


            _ = _cycleCountServiceMock
                .Setup(x => x.EditCycleCount(cycleCount, _controller.User.Identity != null ? _controller.User.Identity.Name : string.Empty))
                .ReturnsAsync(stock);

            // Act
            var result = await _controller.Edit(cycleCount);

            // Assert
            _ = Assert.IsType<OkResult>(result);


            var userName = _controller.User.Identity != null ? _controller.User.Identity.Name : string.Empty;
            _cycleCountServiceMock.Verify(x => x.EditCycleCount(cycleCount, userName), Times.Once);
        }

        [Fact]
        public async Task ModifyCycle_ValidInput_ReturnsOkResult()
        {
            // Arrange
            int baseDays = 1;
            int over100 = 2;
            int cost10 = 3;
            int siteId = 4;
            DateTime expectedDateTime = new(2023, 5, 6);

            _ = _cycleCoutFrequencyServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<CycleCountFrequency>()))
                .ReturnsAsync(new CycleCountFrequency());

            _ = _siteServiceMock.Setup(static x => x.IsExists(It.IsAny<Expression<Func<Site, bool>>>())).Returns(true);

            // Act
            var result = await _controller.ModifyCycle(baseDays, over100, cost10, siteId);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            _cycleCoutFrequencyServiceMock.Verify(static x => x.AddAsync(It.IsAny<CycleCountFrequency>()), Times.Once);
        }

        [Fact]
        public async Task ModifyCycle_ReturnsBadRequest_ForInvalidSite()
        {
            // Arrange
            _ = _siteServiceMock.Setup(static s => s.IsExists(It.IsAny<Expression<Func<Site, bool>>>())).Returns(false);

            // Act
            var result = await _controller.ModifyCycle(5, 10, 15, 1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestResult>(result);
        }
    }
}