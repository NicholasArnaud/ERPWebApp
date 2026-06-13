using ERPWebApp.Controllers;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Config;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Inventory.SkuProperties;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Models.NirfForms;
using ERPWebApp.Models.PurchaseOrders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;

namespace ERPWebApp.UnitTests.Controllers
{

    [Trait("Category", "execute")]
    public class NirfFormsControllerTests
    {
        private readonly Mock<INirfProductMappingService> _nirfProductMappingServiceMock = new();
        private readonly Mock<IDepartmentService> _departmentServiceMock = new();
        private readonly Mock<IUserService> _userServiceMock = new();

        private readonly Mock<ISkuCategoryService> _skuCategoryServiceMock = new();
        private readonly Mock<ISkuColorService> _skuColorServiceMock = new();
        private readonly Mock<ISkuUnitOfMeasureService> _skuUnitOfMeasureServiceMock = new();
        private readonly Mock<IFontService> _fontServiceMock = new();
        private readonly Mock<INirfFormService> _nirfFormServiceMock = new();
        private readonly Mock<INirfForecastingService> _nirfForecastingServiceMock = new();
        private readonly Mock<INirfInventoryService> _nirfInventoryServiceMock = new();
        private readonly Mock<INirfPackagingService> _nirfPackagingServiceMock = new();
        private readonly Mock<INirfShippingService> _nirfShippingServiceMock = new();
        private readonly Mock<INirfParametersService> _nirfParametersServiceMock = new();
        private readonly Mock<INirfVendorMappingService> _nirfVendorMappingServiceMock = new();
        private readonly Mock<INirfImageMappingService> _nirfImageMappingServiceMock = new();
        private readonly Mock<IShippingProviderService> _shippingProviderServiceMock = new();
        private readonly Mock<ISiteService> _siteServiceMock = new();
        private readonly Mock<IProductService> _productServiceMock = new();
        private readonly Mock<IVendorService> _vendorServiceMock = new();
        private readonly Mock<IFilesService> _filesServiceMock = new();
        private readonly Mock<IProductVendorMappingService> _productVendorMappingServiceMock = new();
        private readonly Mock<IStocksService> _stocksServiceMock = new();
        private readonly Mock<ILocationService> _locationServiceMock = new();
        private readonly Mock<IProductFilesMappingsService> _productFilesMappingsServiceMock = new();
        private readonly Mock<IProductContainerService> _productContainerServiceMock = new();
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock = new(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
        private readonly NirfFormsController _controller;
        private readonly Mock<IHttpContextAccessor> _mockHttp;
        private readonly Mock<IGraphAPIService> _graphAPIServiceMock = new();
        private readonly ITempDataDictionary tempData;
        private readonly ClaimsPrincipal claimsPrincipal;
        private readonly Mock<IOptions<ExternalEndpoints>> _endpointsMock = new();
        public NirfFormsControllerTests()
        {
            _ = _endpointsMock.Setup(static e => e.Value).Returns(new ExternalEndpoints
            {
                AppDomain = "https://example.com/api",
                Helpdesk = "https://example.com/api",
                AppConfig = "https://example.com/api",

            });
            _mockHttp = new Mock<IHttpContextAccessor>();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "TestUser"),
                new(ClaimTypes.Role, RoleList.BasicUser),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            claimsPrincipal = new ClaimsPrincipal(identity);
            var userManagerMock = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            ITempDataProvider tempDataProvider = Mock.Of<ITempDataProvider>();
            TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
            tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());
            _controller = new NirfFormsController(
                _userManagerMock.Object,
                _graphAPIServiceMock.Object,
                _nirfProductMappingServiceMock.Object,
                _departmentServiceMock.Object,
                _userServiceMock.Object,
                _skuCategoryServiceMock.Object,
                _skuColorServiceMock.Object,
                _skuUnitOfMeasureServiceMock.Object,
                _fontServiceMock.Object,
                _nirfFormServiceMock.Object,
                _nirfForecastingServiceMock.Object,
                _nirfInventoryServiceMock.Object,
                _nirfPackagingServiceMock.Object,
                _nirfShippingServiceMock.Object,
                _nirfParametersServiceMock.Object,
                _nirfVendorMappingServiceMock.Object,
                _nirfImageMappingServiceMock.Object,
                _shippingProviderServiceMock.Object,
                _siteServiceMock.Object,
                _productServiceMock.Object,
                _vendorServiceMock.Object,
                _filesServiceMock.Object,
                _productVendorMappingServiceMock.Object,
                _stocksServiceMock.Object,
                _locationServiceMock.Object,
                _productFilesMappingsServiceMock.Object,
                _productContainerServiceMock.Object,
                _endpointsMock.Object
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
        public void Index_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var expectedModel = new List<NirfProductMapping>
            {
                new() { NirfFormId = 1, ProductId = 1 },
                new() { NirfFormId = 2, ProductId = 2 }
            };
            _ = _nirfProductMappingServiceMock.Setup(static s => s.QueryFilter(It.IsAny<Func<IQueryable<NirfProductMapping>, IQueryable<NirfProductMapping>>>()
                )
            ).Returns(expectedModel.AsQueryable());

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<NirfProductMapping>>(viewResult.ViewData.Model);
            Assert.Equal(expectedModel.Count, model.Count);
            Assert.Equal(expectedModel[0].NirfFormId, model[0].NirfFormId);
            Assert.Equal(expectedModel[0].ProductId, model[0].ProductId);
            Assert.Equal(expectedModel[1].NirfFormId, model[1].NirfFormId);
            Assert.Equal(expectedModel[1].ProductId, model[1].ProductId);
        }

        [Fact]
        public void Index_ReturnsViewWithNullReferenceException()
        {
            // Arrange
            _ = _nirfProductMappingServiceMock.Setup(s => s.QueryFilter(
                It.IsAny<Func<IQueryable<NirfProductMapping>, IQueryable<NirfProductMapping>>>()
            )).Throws<NullReferenceException>();

            // Assert
            _ = Assert.Throws<NullReferenceException>(() => _controller.Index());
        }

