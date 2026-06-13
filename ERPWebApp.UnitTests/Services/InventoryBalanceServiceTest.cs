using Microsoft.AspNetCore.Http;

namespace ERPWebApp.UnitTests.Services
{
    [Trait("Category", "execute")]
    public class InventoryBalanceServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IHttpContextAccessor> _mockHttp;
        private readonly IInventoryBalanceService _inventoryBalanceService;
        public InventoryBalanceServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockHttp = new Mock<IHttpContextAccessor>();
            _inventoryBalanceService = new InventoryBalanceService(_mockUnitOfWork.Object);
        }

        [Fact]
        public void When_InventoryBalanceHasValue_ReturnsReports()
        {
            // Arrange
            int productId = 1;
            var expectedReports = new List<Report>
            {
                new() { Sku = "ABC123", Description = "Product A", Amount = 10 },
                new() { Sku = "DEF456", Description = "Product B", Amount = 5 }
            };


            _ = _mockUnitOfWork.Setup(uow => uow.InventoryBalance.GetReport(productId))
                .Returns(expectedReports);


            // Act
            var result = _inventoryBalanceService.GetReport(productId);

            // Assert
            Assert.Equal(expectedReports, result);
        }
    }
}
