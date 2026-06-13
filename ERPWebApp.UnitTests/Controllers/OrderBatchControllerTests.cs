using ERPWebApp.Controllers;
using ERPWebApp.Data.DTOModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers
{
    [Trait("Category", "execute")]
    public class OrderBatchControllerTests
    {
        private readonly Mock<IOrderBatchService> _orderBatchServiceMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IStocksService> _stockServiceMock;
        private readonly Mock<IDepartmentService> _departmentServiceMock;
        private readonly OrderBatchController _controller;

        public OrderBatchControllerTests()
        {
            _orderBatchServiceMock = new Mock<IOrderBatchService>();
            _productServiceMock = new Mock<IProductService>();
            _stockServiceMock = new Mock<IStocksService>();
            _departmentServiceMock = new Mock<IDepartmentService>();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, "Administrator")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var mockHttpContext = new Mock<HttpContext>();
            var mockTempDataProvider = new Mock<ITempDataProvider>();

            _controller = new OrderBatchController(
                _orderBatchServiceMock.Object,
                _productServiceMock.Object,
                _stockServiceMock.Object,
                _departmentServiceMock.Object)
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
        public async Task TransferStock_Success_ReturnsSuccessJsonResult()
        {
            // Arrange  
            var stockTransfers = new List<StockTransfer>
            {
                new() {
                    FromLocationId = 1,
                    ToLocationId = 2,
                    ProductId = 1,
                    Quantity = 10,
                    OrderBatchId = 1,
                    OrderBatchItemId = 1,
                    OrderBatchItemIdList = [1, 2],
                    OrderBatchProductMappingId = 1
                }
            };

            _ = _orderBatchServiceMock.Setup(service => service.TransferStock(stockTransfers, It.IsAny<string>()))
                .ReturnsAsync((true, null, "NextStatus"));

            // Act  
            var result = await _controller.TransferStock(stockTransfers);

            // Assert  
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonData = jsonResult.Value;
            Assert.NotNull(jsonData);
            var successProperty = jsonData.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            Assert.True((bool?)successProperty.GetValue(jsonData, null) ?? false);
            var nextStatusProperty = jsonData.GetType().GetProperty("nextStatusName");
            Assert.NotNull(nextStatusProperty);
            Assert.Equal("NextStatus", nextStatusProperty.GetValue(jsonData, null));
        }

        [Fact]
        public async Task TransferStock_Failure_ReturnsFailureJsonResult()
        {
            // Arrange  
            var stockTransfers = new List<StockTransfer>
            {
                new() {
                    FromLocationId = 1,
                    ToLocationId = 2,
                    ProductId = 1,
                    Quantity = 10,
                    OrderBatchId = 1,
                    OrderBatchItemId = 1,
                    OrderBatchItemIdList = [1, 2],
                    OrderBatchProductMappingId = 1
                }
            };

            _ = _orderBatchServiceMock.Setup(service => service.TransferStock(stockTransfers, It.IsAny<string>()))
                .ReturnsAsync((false, "Error occurred", null));

            // Act  
            var result = await _controller.TransferStock(stockTransfers);

            // Assert  
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonData = jsonResult.Value;
            Assert.NotNull(jsonData);
            var successProperty = jsonData.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            Assert.False((bool?)successProperty.GetValue(jsonData, null) ?? false);
            var errorMessageProperty = jsonData.GetType().GetProperty("errorMessage");
            Assert.NotNull(errorMessageProperty);
            Assert.Equal("Error occurred", errorMessageProperty.GetValue(jsonData, null));
        }

        [Fact]
        public async Task TransferStock_ExceptionThrown_ReturnsFailureJsonResultWithExceptionMessage()
        {
            // Arrange  
            var stockTransfers = new List<StockTransfer>
            {
                new() {
                    FromLocationId = 1,
                    ToLocationId = 2,
                    ProductId = 1,
                    Quantity = 10,
                    OrderBatchId = 1,
                    OrderBatchItemId = 1,
                    OrderBatchItemIdList = [1, 2],
                    OrderBatchProductMappingId = 1
                }
            };

            var exceptionMessage = "Exception occurred";
            _ = _orderBatchServiceMock.Setup(service => service.TransferStock(stockTransfers, It.IsAny<string>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act  
            var result = await _controller.TransferStock(stockTransfers);

            // Assert  
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonData = jsonResult.Value;
            Assert.NotNull(jsonData);
            var successProperty = jsonData.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            Assert.False((bool?)successProperty.GetValue(jsonData, null) ?? false);
            var errorMessageProperty = jsonData.GetType().GetProperty("errorMessage");
            Assert.NotNull(errorMessageProperty);
            Assert.Equal(exceptionMessage, errorMessageProperty.GetValue(jsonData, null));
        }
    }
}