        [Fact]
        public async Task Create_SetsViewDataCorrectly()
        {
            // Arrange
            var departments = new List<Department>
            {
                new() { DepartmentId = 1, DepartmentName = "Department 1" },
                new() { DepartmentId = 2, DepartmentName = "Department 2" }
            };
            _ = _departmentServiceMock.Setup(static s => s.GetAll(
                It.IsAny<Expression<Func<Department, string>>[]>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
            )).Returns(departments);

            var users = new List<UserRoleModel>
            {
                new() {
                    UserName = "john_doe",
                    Id = "123",
                    RoleName = "Administrator"
                },
                new() {
                    UserName = "jane_doe",
                    Id = "456",
                    RoleName = "ShippingManager"
                },
                new() {
                    UserName = "jim_smith",
                    Id = "789",
                    RoleName = "ProductionManager"
                }
            };
            _ = _userServiceMock.Setup(static s => s.GetUsersInRole())
                .ReturnsAsync(users);


            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            _ = Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["DepartmentList"]);
            _ = Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["InventoryList"]);
            _ = Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["ParameterList"]);
            _ = Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["PackagingList"]);
            _ = Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["ForecastingList"]);
            _ = Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["ShippingList"]);
            _ = Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["VendorList"]);
            _departmentServiceMock.Verify(static s => s.GetAll(
                It.IsAny<Expression<Func<Department, string>>[]>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
            ), Times.Once);
            _userServiceMock.Verify(static s => s.GetUsersInRole(), Times.Once);
        }

        [Fact]
        public async Task GetSkuCategories_ReturnsOnlyActiveCategories()
        {
            // Arrange
            var expectedCategories = new List<SkuCategory>
            {
                new() { SkuCategoryId = 1, Category = "Category 1", IsActive = true },
                new() { SkuCategoryId = 2, Category = "Category 2", IsActive = true },
            };

            _ = _skuCategoryServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Expression<Func<SkuCategory, bool>>>(),
                It.IsAny<Expression<Func<SkuCategory, string>>[]>(),
                It.IsAny<Expression<Func<SkuCategory, object>>[]>()
            )).ReturnsAsync(expectedCategories);

            // Act
            var result = await _controller.GetSkuCategories();

            // Assert
            var model = Assert.IsAssignableFrom<List<SkuCategory>>(result);
            Assert.Equal(expectedCategories, model);
        }

        [Fact]
        public async Task GetSkuColors_ReturnsOnlyActiveColors()
        {
            // Arrange
            var Colors = new List<SkuColor>
            {
                new() { SkuColorId = 1, Color = "Red", IsActive = true },
                new() { SkuColorId = 2, Color = "Blue", IsActive = false },
                new() { SkuColorId = 3, Color = "Green", IsActive = true },
            };
            _ = _skuColorServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Expression<Func<SkuColor, bool>>>(),
                It.IsAny<Expression<Func<SkuColor, string>>[]>(),
                It.IsAny<Expression<Func<SkuColor, object>>[]>()
            )).ReturnsAsync(Colors.Where(static x => x.IsActive).ToList());

            // Act
            var result = await _controller.GetSkuColors();

            // Assert
            var model = Assert.IsAssignableFrom<List<SkuColor>>(result);
            Assert.Equal(Colors.Where(static x => x.IsActive).ToList(), model);
        }
        [Fact]
        public async Task GetSkuUnitOfMeasure_ReturnsOnlyActiveUnitOfMeasure()
        {
            // Arrange
            var UnitOfMeasures = new List<SkuUnitOfMeasure>
        {
            new() { SkuUnitOfMeasureId = 1, UnitOfMeasure = "Each", IsActive = true },
            new() { SkuUnitOfMeasureId = 2, UnitOfMeasure = "Box", IsActive = false },
            new() { SkuUnitOfMeasureId = 3, UnitOfMeasure = "Case", IsActive = true },
        };

            _ = _skuUnitOfMeasureServiceMock.Setup(static x => x.GetListAsync(
                  It.IsAny<Expression<Func<SkuUnitOfMeasure, bool>>>(),
                  It.IsAny<Expression<Func<SkuUnitOfMeasure, string>>[]>(),
                  It.IsAny<Expression<Func<SkuUnitOfMeasure, object>>[]>()
            )).ReturnsAsync(UnitOfMeasures.Where(static x => x.IsActive).ToList());

            // Act
            var result = _controller.GetSkuUnitOfMeasure();

            // Assert
            var model = Assert.IsAssignableFrom<List<SkuUnitOfMeasure>>(await result);
            Assert.Equal([.. UnitOfMeasures.Where(static x => x.IsActive)], model);
        }
        [Fact]
        public async Task Edit_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            int? id = null;
            _ = _nirfFormServiceMock.Setup(static x => x.GetAllAsync(
                It.IsAny<Expression<Func<NirfForm, string>>[]>(),
                It.IsAny<Expression<Func<NirfForm, object>>[]>()
            )).ReturnsAsync([]);

            // Act
            var result = await _controller.Edit(id);

            // Assert
            var viewResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, viewResult.StatusCode);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenNirfFormsIsNull()
        {
            // Arrange
            int? id = 1;
            _ = _nirfFormServiceMock.Setup(static x => x.GetAllAsync(
                It.IsAny<Expression<Func<NirfForm, string>>[]>(),
                It.IsAny<Expression<Func<NirfForm, object>>[]>()
            )).ReturnsAsync((List<NirfForm>?)null);

            // Act
            var result = await _controller.Edit(id);

            // Assert
            var viewResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, viewResult.StatusCode);
        }

        [Fact]
        public async Task Edit_ReturnsNotFoundResult_WhenIdsDoNotMatch()
        {
            // Arrange
            int id = 1;
            var nirfViewModel = new NirfViewModel
            {
                NirfForms = NirfFormFixtures.GetTestList().Last()
            };

            // Act
            var result = await _controller.Edit(id, nirfViewModel);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Product_Returns_RedirectToActionResult()
        {
            // Arrange
            var nirfForm = new NirfForm
            {
                IsWhiteLayer = true,
                IsColorLayer = true,
                IsSizingX = true,
                IsSizingY = true,
                IsUVPType = true,
                NirfFormId = 1
            };
            var product = new Product
            {
                ProductId = 1,
                Departments =
                [
                    new() {
                        DepartmentId = 1,
                        DepartmentName = "UVP",
                    },
                ],
            };
            var viewModel = new NirfViewModel
            {
                NirfForms = nirfForm,
                NirfProductMapping = new NirfProductMapping
                {
                    Product = product,
                },
            };

            _ = _departmentServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Expression<Func<Department, string>>[]>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
            )).ReturnsAsync(
                [
                    new() {
                        DepartmentId = 1,
                        DepartmentName = "UVP",
                    }
                ]
            );

            // Act
            var result = await _controller.Edit(1, viewModel);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Edit_WhenExceptionIsThrown_ReturnBadRequest()
        {
            // Arrange
            var nirfForm = new NirfForm
            {
                IsWhiteLayer = true,
                IsColorLayer = true,
                IsSizingX = true,
                IsSizingY = true,
                IsUVPType = true,
                NirfFormId = 1
            };
            var product = new Product
            {
                ProductId = 1,
                Departments =
                [
                    new() {
                        DepartmentId = 1,
                        DepartmentName = "UVP",
                    },
                ],
            };
            var viewModel = new NirfViewModel
            {
                NirfForms = nirfForm,
                NirfProductMapping = new NirfProductMapping
                {
                    Product = product,
                },
            };

            _ = _departmentServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Expression<Func<Department, string>>[]>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
            )).Throws<Exception>();

            // Act
            var result = await _controller.Edit(1, viewModel);

            // Assert
            _ = Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Details_Returns_View_With_Model()
        {
            // Arrange
            var expectedId = 1;
            var testDepartments = new List<Department>
            {
                new() { DepartmentId = 1, DepartmentName = "Department A" },
                new() { DepartmentId = 2, DepartmentName = "Department B" },
            };

            var expectedModel = NirfViewModelFixtures.GetTestData();

            _ = _nirfProductMappingServiceMock.Setup(static service => service.Get(
                It.IsAny<Expression<Func<NirfProductMapping, bool>>>(),
                It.IsAny<Expression<Func<NirfProductMapping, object>>[]>()))
                .Returns(NirfProductMappingFixtures.GetTestList().First());


            _ = _nirfForecastingServiceMock.Setup(static service => service.Get(
                 It.IsAny<Expression<Func<NirfForecasting, bool>>>()))
                 .Returns(NirfForecastingFixtures.GetTestList().First());

            _ = _nirfInventoryServiceMock.Setup(static service => service.Get(
                It.IsAny<Expression<Func<NirfInventory, bool>>>(),
                It.IsAny<Expression<Func<NirfInventory, object>>[]>()))
                .Returns(NirfInventoryFixtures.GetTestList().First());

            _ = _nirfPackagingServiceMock.Setup(static service => service.Get(
               It.IsAny<Expression<Func<NirfPackaging, bool>>>()))
               .Returns(NirfPackagingFixtures.GetTestList().First());

            _ = _nirfShippingServiceMock.Setup(static service => service.Get(
                It.IsAny<Expression<Func<NirfShipping, bool>>>(),
                It.IsAny<Expression<Func<NirfShipping, object>>[]>()))
                .Returns(NirfShippingFixtures.GetTestList().First());

            _ = _nirfParametersServiceMock.Setup(static service => service.Get(
               It.IsAny<Expression<Func<NirfParameters, bool>>>(),
               It.IsAny<Expression<Func<NirfParameters, object>>[]>()))
               .Returns(NirfParametersFixtures.GetTestList().First());

            _ = _nirfVendorMappingServiceMock.Setup(static service => service.Get(
               It.IsAny<Expression<Func<NirfVendorMapping, bool>>>(),
               It.IsAny<Expression<Func<NirfVendorMapping, object>>[]>()))
               .Returns(NirfVendorMappingFixtures.GetTestList().First());

            _ = _nirfImageMappingServiceMock.Setup(static service => service.GetList(
               It.IsAny<Expression<Func<NirfImageMapping, bool>>>(), null,
               It.IsAny<Expression<Func<NirfImageMapping, object>>[]>()))
               .Returns(NirfImageMappingFixtures.GetTestList());

            _ = _shippingProviderServiceMock.Setup(static service => service.GetList(
                It.IsAny<Expression<Func<ShippingProvider, bool>>>(),
                It.IsAny<Expression<Func<ShippingProvider, string>>[]>(),
                It.IsAny<Expression<Func<ShippingProvider, object>>[]>()
            )).Returns(ShippingProviderFixtures.GetTestList());

            _ = _departmentServiceMock.Setup(static service => service.GetListAsync(
                It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Expression<Func<Department, string>>[]>(),
                It.IsAny<Expression<Func<Department, object>>[]>()
            )).ReturnsAsync(testDepartments);

            _ = _nirfFormServiceMock.Setup(static service => service.GetList(
               It.IsAny<Expression<Func<NirfForm, bool>>>(),
               It.IsAny<Expression<Func<NirfForm, string>>[]>(),
               It.IsAny<Expression<Func<NirfForm, object>>[]>()))
               .Returns(NirfFormFixtures.GetTestList());

            _ = _vendorServiceMock.Setup(static service => service.GetAll(
                It.IsAny<Expression<Func<Vendor, string>>[]>(),
                It.IsAny<Expression<Func<Vendor, object>>[]>()
            )).Returns(VendorFixtures.GetTestList());

            // Act
            var result =await _controller.Details(expectedId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            _ = viewResult.Model.Should().NotBeNull();
        }
        [Fact]
        public void EmailNirfCreator_WithValidData_ShouldSendEmail()
        {
            // Arrange
            var nirfFormId = 1;
            _ = _nirfProductMappingServiceMock.Setup(
                static x => x.Get(
                    It.IsAny<Expression<Func<NirfProductMapping, bool>>>(),
                    It.IsAny<Expression<Func<NirfProductMapping, object>>[]>()
                )
            )
            .Returns(
                new NirfProductMapping
                {
                    NirfForm = new NirfForm { NirfFormId = nirfFormId, CreatedBy = "testuser" },
                    Product = new Product { Sku = "testsku" }
                }
            );

            var data = NirfFormFixtures.GetTestList().AsQueryable();
            _ = _nirfFormServiceMock.Setup(static x => x.GetAllNirfFormIdById(It.IsAny<int>()))
            .Returns(data);

            _ = _userServiceMock.Setup(static x => x.Get(It.IsAny<Expression<Func<IdentityUser, bool>>>()))
            .Returns(new IdentityUser { UserName = "testuser", Email = "testuser@test.com" });

            _ = _graphAPIServiceMock.Setup(
                static x => x.SendEmailAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>())
            ).Returns(Task.CompletedTask);

            // Act
            _ = _controller.EmailNirfCreator(nirfFormId, "Test Section");

            // Assert  
            _graphAPIServiceMock.Verify(static x => x.SendEmailAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
        }

        [Fact]
        public async Task Create_InvalidNirfViewModel_ReturnsViewResult()
        {
            // Arrange
            _controller.ModelState.AddModelError("key", "error message");

            // Act
            var result = await _controller.Create(nirfForm: null, imagesInput: null, filterText: null, InventoryList: null, ParameterList: null, PackagingList: null, ForecastingList: null, ShippingList: null, VendorList: null);

            // Assert
            _ = _controller.TempData["CreateError"].Should().Be("Nirf Form not Found");

        }

        [Fact]
        public async Task LocationBySiteId_ReturnsListOfLocations_WhenSiteIdIsValid()
        {
            // Arrange
            var siteId = 1;
            var expectedLocationsList = LocationFixtures.GetTestLocations();

            _ = _locationServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Expression<Func<Location, string>>[]>(),
                It.IsAny<Expression<Func<Location, object>>[]>()))
                .ReturnsAsync(expectedLocationsList);

            // Act
            var result = await _controller.LocationBySiteId(siteId);

            // Assert
            var okObjectResult = Assert.IsType<JsonResult>(result);
            var locationsList = Assert.IsAssignableFrom<IEnumerable<Location>>(okObjectResult.Value);
            Assert.Equal(expectedLocationsList.Count, locationsList.Count());
            Assert.Equal(expectedLocationsList.Select(static x => x.LocationId), locationsList.Select(static x => x.LocationId));
            Assert.Equal(expectedLocationsList.Select(static x => x.SiteId), locationsList.Select(static x => x.SiteId));
            Assert.Equal(expectedLocationsList.Select(static x => x.LocationName), locationsList.Select(static x => x.LocationName));
        }

        [Fact]
        public async Task SignInventory_WithValidData_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfForm = new NirfViewModel
            {
                NirfInventories = new NirfInventory(),
            };

            _ = _nirfInventoryServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<NirfInventory, bool>>>(),
                    It.IsAny<Expression<Func<NirfInventory, object>>[]>()))
                .Returns((NirfInventory)null!);

            _ = _nirfProductMappingServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<NirfProductMapping, bool>>>(),
                    It.IsAny<Expression<Func<NirfProductMapping, object>>[]>()))
                .Returns(new NirfProductMapping { ProductId = 1 });

            _ = _stocksServiceMock
                .Setup(static x => x.GetList(
                    It.IsAny<Expression<Func<Stock, bool>>>(),
                    It.IsAny<Expression<Func<Stock, string>>[]>(),
                    It.IsAny<Expression<Func<Stock, object>>[]>()))
                .Returns(StockFixtures.GetTestStocks());

            _ = _locationServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<Location, bool>>>(),
                    It.IsAny<Expression<Func<Location, object>>[]>()))
                .Returns(new Location { LocationId = 1 });

            _ = _stocksServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<Stock>()))
                .Returns(Task.FromResult(new Stock()));

            _ = _nirfInventoryServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<NirfInventory>()))
                .ReturnsAsync(new NirfInventory());



            // Act
            var result = await _controller.SignInventory(nirfForm);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            Assert.Equal(nirfForm.NirfInventories.NirfFormId, redirectToActionResult.RouteValues["id"]);
        }

        [Fact]
        public async Task SignInventory_WhenParameterNull_ReturnsRedirectToIndexPage()
        {
            // Arrange

            // Act
            var result = await _controller.SignInventory(null);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }


        [Fact]
        public async Task SignInventory_WhenExceptionThrows_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfForm = new NirfViewModel
            {
                NirfInventories = new NirfInventory(),
            };

            _ = _nirfInventoryServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<NirfInventory, bool>>>(),
                    It.IsAny<Expression<Func<NirfInventory, object>>[]>()))
                .Returns((NirfInventory)null!);

            _ = _nirfProductMappingServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<NirfProductMapping, bool>>>(),
                    It.IsAny<Expression<Func<NirfProductMapping, object>>[]>()))
                .Returns(new NirfProductMapping { ProductId = 1 });

            _ = _stocksServiceMock
                .Setup(static x => x.GetList(
                    It.IsAny<Expression<Func<Stock, bool>>>(),
                    It.IsAny<Expression<Func<Stock, string>>[]>(),
                    It.IsAny<Expression<Func<Stock, object>>[]>()))
                .Returns(StockFixtures.GetTestStocks());

            _ = _locationServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<Location, bool>>>(),
                    It.IsAny<Expression<Func<Location, object>>[]>()))
                .Returns(new Location { LocationId = 1 });

            _ = _stocksServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<Stock>()))
                .Throws<Exception>();

            _ = _nirfInventoryServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<NirfInventory>()))
                .Throws<Exception>();



            // Act
            var result = await _controller.SignInventory(nirfForm);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            _ = _controller.TempData["CreateError"].Should().Be("Edit Could Not Save");
        }

        [Fact]
        public async Task SignParameters_WithValidData_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfForm = new NirfViewModel
            {
                NirfParameters = NirfParametersFixtures.GetTestList().First(),
            };

            var fonts = new Fonts
            {
                FontId = 1,
                FontTitle = "test"
            };

            _ = _nirfParametersServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<NirfParameters, bool>>>(),
                    It.IsAny<Expression<Func<NirfParameters, object>>[]>()))
                .Returns((NirfParameters)null!);

            _ = _nirfParametersServiceMock
                .Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            _ = _fontServiceMock.Setup(static x => x.Get(
                    It.IsAny<Expression<Func<Fonts, bool>>>(),
                     It.IsAny<Expression<Func<Fonts, object>>[]>()
            )).Returns(fonts);

            _ = _nirfParametersServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<NirfParameters>()))
                .ReturnsAsync(new NirfParameters());

            // Act
            var result = await _controller.SignParameters(nirfForm, "1", 1, 1, 1, "1");

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            Assert.Equal(nirfForm.NirfParameters.NirfFormId, redirectToActionResult.RouteValues["id"]);
        }

        [Fact]
        public async Task SignParameters_WhenParameterNull_ReturnsRedirectToIndexPage()
        {
            // Arrange

            // Act
            var result = await _controller.SignParameters(null, "1", 1, 1, 1, "1");

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task SignParameters_WhenExceptionThrows_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfForm = new NirfViewModel
            {
                NirfParameters = NirfParametersFixtures.GetTestList().First(),
            };

            var fonts = new Fonts
            {
                FontId = 1,
                FontTitle = "test"
            };

            _ = _nirfParametersServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<NirfParameters, bool>>>(),
                    It.IsAny<Expression<Func<NirfParameters, object>>[]>()))
                .Returns((NirfParameters?)null!);

            _ = _nirfParametersServiceMock
                .Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            _ = _fontServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<Fonts, bool>>>(),
                    It.IsAny<Expression<Func<Fonts, object>>[]>()
                )).Returns(fonts);

            _ = _nirfParametersServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<NirfParameters>()))
                .Throws<Exception>();

            // Act
            var result = await _controller.SignParameters(nirfForm, "1", 1, 1, 1, "1");

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            _ = _controller.TempData["CreateError"].Should().Be("Parameters Could Not Save");
        }


        [Fact]
        public async Task SignPackaging_WithValidData_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfForm = new NirfViewModel
            {
                NirfPackagings = NirfPackagingFixtures.GetTestList().First(),
            };

            _ = _nirfPackagingServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<NirfPackaging, bool>>>()))
                .Returns(new NirfPackaging());

            _ = _nirfPackagingServiceMock
                .Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            _ = _nirfPackagingServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<NirfPackaging>()))
                .ReturnsAsync(new NirfPackaging());

            // Act
            var result = await _controller.SignPackaging(nirfForm);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            Assert.Equal(nirfForm.NirfPackagings.NirfFormId, redirectToActionResult.RouteValues["id"]);
        }

        [Fact]
        public async Task SignPackaging_WhenParameterNull_ReturnsRedirectToIndexPage()
        {
            // Arrange

            // Act
            var result = await _controller.SignPackaging(null);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task SignPackaging_WhenExceptionThrows_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfForm = new NirfViewModel
            {
                NirfPackagings = NirfPackagingFixtures.GetTestList().First(),
            };

            _ = _nirfPackagingServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<NirfPackaging, bool>>>()))
                .Returns(new NirfPackaging());

            _ = _nirfPackagingServiceMock
                .Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            _ = _nirfPackagingServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<NirfPackaging>()))
                .Throws<Exception>();

            // Act
            var result = await _controller.SignPackaging(nirfForm);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            _ = _controller.TempData["CreateError"].Should().Be("Nirf Packaging Could Not Save");
        }

        [Fact]
        public async Task SignForecasting_WithValidData_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfForm = new NirfViewModel
            {
                NirfForecastings = NirfForecastingFixtures.GetTestList().First(),
            };

            _ = _nirfForecastingServiceMock
                .Setup(static x => x.Get(It.IsAny<Expression<Func<NirfForecasting, bool>>>()))
                .Returns(new NirfForecasting());

            _ = _nirfForecastingServiceMock
                .Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            _ = _nirfForecastingServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<NirfForecasting>()))
                .ReturnsAsync(new NirfForecasting());

            // Act
            var result = await _controller.SignForecasting(nirfForm);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            Assert.Equal(nirfForm.NirfForecastings.NirfFormId, redirectToActionResult.RouteValues["id"]);
        }

        [Fact]
        public async Task SignForecasting_WhenParameterNull_ReturnsRedirectToIndexPage()
        {
            // Arrange

            // Act
            var result = await _controller.SignForecasting(null);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task SignForecasting_WhenExceptionThrows_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfForm = new NirfViewModel
            {
                NirfForecastings = NirfForecastingFixtures.GetTestList().First(),
            };

            _ = _nirfForecastingServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<NirfForecasting, bool>>>()))
                .Returns(new NirfForecasting());

            _ = _nirfForecastingServiceMock
                .Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            _ = _nirfForecastingServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<NirfForecasting>()))
                .Throws<Exception>();

            // Act
            var result = await _controller.SignForecasting(nirfForm);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            _ = _controller.TempData["CreateError"].Should().Be("Nirf Forecasting Could Not Save");
        }

        [Fact]
        public async Task SignShipping_WithValidData_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfForm = NirfViewModelFixtures.GetTestData();

            _ = _nirfShippingServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<NirfShipping, bool>>>(),
                    It.IsAny<Expression<Func<NirfShipping, object>>[]>()))
                .Returns(new NirfShipping());

            _ = _nirfShippingServiceMock
                .Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            _ = _nirfShippingServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<NirfShipping>()))
                .ReturnsAsync(new NirfShipping());

            // Act
            var result = await _controller.SignShipping(nirfForm);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            Assert.Equal(nirfForm.NirfForecastings.NirfFormId, redirectToActionResult.RouteValues["id"]);
        }

        [Fact]
        public async Task SignShipping_WhenParameterNull_ReturnsRedirectToIndexPage()
        {
            // Arrange

            // Act
            var result = await _controller.SignShipping(null);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task SignShipping_WhenExceptionThrows_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfForm = NirfViewModelFixtures.GetTestData();
            nirfForm.NirfShippingProvider = null;

            _ = _nirfShippingServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<NirfShipping, bool>>>(),
                    It.IsAny<Expression<Func<NirfShipping, object>>[]>()))
                .Returns(new NirfShipping());

            _ = _nirfShippingServiceMock
                .Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            _ = _nirfShippingServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<NirfShipping>()))
                .Throws<Exception>();

            // Act
            var result = await _controller.SignShipping(nirfForm);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            _ = _controller.TempData["CreateError"].Should().Be("Nirf Shipping Could Not Save");
        }

        [Fact]
        public async Task SignShipping_WhenNirfShippingProviderCountZero_ReturnsRedirectToEditPage()
        {
            // Arrange
            var nirfForm = new NirfViewModel
            {
                NirfShippings = NirfShippingFixtures.GetTestList().First(),
                NirfShippingProvider = NirfShippingProdivderFixtures.GetTestList()
            };
            // Act
            var result = await _controller.SignShipping(nirfForm);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            _ = _controller.TempData["Error"].Should().Be("No Shipping Provider Selected");
        }

        [Fact(Skip = "pending")]
        public async Task SignVendorMapping_WithValidData_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfForm = new NirfViewModel
            {
                NirfVendorMapping = NirfVendorMappingFixtures.GetTestList().First(),
            };

            var _nirfFormsController = new Mock<NirfFormsController>(
                _userManagerMock.Object,
                _graphAPIServiceMock.Object,
                _nirfProductMappingServiceMock.Object,
                _departmentServiceMock.Object,
                _userServiceMock.Object,
                _skuCategoryServiceMock.Object,
                _skuColorServiceMock.Object,
                _skuUnitOfMeasureServiceMock.Object,
                _fontServiceMock.Object,
                _nirfFormServiceMock.Object,
                _nirfForecastingServiceMock.Object,
                _nirfInventoryServiceMock.Object,
                _nirfPackagingServiceMock.Object,
                _nirfShippingServiceMock.Object,
                _nirfParametersServiceMock.Object,
                _nirfVendorMappingServiceMock.Object,
                _nirfImageMappingServiceMock.Object,
                _shippingProviderServiceMock.Object,
                _siteServiceMock.Object,
                _productServiceMock.Object,
                _vendorServiceMock.Object,
                _filesServiceMock.Object,
                _productVendorMappingServiceMock.Object,
                _stocksServiceMock.Object,
                _locationServiceMock.Object,
                _productFilesMappingsServiceMock.Object,
                _productContainerServiceMock.Object

            );
            _nirfFormsController.Object.ControllerContext = new ControllerContext();
            _nirfFormsController.Object.ControllerContext.HttpContext = new DefaultHttpContext();
            _nirfFormsController.Object.ControllerContext.HttpContext.User = claimsPrincipal;
            _nirfFormsController.Object.TempData = tempData;

            var _nirfDbFull = NirfViewModelFixtures.GetTestData();
            var privateField = typeof(NirfFormsController).GetField("_nirfDbFull", BindingFlags.NonPublic | BindingFlags.Instance);
            if (privateField != null)
            {
                privateField.SetValue(_nirfFormsController.Object, _nirfDbFull);
            }
            else
            {
                throw new InvalidOperationException("The private field '_nirfDbFull' was not found.");
            }

            _ = _productServiceMock
              .Setup(static x => x.Get(
                  It.IsAny<Expression<Func<Product, bool>>>(),
                  It.IsAny<Expression<Func<Product, object>>[]>()))
              .Returns(ProductFixtures.GetTestProducts().First());

            _ = _nirfVendorMappingServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<NirfVendorMapping, bool>>>(),
                    It.IsAny<Expression<Func<NirfVendorMapping, object>>[]>()))
                .Returns(new NirfVendorMapping());

            _ = _nirfVendorMappingServiceMock
                .Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            _ = _nirfVendorMappingServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<NirfVendorMapping>()))
                .ReturnsAsync(new NirfVendorMapping());

            _ = _productVendorMappingServiceMock
               .Setup(static x => x.Get(
                   It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
                   It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
               .Returns(new ProductVendorMapping());

            _ = _productVendorMappingServiceMock
                .Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            _ = _productVendorMappingServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<ProductVendorMapping>()))
                .ReturnsAsync(new ProductVendorMapping());
            NirfFormsController controller = _nirfFormsController.Object;
            // Act
            var result = await controller.SignVendorMapping(nirfForm);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            Assert.Equal(nirfForm.NirfForecastings.NirfFormId, redirectToActionResult.RouteValues["id"]);
        }

        [Fact]
        public async Task SignVendorMapping_WhenParameterNull_ReturnsRedirectToIndexPage()
        {
            // Arrange

            // Act
            var result = await _controller.SignVendorMapping(null);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task SignVendorMapping_WhenExceptionThrows_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfForm = new NirfViewModel
            {
                NirfVendorMapping = NirfVendorMappingFixtures.GetTestList().First(),
            };

            _ = _productServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<Expression<Func<Product, object>>[]>()))
                .Returns(ProductFixtures.GetTestProducts().First());

            _ = _nirfVendorMappingServiceMock
                .Setup(static x => x.Get(
                    It.IsAny<Expression<Func<NirfVendorMapping, bool>>>(),
                    It.IsAny<Expression<Func<NirfVendorMapping, object>>[]>()))
                .Returns(new NirfVendorMapping());

            _ = _nirfVendorMappingServiceMock
                .Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            _ = _nirfVendorMappingServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<NirfVendorMapping>()))
                .Throws<Exception>();

            _ = _productVendorMappingServiceMock
               .Setup(static x => x.Get(
                   It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
                   It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
               .Throws<Exception>();

            _ = _productVendorMappingServiceMock
                .Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                .Throws<Exception>();

            _ = _productVendorMappingServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<ProductVendorMapping>()))
                .Throws<Exception>();

            // Act
            var result = await _controller.SignVendorMapping(nirfForm);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            _ = _controller.TempData["CreateError"].Should().Be("Nirf Vendor Mapping Could Not Save");
        }

        [Fact(Skip = "pending")]
        public async Task AddNirfProductMapping_ValidInput_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfForm = NirfViewModelFixtures.GetTestData();

            _ = _nirfVendorMappingServiceMock
                .Setup(static x => x.IsExists(It.IsAny<Expression<Func<NirfVendorMapping, bool>>>()))
                .Returns(true);

            _ = _productServiceMock
                .Setup(static x => x.IsExists(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(false);

            _ = _productServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<Product>()))
                .ReturnsAsync(new Product { ProductId = 1 });

            _ = _productServiceMock
                .Setup(static x => x.Get(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>()))
                .Returns(new Product { ProductId = 1 });

            _ = _nirfProductMappingServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<NirfProductMapping>()))
                .ReturnsAsync(new NirfProductMapping { NirfProductMappingId = 1 });

            _ = _nirfVendorMappingServiceMock
                .Setup(static x => x.Get(It.IsAny<Expression<Func<NirfVendorMapping, bool>>>(), It.IsAny<Expression<Func<NirfVendorMapping, object>>[]>()))
                .Returns(new NirfVendorMapping { VendorId = 1 });

            _ = _productVendorMappingServiceMock
                .Setup(static x => x.AddAsync(It.IsAny<ProductVendorMapping>()))
                .ReturnsAsync(new ProductVendorMapping());

            // Act
            var result = await _controller.AddNirfProductMapping(nirfForm);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectToActionResult.ActionName);
            Assert.Equal(1, redirectToActionResult.RouteValues["id"]);
        }

        [Fact]
        public async Task AddNirfProductMapping_ReturnsRedirectToAction_WhenNirfFormIsNull()
        {
            // Arrange
            NirfViewModel? nirfForm = null;

            // Act
            var result = await _controller.AddNirfProductMapping(nirfForm);

            // Assert
            _ = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult)?.ActionName ?? string.Empty);
        }

        [Fact]
        public async Task CancelForm_ValidFormId_ReturnsRedirectToActionResult()
        {
            // Arrange
            int formId = 1;

            _ = _nirfFormServiceMock.Setup(static x => x.Get(
                It.IsAny<Expression<Func<NirfForm, bool>>>(),
                It.IsAny<Expression<Func<NirfForm, object>>[]>()
            )).Returns(NirfFormFixtures.GetTestList().FirstOrDefault(static x => x.NirfStatus != NirfForm.Status.Completed) ?? new NirfForm());

            _ = _nirfFormServiceMock.Setup(static x => x.UpdateAsync(It.IsAny<NirfForm>()))
                                             .ReturnsAsync(1);

            _ = _nirfProductMappingServiceMock.Setup(static x => x.GetList(
                                                             It.IsAny<Expression<Func<NirfProductMapping, bool>>>(),
                                                             It.IsAny<Expression<Func<NirfProductMapping, string>>[]>(),
                                                             It.IsAny<Expression<Func<NirfProductMapping, object>>[]>()))
                                              .Returns(NirfProductMappingFixtures.GetTestList());

            _ = _productServiceMock.Setup(static x => x.GetList(
                                                    It.IsAny<Expression<Func<Product, bool>>>(),
                                                    It.IsAny<Expression<Func<Product, string>>[]>(),
                                                    It.IsAny<Expression<Func<Product, object>>[]>()))
                                             .Returns(ProductFixtures.GetTestProducts());

            _ = _productVendorMappingServiceMock.Setup(static x => x.Get(
                                                   It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
                                                   It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
                                            .Returns(ProductVendorMappingFixtures.GetTestList().First());

            _ = _productVendorMappingServiceMock.Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                                            .ReturnsAsync(1);

            _ = _stocksServiceMock.Setup(static x => x.GetList(
                                                    It.IsAny<Expression<Func<Stock, bool>>>(),
                                                    It.IsAny<Expression<Func<Stock, string>>[]>(),
                                                    It.IsAny<Expression<Func<Stock, object>>[]>()))
                                             .Returns(StockFixtures.GetTestStocks());

            _ = _stocksServiceMock.Setup(static x => x.RemoveAsync(It.IsAny<int>()))
                                            .Returns(Task.FromResult(1));

            _ = _stocksServiceMock.Setup(static x => x.UpdateAsync(It.IsAny<Stock>()))
                                            .ReturnsAsync(1);

            _ = _productServiceMock.Setup(static x => x.UpdateAsync(It.IsAny<Product>()))
                                             .ReturnsAsync(1);

            // Act
            var result = await _controller.CancelForm(formId);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task CancelForm_Should_Return_Error_When_Form_Is_Null()
        {
            // Arrange
            int? id = null;
            _ = _nirfFormServiceMock.Setup(static x => x.Get(
                It.IsAny<Expression<Func<NirfForm, bool>>>(),
                It.IsAny<Expression<Func<NirfForm, object>>[]>()
            )).Returns((NirfForm)null!);

            // Act
            var result = await _controller.CancelForm(id);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", viewResult.ActionName);
            Assert.Null(viewResult.RouteValues?["id"]);
            Assert.Equal("Nirf Form is cant be cancelled", _controller.TempData["Error"]);
        }

        [Fact]
        public async Task CancelForm_Should_Return_Error_When_Form_Status_Is_Completed()
        {
            // Arrange
            int? id = 1;
            _ = _nirfFormServiceMock.Setup(static x => x.Get(
                It.IsAny<Expression<Func<NirfForm, bool>>>(),
                It.IsAny<Expression<Func<NirfForm, object>>[]>()
            )).Returns(new NirfForm { NirfFormId = id.Value, NirfStatus = NirfForm.Status.Completed });

            // Act
            var result = await _controller.CancelForm(id);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", viewResult.ActionName);
            Assert.NotNull(viewResult.RouteValues);
            Assert.Equal(id, viewResult.RouteValues["id"]);
            Assert.Equal("Nirf Form is cant be cancelled", _controller.TempData["Error"]);
        }

        [Fact]
        public async Task DeleteImage_ShouldDeleteImageAndMapping_AndRedirectToEdit()
        {
            // Arrange
            int fileId = 1;
            int nirfFormId = 2;
            var mapping = NirfImageMappingFixtures.GetTestList().First();
            var file = FilesFixtures.GetTestFiles().First();

            _ = _nirfImageMappingServiceMock.Setup(x => x.Get(
                                It.IsAny<Expression<Func<NirfImageMapping, bool>>>(),
                                It.IsAny<Expression<Func<NirfImageMapping, object>>[]>()))
                .Returns(mapping);

            _ = _filesServiceMock.Setup(x => x.Get(It.IsAny<Expression<Func<Files, bool>>>(), It.IsAny<Expression<Func<Files, object>>[]>()))
                .Returns(file);

            // Act
            var result = await _controller.DeleteImage(fileId, nirfFormId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Edit", result.ActionName);
            Assert.NotNull(result.RouteValues);
            Assert.Equal(nirfFormId, result.RouteValues["id"]);
            _nirfImageMappingServiceMock.Verify(x => x.RemoveAsync(mapping.NirfImageMappingId), Times.Once);
            _filesServiceMock.Verify(x => x.RemoveAsync(file.FileId), Times.Once);
        }

        [Fact]
        public async Task DeleteImage_ShouldNotDeleteImageOrMapping_AndRedirectToEdit_WhenMappingAndFileAreNull()
        {
            // Arrange
            int fileId = 1;
            int nirfFormId = 2;
            _ = _nirfImageMappingServiceMock.Setup(static x => x.Get(
                                It.IsAny<Expression<Func<NirfImageMapping, bool>>>(),
                                It.IsAny<Expression<Func<NirfImageMapping, object>>[]>()))
                .Returns((NirfImageMapping)null!);

            _ = _filesServiceMock.Setup(static x => x.Get(It.IsAny<Expression<Func<Files, bool>>>(), It.IsAny<Expression<Func<Files, object>>[]>()))
                .Returns((Files)null!);

            // Act
            var result = await _controller.DeleteImage(fileId, nirfFormId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Edit", result.ActionName);
            Assert.Equal(nirfFormId, result.RouteValues["id"]);
            _nirfImageMappingServiceMock.Verify(static x => x.RemoveAsync(It.IsAny<int>()), Times.Never);
            _filesServiceMock.Verify(static x => x.Remove(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task UploadImages_ValidData_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nirfFormId = 1;
            var formFileMock = new Mock<IFormFile>();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("This is a test file."));
            _ = formFileMock.Setup(static x => x.OpenReadStream()).Returns(stream);
            _ = formFileMock.Setup(static x => x.FileName).Returns("test.txt");
            _ = formFileMock.Setup(static x => x.ContentType).Returns("text/plain");
            var newImages = new IFormFile[] { formFileMock.Object };
            var files = FilesFixtures.GetTestFiles();
            var newMapping = NirfImageMappingFixtures.GetTestList().First();


            _ = _filesServiceMock.Setup(static x => x.AddAsync(It.IsAny<Files>())).Callback<Files>(static file => file.FileId = 1);

            _ = _nirfImageMappingServiceMock.Setup(static x => x.AddAsync(It.IsAny<NirfImageMapping>())).Callback<NirfImageMapping>(static mapping => mapping.NirfImageMappingId = 1);


            // Act
            var result = await _controller.UploadImages(nirfFormId, newImages);

            // Assert
            _ = Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;
            Assert.Equal("Edit", redirectResult.ActionName);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.Equal(nirfFormId, redirectResult.RouteValues["id"]);

            _filesServiceMock.Verify(static x => x.AddAsync(It.IsAny<Files>()), Times.Once);
            _filesServiceMock.Verify(static x => x.Remove(It.IsAny<int>()), Times.Never);
            _nirfImageMappingServiceMock.Verify(static x => x.AddAsync(It.IsAny<NirfImageMapping>()), Times.Once);
            _nirfImageMappingServiceMock.Verify(static x => x.RemoveAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task SetThumbnail_WithValidInput_SetsThumbnailAndRedirects()
        {
            // Arrange
            var fileId = 1;
            var nirfFormId = 2;
            var mappingRow = NirfImageMappingFixtures.GetTestList().First();

            _ = _nirfImageMappingServiceMock.Setup(x => x.Get(
                                   It.IsAny<Expression<Func<NirfImageMapping, bool>>>(),
                                   It.IsAny<Expression<Func<NirfImageMapping, object>>[]>()))
               .Returns(mappingRow);

            _ = _nirfImageMappingServiceMock.Setup(x => x.GetList(
                                    It.IsAny<Expression<Func<NirfImageMapping, bool>>>(),
                                    It.IsAny<Expression<Func<NirfImageMapping, string>>[]>(),
                                    It.IsAny<Expression<Func<NirfImageMapping, object>>[]>()))
                .Returns(NirfImageMappingFixtures.GetTestList().Where(x => x.NirfFormId == nirfFormId && x.IsThumbnail).ToList());

            _ = _nirfImageMappingServiceMock
                .Setup(x => x.UpdateAsync(It.IsAny<NirfImageMapping>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.SetThumbnail(fileId, nirfFormId);

            // Assert
            _nirfImageMappingServiceMock.Verify(
                x => x.Get(
                            It.IsAny<Expression<Func<NirfImageMapping, bool>>>(),
                            It.IsAny<Expression<Func<NirfImageMapping, object>>[]>()),
                Times.Once);
            _nirfImageMappingServiceMock.Verify(
                x => x.GetList(
                                It.IsAny<Expression<Func<NirfImageMapping, bool>>>(),
                                It.IsAny<Expression<Func<NirfImageMapping, string>>[]>(),
                                It.IsAny<Expression<Func<NirfImageMapping, object>>[]>()),
                Times.Once);
            _nirfImageMappingServiceMock.Verify(
                x => x.UpdateAsync(It.IsAny<NirfImageMapping>()),
                Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectResult.ActionName);
            Assert.Equal(nirfFormId, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task FinishForm_Should_Redirect_To_Edit_If_Departments_Not_Done()
        {
            // Arrange
            var viewModel = new NirfViewModel
            {
                NirfProductMapping = NirfProductMappingFixtures.GetTestList().First()
            };
            var form = NirfFormFixtures.GetTestList().First();


            _ = _nirfFormServiceMock.Setup(static m => m.Get(
                It.IsAny<Expression<Func<NirfForm, bool>>>(),
                 It.IsAny<Expression<Func<NirfForm, object>>[]>()
            )).Returns(form);
            _ = _nirfFormServiceMock
                .Setup(static m => m.GetAllNirfFormIdById(It.IsAny<int>()))
                .Returns(NirfFormFixtures.GetTestList().AsQueryable());

            // Act
            var result = await _controller.FinishForm(viewModel);

            // Assert
            _ = Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;
            Assert.Equal("Edit", redirectResult.ActionName);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.Equal(form.NirfFormId, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public void GetShippingProviderlist_ReturnsEmptyJsonResult_WhenIdIsNull()
        {
            // Arrange
            int? id = null;

            var nirfShippings = NirfShippingFixtures.GetTestList();

            _ = _nirfShippingServiceMock.Setup(static s => s.Get(
                It.IsAny<Expression<Func<NirfShipping, bool>>>(),
                It.IsAny<Expression<Func<NirfShipping, object>>[]>()
            )).Returns(nirfShippings.First());

            var shippingProviders = new List<ShippingProvider>()
            {
                new() { ShippingProviderId = 3, ShippingProviderName = "Provider 3" },
                new() { ShippingProviderId = 4, ShippingProviderName = "Provider 4" }
            };
            _ = _shippingProviderServiceMock.Setup(static s => s.GetList(
                It.IsAny<Expression<Func<ShippingProvider, bool>>>(),
                It.IsAny<Expression<Func<ShippingProvider, string>>[]>(),
                It.IsAny<Expression<Func<ShippingProvider, object>>[]>()
            )).Returns(shippingProviders);

            // Act
            var result = _controller.GetShippingProviderlist(id);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsType<SelectList>(jsonResult.Value);
            Assert.Equal(shippingProviders.Count(), model.Count());
        }

        [Fact]
        public void GetShippingProviderlist_ReturnsJsonResult_WhenIdIsNotNull()
        {
            // Arrange
            int id = 1;
            var nirfShippings = NirfShippingFixtures.GetTestList();

            _ = _nirfShippingServiceMock.Setup(static s => s.Get(
                It.IsAny<Expression<Func<NirfShipping, bool>>>(),
                It.IsAny<Expression<Func<NirfShipping, object>>[]>()
            )).Returns(nirfShippings.First());

            var shippingProviders = new List<ShippingProvider>()
            {
                new() { ShippingProviderId = 3, ShippingProviderName = "Provider 3" },
                new() { ShippingProviderId = 4, ShippingProviderName = "Provider 4" }
            };
            _ = _shippingProviderServiceMock.Setup(static s => s.GetList(
                It.IsAny<Expression<Func<ShippingProvider, bool>>>(),
                It.IsAny<Expression<Func<ShippingProvider, string>>[]>(),
                It.IsAny<Expression<Func<ShippingProvider, object>>[]>()
            )).Returns(shippingProviders);

            // Act
            var result = _controller.GetShippingProviderlist(id);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsType<SelectList>(jsonResult.Value);
            Assert.Equal(2, model.Count());
            Assert.Equal("3", model.First().Value);
            Assert.Equal("Provider 3", model.First().Text);
            Assert.Equal("4", model.Last().Value);
            Assert.Equal("Provider 4", model.Last().Text);
        }

        [Fact]
        public void DownloadExcel_ReturnsRedirect_WhenDataIsNull()
        {
            // Arrange
            _ = _nirfFormServiceMock.Setup(static s => s.Get(
                It.IsAny<Expression<Func<NirfForm, bool>>>(),
                It.IsAny<Expression<Func<NirfForm, object>>[]>()
             )).Returns(static () => null!);

            _ = _nirfProductMappingServiceMock.Setup(static s => s.Get(
                It.IsAny<Expression<Func<NirfProductMapping, bool>>>(),
                 It.IsAny<Expression<Func<NirfProductMapping, object>>[]>()
                )).Returns(static () => null!);

            _ = _nirfForecastingServiceMock.Setup(static s => s.Get(
                It.IsAny<Expression<Func<NirfForecasting, bool>>>()
                )).Returns(static () => null!);

            _ = _nirfInventoryServiceMock.Setup(static s => s.Get(
                It.IsAny<Expression<Func<NirfInventory, bool>>>(),
                It.IsAny<Expression<Func<NirfInventory, object>>[]>()
                )).Returns(static () => null!);


            _ = _nirfPackagingServiceMock.Setup(static s => s.Get(
                It.IsAny<Expression<Func<NirfPackaging, bool>>>()
                )).Returns(static () => null!);

            _ = _nirfShippingServiceMock.Setup(static s => s.Get(
                It.IsAny<Expression<Func<NirfShipping, bool>>>(),
                It.IsAny<Expression<Func<NirfShipping, object>>[]>()
                )).Returns(static () => null!);

            _ = _nirfParametersServiceMock.Setup(static s => s.Get(
                 It.IsAny<Expression<Func<NirfParameters, bool>>>(),
                 It.IsAny<Expression<Func<NirfParameters, object>>[]>()
                 )).Returns(static () => null!);

            _ = _nirfVendorMappingServiceMock.Setup(static s => s.Get(
                 It.IsAny<Expression<Func<NirfVendorMapping, bool>>>(),
                 It.IsAny<Expression<Func<NirfVendorMapping, object>>[]>()
                 )).Returns(static () => null!);

            _ = _nirfImageMappingServiceMock.Setup(static s => s.GetList(
                 It.IsAny<Expression<Func<NirfImageMapping, bool>>>(),
                  It.IsAny<Expression<Func<NirfImageMapping, string>>[]>(),
                 It.IsAny<Expression<Func<NirfImageMapping, object>>[]>()
                 )).Returns(static () => null!);


            // Act
            var result = _controller.DownloadExcel(1, new NirfViewModel()) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);
            Assert.NotNull(result.RouteValues);
            Assert.Equal(1, result.RouteValues["id"]);
        }

        [Fact]
        public void GetAspUser_WhenUserIsLoggedIn_ReturnsUserId()
        {
            // Arrange
            var users = new List<IdentityUser> { new() { Id = "TestUser1", UserName = "TestUser" } };
            _ = _userManagerMock.Setup(static um => um.Users).Returns(users.AsQueryable());

            // Act
            var result = _controller.GetAspUser();

            // Assert
            Assert.Equal("TestUser1", result);
        }
        [Fact]
        public void ResetViewData_SetsVendorListInViewData()
        {
            // Arrange

            _ = _vendorServiceMock.Setup(static service => service.GetAll(
               It.IsAny<Expression<Func<Vendor, string>>[]>(),
               It.IsAny<Expression<Func<Vendor, object>>[]>()
               )).Returns(VendorFixtures.GetTestList());


            // Act
            _controller.ResetViewData();

            // Assert
            Assert.True(_controller.ViewData.ContainsKey("VendorList"));
            var vendorList = _controller.ViewData["VendorList"] as SelectList;
            Assert.NotNull(vendorList);
            Assert.Equal(2, vendorList.Count());
        }

    }
}