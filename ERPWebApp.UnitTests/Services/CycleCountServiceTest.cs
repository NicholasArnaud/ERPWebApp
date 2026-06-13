using Microsoft.AspNetCore.Http;

namespace ERPWebApp.UnitTests.Services
{
    [Trait("Category", "execute")]
    public class CycleCountServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IHttpContextAccessor> _mockHttp;
        private readonly ICycleCountService _cycleCountService;
        public CycleCountServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockHttp = new Mock<IHttpContextAccessor>();
            _cycleCountService = new CycleCountService(_mockUnitOfWork.Object);
        }

        [Fact]
        public void When_CycleCountHasValue_ReturnsReports()
        {
            // Arrange
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 12, 31); 
            int locationId = 123;
            var expectedReports = new List<Report>
            {
                new() { Sku = "ABC123", Description = "Product A", Amount = 10 },
                new() { Sku = "DEF456", Description = "Product B", Amount = 5 }
            };

            _ = _mockUnitOfWork.Setup(uow => uow.CycleCountes.GetCycleCountReport(startDate, endDate, locationId))
                .Returns(expectedReports);


            // Act
            var result = _cycleCountService.GetCycleCountReport(startDate, endDate, locationId);

            // Assert
            Assert.Equal(expectedReports, result);
        }
    }
}
