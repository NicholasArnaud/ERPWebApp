using ERPWebApp.Controllers;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace ERPWebApp.UnitTests.Controllers
{
    [Trait("Category", "pending")]
    public class ReportControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock = new();
        private readonly Mock<IOrderShippingService> _orderShippingServiceMock = new();
        private readonly Mock<IOrderService> _orderServiceMock = new();
        private readonly Mock<IStocksService> _stocksServiceMock = new();
        private readonly Mock<ISiteService> _siteServiceMock = new();
        private readonly Mock<ICycleCountService> _cycleCountServiceMock = new();
        private readonly Mock<IInventoryBalanceService> _InventoryBalanceServiceMock = new();
        private readonly Mock<ILocationService> _locationServiceMock = new();
        private readonly Mock<ISubCategoryService> _subCategoryServiceMock = new();
        private readonly Mock<IDepartmentService> _departmentServiceMock = new();
        private readonly Mock<IFinancialsService> _financialServiceMock = new();
        private readonly Mock<IShipStationStoreService> _shipStationStoreServiceMock = new();

        private readonly ReportController _reportController;

        public ReportControllerTests()
        {
            _reportController = new ReportController(_productServiceMock.Object,
                _orderShippingServiceMock.Object,
                _orderServiceMock.Object,
                _siteServiceMock.Object,
                _stocksServiceMock.Object,
                _cycleCountServiceMock.Object,
                _InventoryBalanceServiceMock.Object,
                _locationServiceMock.Object,
                _subCategoryServiceMock.Object,
                _departmentServiceMock.Object,
                _financialServiceMock.Object,
                _shipStationStoreServiceMock.Object
                );
        }

        [Fact]
        public void Index_WhenSucceed_ReturnsViewResultWithSkuData()
        {
            // Arrange
            var products = ProductFixtures.GetTestProducts();
            var sites = SiteFixtures.GetTestSites();
            var locations = LocationFixtures.GetTestLocations();
            var departments = DepartmentsFixtures.GetTestDepartments();
            var subcategories = SubCategoryFixtures.GetTestSubCategories();
            var shipstationStores = ShipStationStoreFixtures.GetTestShipStationStores();

            _ = _productServiceMock.Setup(static x => x.GetList(
                It.IsAny<Func<IQueryable<Product>, IQueryable<SelectListItem>>>()
            )).Returns(
                products.Select(static x => new SelectListItem { Value = x.ProductId.ToString(), Text = x.Sku })
                    .ToList()
            );

            _ = _siteServiceMock
                .Setup(static x => x.GetList(It.IsAny<Func<IQueryable<Site>, IQueryable<SelectListItem>>>()))
                .Returns(sites.Select(static x => new SelectListItem { Value = x.SiteId.ToString(), Text = x.SiteName }).ToList());

            _ = _locationServiceMock
                .Setup(static x => x.GetList(It.IsAny<Func<IQueryable<Location>, IQueryable<SelectListItem>>>()))
                .Returns(locations.Select(static x => new SelectListItem { Value = x.LocationId.ToString(), Text = x.LocationName }).ToList());

            _ = _departmentServiceMock
                .Setup(static x => x.GetList(It.IsAny<Func<IQueryable<Department>, IQueryable<SelectListItem>>>()))
                .Returns(departments.Select(static x => new SelectListItem { Value = x.DepartmentId.ToString(), Text = x.DepartmentName }).ToList());

            _ = _subCategoryServiceMock
                .Setup(static x => x.GetList(It.IsAny<Func<IQueryable<SubCategory>, IQueryable<SelectListItem>>>()))
                .Returns(subcategories.Select(static x => new SelectListItem { Value = x.SubCategoryId.ToString(), Text = x.Description }).ToList());

            _ = _shipStationStoreServiceMock
                .Setup(static x => x.GetList(It.IsAny<Func<IQueryable<ShipStationStore>, IQueryable<SelectListItem>>>()))
                .Returns(shipstationStores.Select(static x => new SelectListItem { Value = x.ShipStationStoreId.ToString(), Text = x.StoreName }).ToList());

            // Act
            var result = _reportController.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            _ = Assert.IsType<ViewResult>(result);

            var skuData = result.ViewData["SkuData"] as SelectList;
            Assert.NotNull(skuData);          

            var siteData = result.ViewData["siteData"] as SelectList;
            Assert.NotNull(siteData);                    

            var locationList = result.ViewData["locationList"] as SelectList;
            Assert.NotNull(locationList);

            var departmentList = result.ViewData["DepartmentList"] as SelectList;
            Assert.NotNull(departmentList);
        }

        [Theory]
        [InlineData(1, "2022-01-01", "2022-12-31", null, null, null)]
        [InlineData(2, "2022-01-01", "2022-12-31", null, null, null)]
        [InlineData(3, "2022-01-01", "2022-12-31", null, null, null)]
        [InlineData(4, "2022-01-01", "2022-12-31", 1, 1, 1)]
        public async Task GetQueries_WhenSucceed_ReturnsOkWithReportData(int query, string startDateString, string endDateString, int? productId, int? SiteId, int? locationId)
        {
            // Arrange
            var startDate = DateTime.Parse(startDateString);
            var endDate = DateTime.Parse(endDateString);
            var reportData = new List<Report>(){
                    new() {
                        Sku = "No matching records found",
                        Description = "No matching records found",
                        Service = "No matching records found",
                        CarrierCode = "No matching records found",
                        Average = 0,
                    }
            };

            if (query == 1)
            {
                _ = _orderShippingServiceMock.Setup(x => x.GetAvgShippingCostInDateRangeBySku(startDate, endDate)).Returns(reportData);
            }
            else if (query == 2)
            {
                _ = _orderShippingServiceMock.Setup(x => x.GetAvgShippingCostInDateRangeByService(startDate, endDate)).Returns(reportData);
            }
            else if (query == 3)
            {
                _ = _orderShippingServiceMock.Setup(x => x.GetAmountItemsShippedByDateRange(startDate, endDate)).Returns(reportData);
            }
            else if (query == 4)
            {
                _ = _orderShippingServiceMock.Setup(x => x.GetAmountShippedByDateRangeSkuFilter(productId, startDate, endDate)).Returns(reportData);
            }

            var expexted = new { recordsTotal = reportData.Count, data = reportData.ToList() };

            // Act
            var result = await _reportController.GetQueries(query, startDate, endDate, productId, SiteId, locationId) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var expectedJson = JsonConvert.SerializeObject(expexted);
            var actualJson = JsonConvert.SerializeObject(result.Value);
            Assert.Equal(expectedJson, actualJson);
        }

        [Theory]
        [InlineData(1, "2022-01-01", "2022-12-31", null, null, null)]
        [InlineData(2, "2022-01-01", "2022-12-31", null, null, null)]
        [InlineData(3, "2022-01-01", "2022-12-31", null, null, null)]
        [InlineData(4, "2022-01-01", "2022-12-31", 1, 1, 1)]
        public async Task GetQueries_WhenFailed_ShouldThrowException(int query, string startDateString, string endDateString, int? productId, int? SiteId, int? locationId)
        {
            // Arrange
            var startDate = DateTime.Parse(startDateString);
            var endDate = DateTime.Parse(endDateString);

            if (query == 1)
            {
                _ = _orderShippingServiceMock.Setup(x => x.GetAvgShippingCostInDateRangeBySku(startDate, endDate))
                                         .Throws(new Exception("Mock exception"));
            }
            else if (query == 2)
            {
                _ = _orderShippingServiceMock.Setup(x => x.GetAvgShippingCostInDateRangeByService(startDate, endDate))
                                         .Throws(new Exception("Mock exception"));
            }
            else if (query == 3)
            {
                _ = _orderShippingServiceMock.Setup(x => x.GetAmountItemsShippedByDateRange(startDate, endDate))
                                         .Throws(new Exception("Mock exception"));
            }
            else if (query == 4)
            {
                _ = _orderShippingServiceMock.Setup(x => x.GetAmountShippedByDateRangeSkuFilter(productId, startDate, endDate))
                                         .Throws(new Exception("Mock exception"));
            }

            // Act
            var result = await _reportController.GetQueries(query, startDate, endDate, productId, SiteId, locationId) as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model);
            var model = result.Model as List<Report>;
            Assert.Empty(model);
        }
    }
}
