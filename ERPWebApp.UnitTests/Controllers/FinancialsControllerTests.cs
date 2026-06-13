using System.Security.Claims;
using System.Text;
using ERPWebApp.Controllers;
using ERPWebApp.Data.DTOModels;
using Microsoft.AspNetCore.Http;

namespace ERPWebApp.UnitTests.Controllers
{
    [Trait("Category", "execute")]
    public class FinancialsControllerTests
    {
        private readonly Mock<IFinancialsService> _financialsServiceMock = new();
        private readonly Mock<IDepartmentService> _departmentServiceMock = new();
        private readonly Mock<IUserPreferencesService> _userPreferencesServiceMock = new();
        private readonly Mock<IMyDashService> _myDashServiceMock = new();
        private readonly FinancialsController _controller;

        public FinancialsControllerTests()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.BasicUser),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            _controller = new FinancialsController(_financialsServiceMock.Object, _departmentServiceMock.Object, _userPreferencesServiceMock.Object, _myDashServiceMock.Object) {
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
        public async Task IndexAsync_WhenSucceed_ReturnsViewWithFinancialsViewModel()
        {
            // Arrange
            _ = _financialsServiceMock.Setup(static s => s.FulfillmentTable()).ReturnsAsync([]);
            _ = _financialsServiceMock.Setup(static s => s.ProductSalesTable(It.IsAny<int>())).ReturnsAsync([]);
            _ = _userPreferencesServiceMock.Setup(static s => s.GetDashboardLayoutByDashboardAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync([]);

            // Act
            var result = await _controller.IndexAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            _ = Assert.IsAssignableFrom<FinancialsViewModel>(viewResult.Model);
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

        [Fact]
        public async Task HistoricalTrends_WhenSucceed_ReturnsTrendsInfoList()
        {
            // Arrange  
            DateTime startDate = DateTime.Today.AddDays(-30);
            DateTime endDate = DateTime.Today;
            var expectedData = new List<TrendsInfoDto>();
            _ = _financialsServiceMock.Setup(s => s.TrendsTable(startDate, endDate)).ReturnsAsync(expectedData);

            // Act  
            var result = await _controller.HistoricalTrends(startDate, endDate);

            // Assert  
            Assert.Equal(expectedData, result);
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
        public async Task ProductSalesView_WhenSucceed_ReturnsProductSalesView()
        {
            // Arrange
            var days = 60;
            var productSalesInfo = new List<ProductSalesInfoDto>();

            _ = _financialsServiceMock.Setup(s => s.ProductSalesTable(days)).ReturnsAsync(productSalesInfo);

            // Act
            var result = await _controller.ProductSalesView(days) as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ProductSalesTablePartial", result.ViewName);
            Assert.Equal(productSalesInfo, result.Model);
        }

        [Fact]
        public async Task ExportFulfillmentToCSV_ReturnsFileResultWithCSVData()
        {
            // Arrange
            var data = new List<FulfillmentInfoDto>();
            var csv = "CSV data";

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller.ControllerContext.HttpContext.Response.Body = memoryStream;
            _controller.ControllerContext.HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=fulfillment_export.csv";
            _controller.ControllerContext.HttpContext.Response.ContentType = "text/csv";

            _ = _financialsServiceMock.Setup(static s => s.FulfillmentTable()).ReturnsAsync(data);

            // Act
            var result = await _controller.ExportFulfillmentToCSV();

            // Assert
            Assert.NotNull(result);
            _ = Assert.IsAssignableFrom<FileResult>(result);

            var fileResult = (FileStreamResult)result;
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal("fulfillment_export.csv", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportTrendsToCSV_ReturnsFileResultWithCSVData()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(-7);
            DateTime endDate = DateTime.Today;
            var data = new List<TrendsInfoDto>();
            var csv = "CSV data";

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller.ControllerContext.HttpContext.Response.Body = memoryStream;
            _controller.ControllerContext.HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=historical_trends_export.csv";
            _controller.ControllerContext.HttpContext.Response.ContentType = "text/csv";

            _ = _financialsServiceMock.Setup(s => s.TrendsTable(startDate, endDate)).ReturnsAsync(data);

            // Act
            var result = await _controller.ExportTrendsToCSV(startDate, endDate);

            // Assert
            Assert.NotNull(result);
            _ = Assert.IsAssignableFrom<FileResult>(result);

            var fileResult = (FileStreamResult)result;
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal("historical_trends_export.csv", fileResult.FileDownloadName);
        }
    }
}