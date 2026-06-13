using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ERPWebApp.UnitTests.Services
{
    [Trait("Category", "execute")]
    public class MoveStockHistoryServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IWebhooks> _mockWebhooks;
        private readonly Mock<IHttpContextAccessor> _mockHttp;
        private readonly IStocksService _stocksService;
        private readonly IMoveStockHistoryService _moveStockHistoryService;
        private readonly Mock<IMoveStockHistoryRepository> _moveStockHistoryRepository;
        private readonly Mock<ITriggerEmailAlertService> _mockTriggerEmailAlertService;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ISiteRepository> _mockSiteRepository;
        private readonly Mock<ILocationRepository> _mockLocationRepository;
        private readonly Mock<IStockRepository> _mockStockRepository;
        private readonly Mock<IRepository<Stock>> _mockStockBaseRepository;
        private readonly Mock<IMoveStockHistoryRepository> _mockMoveStockHistoryRepository;
        public MoveStockHistoryServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockWebhooks = new Mock<IWebhooks>();
            _mockHttp = new Mock<IHttpContextAccessor>();
            _mockTriggerEmailAlertService = new Mock<ITriggerEmailAlertService>();
            _moveStockHistoryRepository = new Mock<IMoveStockHistoryRepository>();

            _mockProductRepository = new Mock<IProductRepository>();
            _ = _mockProductRepository
                .Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(static (Expression<Func<Product, bool>> predicate, Expression<Func<Product, object>>[] _) =>
                {
                    var testProduct1 = new Product { ProductId = 1, Sku = "PCHIPBAG03ZSO01", IsActive = true, DepartmentList = [1], Departments = [new Department { DepartmentId = 1, DepartmentName = "Dep 1", IsProduction = true, IsActive = true }] };
                    var testProduct2 = new Product { ProductId = 2, Sku = "PCHIPBAGBBQSO01", IsActive = true, DepartmentList = [2], Departments = [new Department { DepartmentId = 2, DepartmentName = "Dep 2", IsProduction = true, IsActive = true }] };
                    var testProduct3 = new Product { ProductId = 3, Sku = "SKU-001", IsActive = true, DepartmentList = [3], Departments = [new Department { DepartmentId = 3, DepartmentName = "Dep 3", IsProduction = true, IsActive = true }] };
                    var testProduct4 = new Product { ProductId = 4, Sku = "CHKSKU01", IsActive = true, DepartmentList = [4], Departments = [new Department { DepartmentId = 4, DepartmentName = "Dep 4", IsProduction = true, IsActive = true }] };
                    var testProduct5 = new Product { ProductId = 5, Sku = "SKU001", IsActive = true, DepartmentList = [5], Departments = [new Department { DepartmentId = 5, DepartmentName = "Dep 5", IsProduction = true, IsActive = true }] };
                    var testProduct6 = new Product { ProductId = 6, Sku = "SKU002", IsActive = true, DepartmentList = [6], Departments = [new Department { DepartmentId = 6, DepartmentName = "Dep 6", IsProduction = true, IsActive = true }] };
                    var testProduct7 = new Product { ProductId = 7, Sku = "GIL.G500VL.L.BLACK", IsActive = true, DepartmentList = [6], Departments = [new Department { DepartmentId = 6, DepartmentName = "Dep 6", IsProduction = true, IsActive = true }] };
                    var testProduct8 = new Product { ProductId = 8, Sku = "GIL.G2400.XL.NAVY", IsActive = true, DepartmentList = [6], Departments = [new Department { DepartmentId = 6, DepartmentName = "Dep 6", IsProduction = true, IsActive = true }] };
                    var testProduct9 = new Product { ProductId = 9, Sku = "BEL.3739.L.NAVYBLUE", IsActive = true, DepartmentList = [6], Departments = [new Department { DepartmentId = 6, DepartmentName = "Dep 6", IsProduction = true, IsActive = true }] };
                    var testProducts = new List<Product> { testProduct1, testProduct2, testProduct3, testProduct4, testProduct5, testProduct6, testProduct7, testProduct8, testProduct9 };

                    return testProducts.FirstOrDefault(predicate.Compile());
                });
            _mockSiteRepository = new Mock<ISiteRepository>();
            _ = _mockSiteRepository
                .Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Site, bool>>>(), It.IsAny<Expression<Func<Site, object>>[]>()))
                .ReturnsAsync(static (Expression<Func<Site, bool>> predicate, Expression<Func<Site, object>>[] _) =>
                {
                    var testSite = new Site { SiteId = 1, SiteName = "Site 1", IsActive = true };
                    return testSite;
                });
            _mockLocationRepository = new Mock<ILocationRepository>();
            _ = _mockLocationRepository
                .Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Location, bool>>>(), It.IsAny<Expression<Func<Location, object>>[]>()))
                .ReturnsAsync(static (Expression<Func<Location, bool>> predicate, Expression<Func<Location, object>>[] includes) =>
                {
                    var testSite = new Site { SiteId = 1, SiteName = "Site 1", IsActive = true };
                    var testLocation1 = new Location { LocationId = 1, SiteId = 1, Sites = testSite, LocationName = "Location 1", IsActive = true };
                    var testLocation2 = new Location { LocationId = 2, SiteId = 1, Sites = testSite, LocationName = "Location B", IsActive = true };
                    var testLocation3 = new Location { LocationId = 3, SiteId = 1, Sites = testSite, LocationName = "Production", IsActive = true };
                    var testLocations = new List<Location> { testLocation1, testLocation2, testLocation3 };
                    var query = testLocations.AsQueryable();
                    foreach (var include in includes)
                    {
                        query = query.Include(include);
                    }
                    return query.FirstOrDefault(predicate.Compile());
                });
            _mockStockRepository = new Mock<IStockRepository>();
            _ = _mockStockRepository
                .Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Stock, bool>>>(), It.IsAny<Expression<Func<Stock, object>>[]>()))
                .ReturnsAsync(static (Expression<Func<Stock, bool>> predicate, Expression<Func<Stock, object>>[] includes) =>
                {
                    var testStocks = new List<Stock>
                    {
                        new () { TotalAvailable = 10, StockId = 1, ProductId = 1, LocationId = 1, Location = new Location { LocationId = 1, SiteId = 1,LocationName = "Location 1", IsActive = true, Sites = new Site { SiteId = 1, SiteName = "Site 1", IsActive = true } } },
                        new () { TotalAvailable = 20, StockId = 2, ProductId = 1, LocationId = 2, Location = new Location { LocationId = 2, SiteId = 1,LocationName = "Location B", IsActive = true, Sites = new Site { SiteId = 1, SiteName = "Site 1", IsActive = true } } },  
                    // Add other stocks as needed
                    };
                    var query = testStocks.AsQueryable();
                    foreach (var include in includes)
                    {
                        query = query.Include(include);
                    }
                    return query.FirstOrDefault(predicate.Compile());
                });

            _mockStockBaseRepository = new Mock<IRepository<Stock>>();
            _ = _mockStockBaseRepository.Setup(static repo => repo.Update(It.IsAny<Stock>()));

            _mockMoveStockHistoryRepository = new Mock<IMoveStockHistoryRepository>();
            _ = _mockMoveStockHistoryRepository.Setup(static repo => repo.AddAsync(It.IsAny<MoveStockHistory>()))
            .ReturnsAsync(static (MoveStockHistory history) => history);

            _ = _mockUnitOfWork.Setup(static x => x.Products).Returns(_mockProductRepository.Object);
            _ = _mockUnitOfWork.Setup(static x => x.Sites).Returns(_mockSiteRepository.Object);
            _ = _mockUnitOfWork.Setup(static x => x.Locations).Returns(_mockLocationRepository.Object);
            _ = _mockUnitOfWork.Setup(static x => x.Stocks).Returns(_mockStockRepository.Object);
            _ = _mockUnitOfWork.Setup(static x => x.GetRepository<Stock>()).Returns(_mockStockBaseRepository.Object);
            _ = _mockUnitOfWork.Setup(static x => x.GetRepository<MoveStockHistory>()).Returns(_mockMoveStockHistoryRepository.Object);
            _ = _mockUnitOfWork.Setup(static x => x.MoveStockHistories).Returns(_mockMoveStockHistoryRepository.Object);

            _stocksService = new StocksService(_mockUnitOfWork.Object, _mockHttp.Object);
            _moveStockHistoryService = new MoveStockHistoryService(_mockUnitOfWork.Object);
        }
        [Fact]
        public async Task MoveStock_MoveProductAToLocationB_ReturnsMoveStock()
        {
            //Arrange
            MoveStock moveStock = new()
            {
                FromStock = await _mockStockRepository.Object.FilterOneAsync(static x => x.LocationId == 1 && x.ProductId == 1,
                    includes: [static x => x.Location, static x => x.Location.Sites]),
                ToStock = await _mockStockRepository.Object.FilterOneAsync(static x => x.LocationId == 2 && x.ProductId == 1,
                    includes: [static x => x.Location, static x => x.Location.Sites]),
                quantity = 5,
                DateTime = DateTime.Now,
                ModifiedBy = "John Doe",
                StockHistory = []
            };
            var updatedMoveStock = await _moveStockHistoryService.MoveStock(moveStock);
            var getFromStock = await _mockStockRepository.Object.FilterOneAsync(static x => x.LocationId == 1 && x.ProductId == 1);
            var getToStock2 = await _mockStockRepository.Object.FilterOneAsync(static x => x.LocationId == 2 && x.ProductId == 1);
            Assert.Equal(5, updatedMoveStock.FromStock.TotalAvailable);
            Assert.Equal(25, updatedMoveStock.ToStock.TotalAvailable);
        }
    }
}
