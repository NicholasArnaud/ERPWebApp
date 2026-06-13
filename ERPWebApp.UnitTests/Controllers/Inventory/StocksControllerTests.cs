using ERPWebApp.Controllers.Inventory;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers.Inventory
{
    public class StocksControllerTests
    {
        private readonly Mock<IStocksService> _stocksServiceMock = new();
        private readonly Mock<IProductService> _productServiceMock = new();
        private readonly Mock<ISiteService> _siteServiceMock = new();
        private readonly Mock<ISubCategoryService> _subCategoryServiceMock = new();
        private readonly Mock<IDepartmentService> _departmentServiceMock = new();
        private readonly Mock<ILocationService> _locationServiceMock = new();
        private readonly Mock<IProductTagService> _productTagServiceMock = new();
        private readonly Mock<IVendorService> _vendorServiceMock = new();
        private readonly Mock<IShipStationStoreService> _shipstationStoreServiceMock = new();
        private readonly StocksController _controller;
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock = new(Mock.Of<IUserStore<IdentityUser>>());

        public StocksControllerTests()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.BasicUser),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

            _controller = new StocksController(
                _stocksServiceMock.Object,
                _productServiceMock.Object,
                _siteServiceMock.Object,
                _departmentServiceMock.Object,
                _subCategoryServiceMock.Object,
                _locationServiceMock.Object,
                _productTagServiceMock.Object,
                _vendorServiceMock.Object,
                _shipstationStoreServiceMock.Object,
                _userManagerMock.Object
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

        #region Unit Tests for Index

        [Fact]
        public void Index_ReturnsViewResult_WithCorrectViewData()
        {
            // Arrange

            // Set up the mock service methods to return dummy data
            var allSites = SiteFixtures.GetTestSites();

            var allDepartments = DepartmentsFixtures.GetTestDepartments();

            var allSubCategories = SubCategoriesFixtures.GetTestSubCategories();

            var allProductTags = ProductTagsRegistryFixtures.GetTestProductTags();

            var allVendors = VendorFixtures.GetTestList();

            _ = _siteServiceMock.Setup(static s => s.GetAll(null, null)).Returns(allSites);
            _ = _departmentServiceMock.Setup(static d => d.GetAll(
                    It.IsAny<Expression<Func<Department, string>>[]>(),
                    It.IsAny<Expression<Func<Department, object>>[]>()
            )).Returns(allDepartments);
            _ = _subCategoryServiceMock.Setup(static s => s.GetAll(
                    It.IsAny<Expression<Func<SubCategory, string>>[]>(),
                    It.IsAny<Expression<Func<SubCategory, object>>[]>()
            )).Returns(allSubCategories);
            _ = _productTagServiceMock.Setup(static t => t.GetAll(
                    It.IsAny<Expression<Func<ProductTagsRegistry, string>>[]>(),
                    It.IsAny<Expression<Func<ProductTagsRegistry, object>>[]>()
            ));
            _ = _vendorServiceMock.Setup(static v => v.GetAll(
                    It.IsAny<Expression<Func<Vendor, string>>[]>(),
                    It.IsAny<Expression<Func<Vendor, object>>[]>()
            ));

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["SiteName"]);
            Assert.NotNull(viewResult.ViewData["DepartmentName"]);
            Assert.NotNull(viewResult.ViewData["SubCategoryList"]);
            Assert.NotNull(viewResult.ViewData["ProductTagList"]);
            Assert.NotNull(viewResult.ViewData["VendorsList"]);

            if (_controller.User.IsInRole(RoleList.Administrator) || _controller.User.IsInRole(RoleList.ExternalViewer))
            {
                _ = Assert.IsType<SelectList>(viewResult.ViewData["SiteName"]);
                _ = Assert.IsType<SelectList>(viewResult.ViewData["DepartmentName"]);
                _ = Assert.IsType<SelectList>(viewResult.ViewData["SubCategoryList"]);
                _ = Assert.IsType<SelectList>(viewResult.ViewData["ProductTagList"]);
                _ = Assert.IsType<SelectList>(viewResult.ViewData["VendorsList"]);

                var siteNameSelectList = viewResult.ViewData["SiteName"] as SelectList 
                                         ?? throw new InvalidOperationException("SiteName ViewData is null or not a SelectList.");
                var departmentNameSelectList = viewResult.ViewData["DepartmentName"] as SelectList 
                                               ?? throw new InvalidOperationException("DepartmentName ViewData is null or not a SelectList.");
                var subCategoryListSelectList = viewResult.ViewData["SubCategoryList"] as SelectList 
                                                 ?? throw new InvalidOperationException("SubCategoryList ViewData is null or not a SelectList.");
                var productTagSelectList = viewResult.ViewData["ProductTagList"] as SelectList 
                                             ?? throw new InvalidOperationException("ProductTagList ViewData is null or not a SelectList.");
                var vendorSelectList = viewResult.ViewData["VendorsList"] as SelectList 
                                         ?? throw new InvalidOperationException("VendorsList ViewData is null or not a SelectList.");

                Assert.Equal(allSites.Count(), siteNameSelectList.Count());
                Assert.Equal(allDepartments.Count(), departmentNameSelectList.Count());
                Assert.Equal(allSubCategories.Count(), subCategoryListSelectList.Count());
                Assert.Equal(allProductTags.Count(), productTagSelectList.Count());
                Assert.Equal(allVendors.Count(), vendorSelectList.Count());
            }
            else if (_controller.User.IsInRole(RoleList.ExternalUser))
            {
                _ = Assert.IsType<SelectList>(viewResult.ViewData["SiteName"]);
                _ = Assert.IsType<SelectList>(viewResult.ViewData["DepartmentName"]);
                _ = Assert.IsType<SelectList>(viewResult.ViewData["SubCategoryList"]);
                _ = Assert.IsType<SelectList>(viewResult.ViewData["ProductTagList"]);
                _ = Assert.IsType<SelectList>(viewResult.ViewData["VendorsList"]);

                var siteNameSelectList = viewResult.ViewData["SiteName"] as SelectList 
                                         ?? throw new InvalidOperationException("SiteName ViewData is null or not a SelectList.");
                var departmentNameSelectList = viewResult.ViewData["DepartmentName"] as SelectList 
                                               ?? throw new InvalidOperationException("DepartmentName ViewData is null or not a SelectList.");
                var subCategoryListSelectList = viewResult.ViewData["SubCategoryList"] as SelectList 
                                                 ?? throw new InvalidOperationException("SubCategoryList ViewData is null or not a SelectList.");
                var productTagSelectList = viewResult.ViewData["ProductTagList"] as SelectList 
                                             ?? throw new InvalidOperationException("ProductTagList ViewData is null or not a SelectList.");
                var vendorSelectList = viewResult.ViewData["VendorsList"] as SelectList 
                                         ?? throw new InvalidOperationException("VendorsList ViewData is null or not a SelectList.");

                var externalSites = allSites.Where(static s => s.IsExternal);
                var externalDept = allDepartments.Where(static d => d.DepartmentName.Equals("External"));

                Assert.Equal(externalSites.Count(), siteNameSelectList.Count());
                _ = Assert.Single(departmentNameSelectList);
                Assert.Equal(allSubCategories.Count(), subCategoryListSelectList.Count());
                Assert.Equal(allProductTags.Count(), productTagSelectList.Count());
                Assert.Equal(allVendors.Count(), vendorSelectList.Count());
            }
            else
            {
                _ = Assert.IsType<SelectList>(viewResult.ViewData["SiteName"]);
                _ = Assert.IsType<SelectList>(viewResult.ViewData["DepartmentName"]);
                _ = Assert.IsType<SelectList>(viewResult.ViewData["SubCategoryList"]);
                _ = Assert.IsType<SelectList>(viewResult.ViewData["ProductTagList"]);
                _ = Assert.IsType<SelectList>(viewResult.ViewData["VendorsList"]);

                var siteNameSelectList = viewResult.ViewData["SiteName"] as SelectList 
                                         ?? throw new InvalidOperationException("SiteName ViewData is null or not a SelectList.");
                var departmentNameSelectList = viewResult.ViewData["DepartmentName"] as SelectList 
                                               ?? throw new InvalidOperationException("DepartmentName ViewData is null or not a SelectList.");
                var subCategoryListSelectList = viewResult.ViewData["SubCategoryList"] as SelectList 
                                                 ?? throw new InvalidOperationException("SubCategoryList ViewData is null or not a SelectList.");
                var productTagSelectList = viewResult.ViewData["ProductTagList"] as SelectList 
                                             ?? throw new InvalidOperationException("ProductTagList ViewData is null or not a SelectList.");
                var vendorSelectList = viewResult.ViewData["VendorsList"] as SelectList 
                                         ?? throw new InvalidOperationException("VendorsList ViewData is null or not a SelectList.");

                var internalSites = allSites.Where(static s => !s.IsExternal);
                var internalDepts = allDepartments.Where(static d => !d.DepartmentName.Equals("External"));

                Assert.Equal(internalSites.Count(), siteNameSelectList.Count());
                Assert.Equal(internalDepts.Count(), departmentNameSelectList.Count());
                Assert.Equal(allSubCategories.Count(), subCategoryListSelectList.Count());
                Assert.Equal(allProductTags.Count(), productTagSelectList.Count());
                Assert.Equal(allVendors.Count(), vendorSelectList.Count());
            }
        }

        #endregion


        #region Unit Tests for GetProducts

        [Fact]
        public void GetProducts_ReturnsOkResult_WithCorrectData()
        {
            // Arrange

            // Set up the mock service method to return dummy data
            var dummyProducts = ProductFixtures.GetTestProducts();

            _ = _stocksServiceMock.Setup(static s => s.GetProducts(
                It.IsAny<string>(),
                It.IsAny<bool?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>()
            )).Returns(dummyProducts.AsQueryable());

            _controller.Request.Form = new FormCollection(new Dictionary<string, StringValues>
              {
                  { "draw", new StringValues("1") },
                  { "start", new StringValues("0") },
                  { "length", new StringValues("10") },
                  { "order[0][column]", new StringValues("0") },
                  { "order[0][dir]", new StringValues("asc") },
                  { "search[value]", new StringValues("") }
              });

            // Act
            var result = _controller.GetProducts(null, null, null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        #endregion


        #region Unit Tests for GetProductsStock

        [Fact]
        public void GetProductsStock_ReturnsOkResult_WithCorrectData()
        {
            // Arrange

            // Set up the mock service method to return dummy data
            var dummyStocks = StockFixtures.GetTestStocks();

            _ = _stocksServiceMock.Setup(static s => s.GetProductsStock(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>()
            )).Returns(dummyStocks.AsQueryable());

            _controller.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "draw", new StringValues("1") },
                { "start", new StringValues("0") },
                { "length", new StringValues("10") },
                { "order[0][column]", new StringValues("0") },
                { "order[0][dir]", new StringValues("asc") },
                { "search[value]", new StringValues("") }
            });

            // Set up the ViewData for role
            _controller.ViewData["role"] = "Administrator";

            // Act
            var result = _controller.GetProductsStock(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        #endregion


        #region Unit Tests for Details Get

        [Fact]
        public async Task Details_ValidId_ReturnsViewResult()
        {
            // Arrange
            int validId = 1;
            var expectedStock = StockFixtures.GetTestStocks().First();
            _ = _stocksServiceMock.Setup(static mock => mock.GetAsync(
                It.IsAny<Expression<Func<Stock, bool>>>(),
                It.IsAny<Expression<Func<Stock, object>>[]>()
            )).ReturnsAsync(expectedStock);

            // Act
            var result = await _controller.Details(validId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedStock, viewResult.Model);
        }

        [Fact]
        public async Task Details_InvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            int invalidId = 0;

            // Act
            var result = await _controller.Details(invalidId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        #endregion


        #region Unit Tests for Create Get

        [Fact]
        public void Create_ReturnsViewResult_WithCorrectData_ForAdminOrExternalViewerRole()
        {
            // Arrange

            // Set up the mock services to return dummy data
            var dummySites = new List<Site>
            {
                new() { SiteId = 1, SiteName = "Site 1", IsExternal = false },
                new() { SiteId = 2, SiteName = "Site 2", IsExternal = true }
            };

            var dummyLocations = new List<Location>
            {
                new() { LocationId = 1, LocationName = "Location 1", IsExternal = false },
                new() { LocationId = 2, LocationName = "Location 2", IsExternal = true }
            };

            var dummyProducts = new List<Product>
            {
                new() { ProductId = 1, Sku = "SKU1", IsActive = true, IsExternalProduct = false },
                new() { ProductId = 2, Sku = "SKU2", IsActive = true, IsExternalProduct = true }
            };

            _ = _siteServiceMock.Setup(static s => s.GetAll(null, null)).Returns(dummySites);
            _ = _locationServiceMock.Setup(static l => l.GetAll(null, null)).Returns(dummyLocations);
            _ = _productServiceMock.Setup(static p => p.GetList(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<Product, string>>[]>(),
                It.IsAny<Expression<Func<Product, object>>[]>()
            )).Returns(dummyProducts);

            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            // Assert the ViewData for SiteName, LocationName, and ProductId
            var siteNameSelectList = Assert.IsType<SelectList>(viewResult.ViewData["SiteName"]);
            var locationNameSelectList = Assert.IsType<SelectList>(viewResult.ViewData["LocationName"]);
            var productIdSelectList = Assert.IsType<SelectList>(viewResult.ViewData["ProductId"]);

            Assert.Equal(dummySites.Where(static x => x.IsExternal == true).Count(), siteNameSelectList.Count());
            Assert.Equal(dummyLocations.Where(static x => x.IsExternal == true).Count(), locationNameSelectList.Count());
            Assert.Equal(dummyProducts.Where(static x => x.IsExternalProduct == true).Count(), productIdSelectList.Count());
        }

        #endregion


        #region Unit Tests for Create Post

        [Fact]
        public async Task Create_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var validStock = StockFixtures.GetTestStocks().First();
            _ = _stocksServiceMock.Setup(mock => mock.AddAsync(validStock)).ReturnsAsync(validStock);
            var now = DateTime.Now;

            var stockViewModel = new StockViewModel
            {
                Stock = validStock
            };

            // Act
            var result = await _controller.Create(stockViewModel.Stock);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Create_InternalUser_ReturnsViewResultWithInternalData()
        {
            // Arrange
            var internalUserStock = StockFixtures.GetTestStocks().First();
            var allSites = new List<Site>
            {
                new() { SiteId = 1, SiteName = "Site A", IsExternal = false },
                new() { SiteId = 2, SiteName = "Site B", IsExternal = true }
            };
            var allLocations = new List<Location>
            {
                new() { LocationId = 1, LocationName = "Location A", IsExternal = false },
                new() { LocationId = 2, LocationName = "Location B", IsExternal = true }
            };
            var allActiveProducts = new List<Product>
            {
                new() { ProductId = 1, Sku = "Product A", IsActive = true, IsExternalProduct = false },
                new() { ProductId = 2, Sku = "Product B", IsActive = true, IsExternalProduct = true }
            };
            _controller.ModelState.AddModelError("Stock", "Invalid Stock details");
            _ = _siteServiceMock.Setup(static mock => mock.GetAll(null, null)).Returns(allSites);
            _ = _locationServiceMock.Setup(static mock => mock.GetAll(null, null)).Returns(allLocations);
            _ = _productServiceMock.Setup(static mock => mock.GetList(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<Product, string>>[]>(),
                It.IsAny<Expression<Func<Product, object>>[]>()
                ))
            .Returns(allActiveProducts);

            var stockViewModel = new StockViewModel
            {
                Stock = internalUserStock
            };

            // Act
            var result = await _controller.Create(stockViewModel.Stock);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var siteNameSelectList = Assert.IsType<SelectList>(viewResult.ViewData["SiteName"]);
            var locationNameSelectList = Assert.IsType<SelectList>(viewResult.ViewData["LocationName"]);
            var productIdSelectList = Assert.IsType<SelectList>(viewResult.ViewData["ProductId"]);
            var expectedInternalSites = allSites.Where(static s => !s.IsExternal).OrderBy(static s => s.SiteName);
            var expectedInternalLocations = allLocations.Where(static l => !l.IsExternal).OrderBy(static l => l.LocationName);
            var expectedInternalProducts = allActiveProducts.Where(static ap => !ap.IsExternalProduct).OrderBy(static ap => ap.Sku);
            Assert.Equal(expectedInternalSites.Count(), siteNameSelectList.Count());
            Assert.Equal(expectedInternalLocations.Count(), locationNameSelectList.Count());
            Assert.Equal(expectedInternalProducts.Count(), productIdSelectList.Count());
        }

        [Fact]
        public async Task Create_ExternalViewer_ReturnsViewResultWithAllData()
        {
            // Arrange
            var externalViewerStock = StockFixtures.GetTestStocks().First();
            var allSites = new List<Site>
            {
                new() { SiteId = 1, SiteName = "Site A", IsExternal = false },
                new() { SiteId = 2, SiteName = "Site B", IsExternal = true }
            };
            var allLocations = new List<Location>
            {
                new() { LocationId = 1, LocationName = "Location A", IsExternal = false },
                new() { LocationId = 2, LocationName = "Location B", IsExternal = true }
            };
            var allActiveProducts = new List<Product>
            {
                new() { ProductId = 1, Sku = "Product A", IsActive = true, IsExternalProduct = false },
                new() { ProductId = 2, Sku = "Product B", IsActive = true, IsExternalProduct = true }
            };
            _controller.ModelState.AddModelError("Stock", "Invalid Stock details");
            _ = _siteServiceMock.Setup(static mock => mock.GetAll(null, null)).Returns(allSites);
            _ = _locationServiceMock.Setup(static mock => mock.GetAll(null, null)).Returns(allLocations);
            _ = _productServiceMock.Setup(static mock => mock.GetList(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<Product, string>>[]>(),
                It.IsAny<Expression<Func<Product, object>>[]>()
                ))
            .Returns(allActiveProducts);

            var stockViewModel = new StockViewModel
            {
                Stock = externalViewerStock
            };

            // Act
            var result = await _controller.Create(stockViewModel.Stock);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var siteNameSelectList = Assert.IsType<SelectList>(viewResult.ViewData["SiteName"]);
            var locationNameSelectList = Assert.IsType<SelectList>(viewResult.ViewData["LocationName"]);
            var productIdSelectList = Assert.IsType<SelectList>(viewResult.ViewData["ProductId"]);
            Assert.Equal(allSites.Where(static s => s.IsExternal == true).Count(), siteNameSelectList.Count());
            Assert.Equal(allLocations.Where(static s => s.IsExternal == true).Count(), locationNameSelectList.Count());
            Assert.Equal(allActiveProducts.Where(static s => s.IsExternalProduct == true).Count(), productIdSelectList.Count());
        }

        #endregion


        #region Unit Tests for Edit Get

        [Fact]
        public async Task Edit_WithValidId_ReturnsViewResultWithFilteredData()
        {
            // Arrange
            int stockId = 1;
            var stock = StockFixtures.GetTestStocks().First();

            _ = _stocksServiceMock.Setup(static mock => mock.GetAsync(
                It.IsAny<Expression<Func<Stock, bool>>>(),
                It.IsAny<Expression<Func<Stock, object>>[]>()))
            .ReturnsAsync(stock);

            _ = _siteServiceMock.Setup(static mock => mock.GetAll(null, null))
                .Returns(
                [
                new() { SiteId = 1, SiteName = "Site A", IsExternal = false },
                new() { SiteId = 2, SiteName = "Site B", IsExternal = true }
                ]);

            _ = _locationServiceMock.Setup(static mock => mock.GetList(
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Expression<Func<Location, string>>[]>(),
                It.IsAny<Expression<Func<Location, object>>[]>()
                ))
            .Returns(
            [
                new() { LocationId = 1, LocationName = "Location A", SiteId = 1, IsExternal = false },
                new() { LocationId = 2, LocationName = "Location B", SiteId = 1, IsExternal = true }
            ]);

            _ = _productServiceMock.Setup(static mock => mock.GetAll(
                It.IsAny<Expression<Func<Product, string>>[]>(),
                It.IsAny<Expression<Func<Product, object>>[]>()
                ))
            .Returns(
            [
            new() { ProductId = 1, Sku = "Product A", IsExternalProduct = false },
            new() { ProductId = 2, Sku = "Product B", IsExternalProduct = true }
            ]);

            // Act
            var result = await _controller.Edit(stockId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var siteNameSelectList = Assert.IsType<SelectList>(viewResult.ViewData["SiteName"]);
            var locationNameSelectList = Assert.IsType<SelectList>(viewResult.ViewData["LocationName"]);
            var productIdSelectList = Assert.IsType<SelectList>(viewResult.ViewData["ProductId"]);
        }

        #endregion


        #region Unit Tests for Edit Post

        [Fact]
        public async Task Edit_OR_Update_WithInvalidModel_ReturnsViewResultWithFilteredData()
        {
            // Arrange
            int stockId = 1;
            var stock = StockFixtures.GetTestStocks().First();

            _ = _stocksServiceMock.Setup(static mock => mock.GetAsync(
                It.IsAny<Expression<Func<Stock, bool>>>(),
                It.IsAny<Expression<Func<Stock, object>>[]>()))
            .ReturnsAsync(stock);

            _ = _siteServiceMock.Setup(static mock => mock.GetAll(null, null))
                .Returns(SiteFixtures.GetTestSites());

            _ = _locationServiceMock.Setup(static mock => mock.GetList(
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Expression<Func<Location, string>>[]>(),
                It.IsAny<Expression<Func<Location, object>>[]>()
                ))
            .Returns(
            [
                new() { LocationId = 1, LocationName = "Location A", SiteId = 1, IsExternal = false },
                new() { LocationId = 2, LocationName = "Location B", SiteId = 1, IsExternal = true }
            ]);

            _ = _productServiceMock.Setup(static mock => mock.GetAll(
                It.IsAny<Expression<Func<Product, string>>[]>(),
                It.IsAny<Expression<Func<Product, object>>[]>()
                ))
            .Returns(
            [
            new() { ProductId = 1, Sku = "Product A", IsExternalProduct = false },
            new() { ProductId = 2, Sku = "Product B", IsExternalProduct = true }
            ]);

            // Add ModelState error to simulate an invalid model state
            _controller.ModelState.AddModelError("TotalAvailable", "TotalAvailable is required");

            // Act
            var result = await _controller.Edit(stockId, stock);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var siteNameSelectList = Assert.IsType<SelectList>(viewResult.ViewData["SiteName"]);
            var locationNameSelectList = Assert.IsType<SelectList>(viewResult.ViewData["LocationName"]);
            var productIdSelectList = Assert.IsType<SelectList>(viewResult.ViewData["ProductId"]);
        }

        [Fact]
        public async Task Edit_OR_Update_WithValidId_ReturnsViewResultWithFilteredData()
        {
            // Arrange
            int stockId = 1;
            var stock = new Stock { StockId = stockId, Location = new Location { SiteId = 1 } };

            _ = _stocksServiceMock.Setup(mock => mock.Update(stock));
            _ = _stocksServiceMock.Setup(mock => mock.IsExists(It.IsAny<Expression<Func<Stock, bool>>>())).Returns(false);

            // Act
            var result = await _controller.Edit(stockId, stock);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(Index), redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Edit_OR_Update_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            int stockId = 1;
            var stock = new Stock { StockId = 2 };

            // Act
            var result = await _controller.Edit(stockId, stock);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        #endregion


        #region Unit Tests for Delete Get

        [Fact]
        public async Task Delete_WithValidId_ReturnsViewResult()
        {
            // Arrange
            int stockId = 1;

            _ = _stocksServiceMock.Setup(static mock => mock.GetAsync(
                    It.IsAny<Expression<Func<Stock, bool>>>(),
                    It.IsAny<Expression<Func<Stock, object>>[]>()))
                .ReturnsAsync(new Stock { StockId = stockId });

            // Act
            var result = await _controller.Delete(stockId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Stock>(viewResult.Model);
            Assert.Equal(stockId, model.StockId);
        }

        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            int stockId = 1;

            _ = _stocksServiceMock.Setup(static mock => mock.GetAsync(
                    It.IsAny<Expression<Func<Stock, bool>>>(),
                    It.IsAny<Expression<Func<Stock, object>>[]>()))
                .ReturnsAsync((Stock?)null);

            // Act
            var result = await _controller.Delete(stockId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WithNullId_ReturnsNotFoundResult()
        {
            // Arrange
            int? stockId = null;

            // Act
            var result = await _controller.Delete(stockId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        #endregion


        #region Unit Tests for DeleteConfirmed Post

        [Fact]
        public async Task DeleteConfirmed_WithValidId_RedirectsToIndexAction()
        {
            // Arrange
            int stockId = 1;

            _ = _stocksServiceMock.Setup(mock => mock.DeleteConfirmed(stockId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteConfirmed(stockId);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        #endregion


        #region Unit Tests for LocationsBySiteId

        [Fact]
        public async Task LocationsBySiteId_ReturnsJsonResult()
        {
            // Arrange
            int siteId = 1;
            var expectedLocations = LocationFixtures.GetTestLocations();

            _ = _locationServiceMock.Setup(static mock => mock.GetListAsync(
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Expression<Func<Location, string>>[]>(),
                It.IsAny<Expression<Func<Location, object>>[]>()))
                .ReturnsAsync(expectedLocations);

            // Act
            var result = await _controller.LocationsBySiteId(siteId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var actualLocations = jsonResult.Value as List<Location>;
            Assert.Equal(expectedLocations.Count, actualLocations.Count);
            for (int i = 0; i < expectedLocations.Count; i++)
            {
                Assert.Equal(expectedLocations[i].LocationId, actualLocations[i].LocationId);
                Assert.Equal(expectedLocations[i].LocationName, actualLocations[i].LocationName);
            }
        }

        #endregion


        #region Unit Tests for DownloadExcelTemplate

        [Fact]
        public void DownloadExcelTemplate_ReturnsFileResult_WithCorrectFormat()
        {
            var result = _controller.DownloadExcelTemplate() as FileContentResult;

            Assert.NotNull(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.ContentType);
            Assert.Equal("Stocks_Template.xlsx", result.FileDownloadName);
        }

        #endregion
    }
}