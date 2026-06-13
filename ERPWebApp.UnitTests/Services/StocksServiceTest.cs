using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Services
{
    [Trait("Category", "execute")]
    public class StocksServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IHttpContextAccessor> _mockHttp;
        private readonly IStocksService _stocksService;
        public StocksServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockHttp = new Mock<IHttpContextAccessor>();
            _stocksService = new StocksService(_mockUnitOfWork.Object, _mockHttp.Object);
        }


        #region SetupVolumeTally Test
        [Fact]
        public void SetupVolumeTally_WhenSucceed_ShouldReturnVolumeTally()
        {
            //Arrange

            var site = SiteFixtures.GetTestSites().FirstOrDefault(static x => x.SiteId == 1);
            var stockProductContainers = new List<StockProductContainer>{
                new() {
                    Stock = StockFixtures.GetTestStocks().FirstOrDefault(),
                    ProductContainer = new ProductContainer()
                    {
                        ProductVendorMappingId = 1,
                        ContainerQuantity = 10,
                        Length = 10.0m,
                        Width = 5.0m,
                        Height = 3.0m,
                        IsActive = true,
                        ContainerDiminsions = ContainerDiminsions.Feet,
                        ContainerCost = 500.0m,
                        ModifyDate = DateTime.Now,
                        ModifyByUser = "John Doe"
                    }
                },
                new() {
                    Stock = StockFixtures.GetTestStocks().FirstOrDefault(static x=>x.StockId == 2),
                    ProductContainer = new ProductContainer()
                    {
                        ProductVendorMappingId = 2,
                        ContainerQuantity = 20,
                        Length = 10.0m,
                        Width = 5.0m,
                        Height = 3.0m,
                        IsActive = true,
                        ContainerDiminsions = ContainerDiminsions.Meters,
                        ContainerCost = 500.0m,
                        ModifyDate = DateTime.Now,
                        ModifyByUser = "John Doe"
                    }
                }
            };

            _ = _mockUnitOfWork.Setup(static x => x.Stocks.GetStockProductContainersBySiteId(It.IsAny<int>()))
               .Returns(stockProductContainers.AsQueryable());
            //Act
            var result = _stocksService.SetupVolumeTally(1);

            //Assert
            _ = Assert.IsType<decimal>(result);
        }

        #endregion

        #region Get methods
        [Fact]
        public void GetProducts_WhenProductsExistsInDatabase_ShouldReturnSelectedData()
        {
            // Arrange
            int id = 1;
            string searchValue = "100";
            var stock = StockFixtures.GetTestStocks().FirstOrDefault(x => x.StockId == id);
            IEnumerable<QueryDataModel> queryDataModels = StockFixtures.GetTestStocks()
            .GroupBy(x => x.Products.Sku)
            .Select(x => new QueryDataModel
            {
                product = x.Key,
                Total = x.Where(z =>
                    z.Location.SiteId == 1
                    || z.Location.SiteId == 2
                    || z.Location.SiteId == 48
                    || z.Location.SiteId == 49)
                                .Sum(i => i.TotalAvailable),
                Description = x.First().Products.Description
            }).ToList();


            var product = ProductFixtures.GetTestProducts();
            var productVendorMapping = ProductVendorMappingFixtures.GetTestList();

            _ = _mockUnitOfWork.Setup(x => x.ProductVendorMappings.GetAll(
                It.IsAny<Expression<Func<ProductVendorMapping, string>>[]>(),
                It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()
            )).Returns(productVendorMapping);

            _ = _mockUnitOfWork.Setup(x => x.Stocks.GetListByFilter(
                    It.IsAny<Expression<Func<Stock, bool>>>(),
                    It.IsAny<Expression<Func<Stock, string>>[]>(),
                    It.IsAny<Expression<Func<Stock, object>>[]>()
                )
            )
            .Returns(StockFixtures.GetTestStocks());

            _ = _mockUnitOfWork.Setup(x => x.Stocks.QueryFilter(
                It.IsAny<Func<IQueryable<Stock>, IQueryable<QueryDataModel>>>()
            )).Returns(queryDataModels.AsQueryable());

            _ = _mockUnitOfWork.Setup(x => x.Products.GetAll(
                It.IsAny<Expression<Func<Product, string>>[]>(),
                It.IsAny<Expression<Func<Product, object>>[]>()
            )).Returns(product);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.Name, "John Doe"),
                new(ClaimTypes.Email, "johndoe@example.com"),
                new(ClaimTypes.Role, RoleList.Administrator),
            }));

            _ = _mockHttp.Setup(x => x.HttpContext.User).Returns(user);
            // Act
            var result = _stocksService.GetProducts(searchValue, true, 1, 1, 1, 1, 1, "Sku", "asc", 1);

            // Assert
            _ = result.Should().NotBeNull();
        }

        [Fact]
        public void GetProductsStock_WhenProductsStockExistsInDatabase_ShouldReturnSelectedProductsStock()
        {
            // Arrange
            string searchValue = "Warehouse A";
            var stock = StockFixtures.GetTestStocks();

            _ = _mockUnitOfWork.Setup(static x => x.Stocks.GetAllAsync(
                    It.IsAny<Expression<Func<Stock, string>>[]>(),
                    It.IsAny<Expression<Func<Stock, object>>[]>()
                )
            )
            .ReturnsAsync(stock);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                                        {
                                            new(ClaimTypes.Name, "John Doe"),
                                            new(ClaimTypes.Email, "johndoe@example.com"),
                                            new(ClaimTypes.Role, RoleList.Administrator),
                                        }));

            _ = _mockHttp.Setup(static x => x.HttpContext.User).Returns(user);
            // Act
            var result = _stocksService.GetProductsStock(searchValue, "Sku", "asc", "", "Yes", 1);

            // Assert
            _ = result.Should().NotBeNull();
        }
        #endregion

        #region Reports
        [Fact]
        public void GetOnHandBySiteFilter_WhenStockExistsInDatabase_ReturnsReports()
        {
            // Arrange
            int siteId = 123; // Replace with actual site ID
            var expectedReports = new List<Report>
            {
                new() { Sku = "ABC123", Description = "Product A", Amount = 10 },
                new() { Sku = "DEF456", Description = "Product B", Amount = 5 }
            };

            _ = _mockUnitOfWork.Setup(uow => uow.Stocks.GetOnHandReport(siteId))
                .Returns(expectedReports);


            // Act
            var result = _stocksService.GetOnHandBySiteFilter(siteId);

            // Assert
            Assert.Equal(expectedReports, result);
        }

        [Fact]
        public void StockHistoryReportOld_WhenStockHistoryExistsInDatabase_ReturnsReports()
        {
            // Arrange
            int locationId = 1;
            DateTime selectedDate = new(2023, 1, 1);
            var expectedReports = new List<Report>
            {
                new() { Sku = "ABC123", Description = "Product A", Amount = 10 },
                new() { Sku = "DEF456", Description = "Product B", Amount = 5 }
            };

            _ = _mockUnitOfWork.Setup(uow => uow.Stocks.StockHistoryReport_Old(locationId, selectedDate))
                .Returns(expectedReports);


            // Act
            var result = _stocksService.StockHistoryReport_Old(locationId, selectedDate);

            // Assert
            Assert.Equal(expectedReports, result);
        }

        [Fact]
        public void GetStockHistoryReport_WhenStockHistoryExistsInDatabase_ReturnsReports()
        {
            // Arrange
            int siteId = 1;
            DateTime selectedDate = new(2023, 1, 1);
            var expectedReports = new List<Report>
            {
                new() { Sku = "ABC123", Description = "Product A", Amount = 10 },
                new() { Sku = "DEF456", Description = "Product B", Amount = 5 }
            };

            _ = _mockUnitOfWork.Setup(uow => uow.Stocks.GetStockHistoryReport(siteId, selectedDate))
                .Returns(expectedReports);


            // Act
            var result = _stocksService.GetStockHistoryReport(siteId, selectedDate);

            // Assert
            Assert.Equal(expectedReports, result);
        }
        #endregion
    }
}