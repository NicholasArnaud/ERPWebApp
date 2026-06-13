using ERPWebApp.Controllers.PurchaseOrders;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Models.PurchaseOrders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers.PurchaseOrders;

[Trait("Category", "execute")]
public class PurchaseOrdersControllerTests
{
    private readonly Mock<IProductPurchaseOrderService> _productPurchaseOrderServiceMock = new();
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly Mock<IPurchaseOrderService> _purchaseorderServiceMock = new();
    private readonly Mock<IPurchaseOrderFilesMappingService> _purchaseOrderFilesMappingServiceMock = new();
    private readonly Mock<IProductVendorMappingService> _productVendorMappingServiceMock = new();
    private readonly Mock<IShippingMethodService> _shippingMethodServiceMock = new();
    private readonly Mock<IShippingProviderService> _shippingProviderServiceMock = new();
    private readonly Mock<IVendorService> _vendorServiceMock = new();
    private readonly Mock<IStocksService> _stocksServiceMock = new();
    private readonly Mock<IProductPurchaseOrderStockMappingService> _productPurchaseOrderStockMappingServiceMock = new();
    private readonly Mock<ILocationService> _locationServiceMock = new();
    private readonly Mock<IMoveStockHistoryService> _moveStockHistoryServiceMock = new();
    private readonly Mock<IFilesService> _fileServiceMock = new();
    private readonly Mock<IEmployeeService> _employeeServiceMock = new();
    private readonly Mock<IOrderBatchService> _orderBatchServiceMock = new();
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock = new(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

    private readonly PurchaseOrdersController _controller;
    private readonly Mock<IHttpContextAccessor> _mockHttp;
    public PurchaseOrdersControllerTests()
    {
        _mockHttp = new Mock<IHttpContextAccessor>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.Role, RoleList.BasicUser),
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller = new PurchaseOrdersController(
            _productPurchaseOrderServiceMock.Object,
            _productServiceMock.Object,
            _purchaseorderServiceMock.Object,
            _purchaseOrderFilesMappingServiceMock.Object,
            _productVendorMappingServiceMock.Object,
            _shippingMethodServiceMock.Object,
            _shippingProviderServiceMock.Object,
            _vendorServiceMock.Object,
            _stocksServiceMock.Object,
            _productPurchaseOrderStockMappingServiceMock.Object,
            _locationServiceMock.Object,
            _moveStockHistoryServiceMock.Object,
            _fileServiceMock.Object,
            _userManagerMock.Object,
            _employeeServiceMock.Object,
            _orderBatchServiceMock.Object
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

    [Fact]
    public async Task Index_WhenSucceed_ShouldReturnViewWithPurchaseOrderIndexData()
    {

        // Arrange
        var existingIds = new List<int> { 1, 2, 3 }; // Sample existing IDs
        var productList = new List<Product>
        {
            new() { ProductId = 1, Sku = "SKU1", Description = "Description1" },
            new() { ProductId = 2, Sku = "SKU2", Description = "Description2" }
        };

        _ = _productPurchaseOrderServiceMock.Setup(static mock => mock.GetListAsync(It.IsAny<Func<IQueryable<ProductPurchaseOrder>, IQueryable<int>>>()))
            .ReturnsAsync(existingIds);

        _ = _productServiceMock.Setup(static mock => mock.GetListAsync(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()))
        .ReturnsAsync(productList);


        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var productListViewData = viewResult.ViewData["ProductList"] as SelectList;
        var statusListViewData = viewResult.ViewData["StatusList"] as SelectList;

        Assert.NotNull(productListViewData);
        Assert.NotNull(statusListViewData);



        Assert.Equal(productList.Count, productListViewData.Count());


        var enumValues = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();
        Assert.Equal(enumValues.Count, statusListViewData.Count());

    }

    [Fact]
    public async Task Create_WhenCreateScuressReturnsViewWithCorrectData()
    {
        // Arrange
        var shippingProvider = new ShippingProvider
        {
            ShippingProviderId = 1,
            ShippingProviderName = "Test Shipping Provider",
            ModifyDate = DateTime.Now,
            ModifyByUser = "Admin",
            IsActive = true
        };

        var shippingProviderList = new List<ShippingProvider> { shippingProvider };

        var shippingMethod = new ShippingMethod
        {
            ShippingMethodId = 1,
            ShippingMethodName = "Test Shipping Method",
            ModifyDate = DateTime.Now,
            ModifyByUser = "Admin",
            IsActive = true,
            ShippingProviderId = 1,
            ShippingProvider = shippingProvider
        };

        var shippingMethods = new List<ShippingMethod> { shippingMethod };

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetVendorsAsync()).ReturnsAsync([]);

        _ = _productVendorMappingServiceMock.Setup(static svc => svc.GetListAsync(
            It.IsAny<Func<IQueryable<ProductVendorMapping>, IQueryable<ProductVendorMapping>>>()
        )).ReturnsAsync([]);

        _ = _shippingMethodServiceMock.Setup(static svc => svc.IsExists(It.IsAny<Expression<Func<ShippingMethod, bool>>>()))
            .Returns(false);

        _ = _shippingProviderServiceMock.Setup(static svc => svc.Add(It.IsAny<ShippingProvider>()));
        _ = _shippingProviderServiceMock.Setup(static svc => svc.GetAsync(It.IsAny<Expression<Func<ShippingProvider, bool>>>(), It.IsAny<Expression<Func<ShippingProvider, object>>[]>()))
            .ReturnsAsync(shippingProvider);
        _ = _shippingProviderServiceMock.Setup(static svc => svc.GetListAsync(
            It.IsAny<Expression<Func<ShippingProvider, bool>>>(),
            It.IsAny<Expression<Func<ShippingProvider, string>>[]>(),
            It.IsAny<Expression<Func<ShippingProvider, object>>[]>()
        )).ReturnsAsync(shippingProviderList);

        _ = _shippingMethodServiceMock.Setup(static svc => svc.Add(It.IsAny<ShippingMethod>()));
        _ = _shippingMethodServiceMock.Setup(static svc => svc.GetListAsync(
            It.IsAny<Expression<Func<ShippingMethod, bool>>>(),
            It.IsAny<Expression<Func<ShippingMethod, string>>[]>(),
            It.IsAny<Expression<Func<ShippingMethod, object>>[]>()
        )).ReturnsAsync(shippingMethods);

        var users = new List<IdentityUser> { new() { Id = "1", NormalizedUserName = "USERNAME" } };
        var usersQueryable = users.AsQueryable();
        _ = _userManagerMock.Setup(static m => m.Users).Returns(usersQueryable);
        _ = _userManagerMock.Setup(static u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(users.FirstOrDefault());
        _ = _employeeServiceMock.Setup(static e => e.GetAsync(It.IsAny<Expression<Func<Employee, bool>>>(), null)).ReturnsAsync((Employee?)null);
        _ = _purchaseorderServiceMock.Setup(static p => p.GetCountAsync(null)).ReturnsAsync(5);

        // Act
        var result = (await _controller.Create()) as ViewResult;

        // Assert
        Assert.NotNull(result);
        _ = result.Should().NotBeNull();
        Assert.NotNull(result.ViewData["ShippingPro"]);
        Assert.NotNull(result.ViewData["ShippingMeth"]);

    }

    [Fact]
    public async Task Create_WithValidModel_ReturnsRedirectToActionResult()
    {
        // Arrange
        var viewModel = new PurchaseOrderViewModel
        {
            PurchaseOrderSingle = new PurchaseOrder() { VendorId = 1, ShippingMethodId = 1, ShippingProviderId = 1 },
        };
        var productSkus = "SKU001";
        var productAverages = "1.99";
        var productCosts = "1.49";
        var vendorId = "1";
        var shippingMethodId = "2";
        var shippingProviderId = "3";
        var productQuantity = "10";
        var expectedDates = "1/10/2024";
        var product = new Product
        {
            ProductId = 1,
            Sku = "SKU001",
            Description = "Test Product"
        };
        var productVendorMapping = new ProductVendorMapping
        {
            ProductVendorMappingId = 1,
            VendorSku = "VSKU001",
            Product = product
        };

        var productPurchaseOrder = new ProductPurchaseOrder
        {
            ProductVendorMapping = productVendorMapping,
            TotalOrdered = 10,
            TotalProductCost = 1.49m,
            CustomCost = 1.49m,
            AverageCost = 1.99m,
            TotalRecieved = 5,
            DiscountPercentage = 0,
            DiscountAmount = 0,
            ExpectedDate = DateTime.Parse(expectedDates)
        };

        var productPurchaseOrderList = new List<ProductPurchaseOrder> { productPurchaseOrder };
        var products = new List<Product> { product };

        _productVendorMappingServiceMock.Setup(x => x.GetVendorsAsync()).ReturnsAsync(new List<Vendor>());
        _purchaseorderServiceMock.Setup(x => x.IsExistsAsync(It.IsAny<Expression<Func<PurchaseOrder, bool>>>())).ReturnsAsync(false);
        _productVendorMappingServiceMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(), It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>())).ReturnsAsync(productVendorMapping);
        _vendorServiceMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Vendor, bool>>>(), It.IsAny<Expression<Func<Vendor, object>>[]>())).ReturnsAsync(new Vendor() { VendorName = "new", VendorNumber = "1" });
        _shippingProviderServiceMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<ShippingProvider, string>>[]>(), It.IsAny<Expression<Func<ShippingProvider, object>>[]>())).ReturnsAsync(new List<ShippingProvider>());
        _shippingMethodServiceMock.Setup(x => x.GetListAsync(It.IsAny<Expression<Func<ShippingMethod, bool>>>(), It.IsAny<Expression<Func<ShippingMethod, string>>[]>(), It.IsAny<Expression<Func<ShippingMethod, object>>[]>())).ReturnsAsync(new List<ShippingMethod>());
        _purchaseorderServiceMock.Setup(x => x.AddAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(new PurchaseOrder());
        _productServiceMock.Setup(x => x.GetListAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Expression<Func<Product, string>>[]>(),
            It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(products);

        var users = new List<IdentityUser> { new() { Id = "1", NormalizedUserName = "USERNAME" } };
        var usersQueryable = users.AsQueryable();
        _ = _userManagerMock.Setup(static m => m.Users).Returns(usersQueryable);
        _ = _userManagerMock.Setup(static u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(users.FirstOrDefault());
        _ = _employeeServiceMock.Setup(static e => e.GetAsync(It.IsAny<Expression<Func<Employee, bool>>>(), null)).ReturnsAsync((Employee)null);
        _ = _purchaseorderServiceMock.Setup(static p => p.GetCountAsync(null)).ReturnsAsync(5);

        if (productPurchaseOrderList == null)
        {
            throw new ArgumentNullException(nameof(productPurchaseOrderList));
        }

        if (products == null)
        {
            throw new ArgumentNullException(nameof(products));
        }

        // Act
        var result = await _controller.Create(
            viewModel,
            productSkus,
            productAverages,
            productCosts,
            vendorId,
            shippingMethodId,
            shippingProviderId,
            productQuantity,
            null,
            "",
            "",
            "",
            expectedDates,
            "",
            ""
        ) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }

    [Fact]
    public async Task Create_ReturnsView_WhenModelStateIsInvalid()
    {
        // Arrange
        var viewModel = new PurchaseOrderViewModel();
        var result = new Mock<IActionResult>();
        _controller.ModelState.AddModelError("key", "error message");

        var _ProductVendorMapping = ProductVendorMappingFixtures.GetTestList();

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetVendorsAsync()).ReturnsAsync([]);
        _ = _purchaseorderServiceMock.Setup(static x => x.IsExistsAsync(It.IsAny<Expression<Func<PurchaseOrder, bool>>>())).ReturnsAsync(false);
        _ = _productServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>())).ReturnsAsync(new Product());
        _ = _productVendorMappingServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(), It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>())).ReturnsAsync(new ProductVendorMapping());
        _ = _productVendorMappingServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Func<IQueryable<ProductVendorMapping>, IQueryable<ProductVendorMapping>>>()
        )).ReturnsAsync(_ProductVendorMapping);
        _ = _vendorServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<Vendor, bool>>>(), It.IsAny<Expression<Func<Vendor, object>>[]>())).ReturnsAsync(new Vendor() { VendorName = "new", VendorNumber = "1" });
        _ = _shippingProviderServiceMock.Setup(static x => x.GetAllAsync(
            It.IsAny<Expression<Func<ShippingProvider, string>>[]>(),
            It.IsAny<Expression<Func<ShippingProvider, object>>[]>()))
            .ReturnsAsync([]);
        _ = _shippingMethodServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Expression<Func<ShippingMethod, bool>>>(),
            It.IsAny<Expression<Func<ShippingMethod, string>>[]>(),
            It.IsAny<Expression<Func<ShippingMethod, object>>[]>()))
        .ReturnsAsync([]);
        _ = _purchaseorderServiceMock.Setup(static x => x.AddAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(new PurchaseOrder());

        // Act
        var actionResult = await _controller.Create(viewModel, "", "", "", "", "", "", "", null, "", "", "", "", "", "");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(actionResult);
        Assert.Equal(false, _controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Details_WithValidId_ReturnsViewResult()
    {
        // Arrange
        int id = 1;

        _ = _purchaseorderServiceMock.Setup(p => p.GetAsync(
                        It.Is<Expression<Func<PurchaseOrder, bool>>>(q => q.Compile()(new PurchaseOrder { PurchaseOrderId = id })),
                        It.Is<Expression<Func<PurchaseOrder, object>>[]>(i => i.Select(x => x.ToString()).SequenceEqual(new[] { "x => x.Vendor", "x => x.ShippingMethod", "x => x.ShippingProvider" }))
                    )).ReturnsAsync(PurchaseOrderFixtures.GetTestList().First());

        _ = _productPurchaseOrderServiceMock.Setup(x => x.GetListAsync(
            It.IsAny<Expression<Func<ProductPurchaseOrder, bool>>>(),
            It.IsAny<Expression<Func<ProductPurchaseOrder, string>>[]>(),
            It.IsAny<Expression<Func<ProductPurchaseOrder, object>>[]>()
        )).ReturnsAsync(ProductPurchaseOrderFixtures.GetTestList().Where(x => x.PurchaseOrderId == 1001).ToList());

        _ = _stocksServiceMock.Setup(s => s.GetListAsync(
                It.IsAny<Expression<Func<Stock, bool>>>(),
                It.IsAny<Expression<Func<Stock, string>>[]>(),
                It.IsAny<Expression<Func<Stock, object>>[]>()
            )
        ).ReturnsAsync(StockFixtures.GetTestStocks());

        _ = _purchaseOrderFilesMappingServiceMock.Setup(p => p.GetListAsync(
            It.IsAny<Expression<Func<PurchaseOrderFilesMapping, bool>>>(),
            It.IsAny<Expression<Func<PurchaseOrderFilesMapping, string>>[]>(),
            It.IsAny<Expression<Func<PurchaseOrderFilesMapping, object>>[]>()
        )).ReturnsAsync([]);

        _ = _productPurchaseOrderStockMappingServiceMock.Setup(p => p.GetListAsync(
            It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, bool>>>(),
            It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, string>>[]>(),
            It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, object>>[]>()
        )).ReturnsAsync([]);

        // Act
        var result = await _controller.Details(id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<PurchaseOrderViewModel>(viewResult.Model);
        _ = model.Should().NotBeNull();
        Assert.Equal("no", viewResult.ViewData["IsComplete"]);
    }
    [Fact]
    public async Task Details_ReturnsNotFound_WhenIdIsNull()
    {
        // Arrange
        int? id = null;

        // Act
        var result = await _controller.Details(id);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void EditStockTest()
    {
        string id = "123";
        string siteLocation = "456";
        int recieved = 10;
        int? productPurchase = 1;
        decimal customCost = 12.5m;
        int productQuantity = 20;
        decimal discountPercentage = 0;
        decimal totalCost = 250m;
        string groupName = "Current Stock Location";

        // mock data for ProductPurchaseOrder
        var productPurchaseOrder = new ProductPurchaseOrder
        {
            ProductPurchaseOrderId = 1,
            TotalRecieved = 5,
            TotalOrdered = 10,
            ProductVendorMapping = new ProductVendorMapping
            {
                ProductVendorMappingId = 1,
                Product = new Product { ProductId = 1, Sku = "ABC" },
                Vendor = new Vendor { VendorId = 1, VendorName = "Vendor", VendorNumber = "1" }
            }
        };
        _ = _productPurchaseOrderServiceMock.Setup(static x => x.GetAsync(
                It.IsAny<Expression<Func<ProductPurchaseOrder, bool>>>(),
                It.IsAny<Expression<Func<ProductPurchaseOrder, object>>[]>()
            )
        ).ReturnsAsync(productPurchaseOrder);

        _ = _productPurchaseOrderServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Expression<Func<ProductPurchaseOrder, bool>>>(),
            It.IsAny<Expression<Func<ProductPurchaseOrder, string>>[]>(),
             It.IsAny<Expression<Func<ProductPurchaseOrder, object>>[]>()))
            .ReturnsAsync(ProductPurchaseOrderFixtures.GetTestList());

        // mock data for Product
        var product = new Product { ProductId = 1, Sku = "ABC" };
        _ = _productServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), null)).ReturnsAsync(product);

        // mock data for Stock
        var stock = new Stock { StockId = 1, TotalAvailable = 5, ProductId = 1, LocationId = 456 };
        _ = _stocksServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<Stock, bool>>>(), It.IsAny<Expression<Func<Stock, object>>[]>())).ReturnsAsync(StockFixtures.GetTestStocks().First());

        // mock data for Location
        var location = new Location { LocationId = 456, LocationName = "Location" };
        _ = _locationServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<Location, bool>>>(), null)).ReturnsAsync(location);

        // mock data for MoveStockHistory
        var moveStockHistory = new MoveStockHistory { MoveStockHistoryId = 1 };
        _ = _moveStockHistoryServiceMock.Setup(static x => x.Add(It.IsAny<MoveStockHistory>())).Returns(new MoveStockHistory());

        // mock data for ProductPurchaseOrderStockMapping
        var productPurchaseOrderStockMapping = new ProductPurchaseOrderStockMapping { ProductPurchaseOrderId = 1, StockId = 1, QtyRecieved = 5 };
        _ = _productPurchaseOrderStockMappingServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, bool>>>(), It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, object>>[]>())).ReturnsAsync(productPurchaseOrderStockMapping);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                                    {
                                        new(ClaimTypes.Name, "John Doe"),
                                        new(ClaimTypes.Email, "johndoe@example.com"),
                                        new(ClaimTypes.Role, RoleList.Administrator),
                                    }));

        _mockHttp.Setup(x => x.HttpContext.User).Returns(user);
        // act
        var result = _controller.EditStock(id, siteLocation, recieved, productPurchase, customCost, productQuantity, discountPercentage, totalCost, groupName);

        // assert
        _ = Assert.IsType<Task<IActionResult>>(result);
    }

    [Fact]
    public void EditStock_WhenRecievedIsNotZero_UpdatesStockAndProductPurchaseOrderAndAddsToHistoryAndReturnsOk()
    {
        // Arrange
        string siteLocation = "1";
        int recieved = 5;
        int productPurchaseOrder = 1;
        decimal customCost = 1.23m;
        int productQuantity = 10;
        decimal discountPercentage = 0m;
        decimal totalCost = 12.3m;
        string groupName = "Current Stock Location";
        ClaimsPrincipal user = new(new ClaimsIdentity([new(ClaimTypes.Name, "testuser")]));

        var productPurchaseOrderDb = new ProductPurchaseOrder
        {
            ProductPurchaseOrderId = productPurchaseOrder,
            TotalOrdered = 10,
            TotalRecieved = 0,
            ProductVendorMapping = new ProductVendorMapping
            {
                Product = new Product
                {
                    ProductId = 1,
                    Sku = "ABC123"
                }
            }
        };

        _ = _productPurchaseOrderServiceMock.Setup(
            x => x.GetAsync(It.IsAny<Expression<Func<ProductPurchaseOrder, bool>>>(),
            It.IsAny<Expression<Func<ProductPurchaseOrder, object>>[]>()
        )).ReturnsAsync(productPurchaseOrderDb);

        var productDb = new Product
        {
            ProductId = 1,
            Sku = "ABC123"
        };

        _ = _productServiceMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(productDb);

        var findStock = new Stock
        {
            StockId = 1,
            TotalAvailable = 5,
            Products = productDb
        };

        _ = _stocksServiceMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Stock, bool>>>(),
                It.IsAny<Expression<Func<Stock, object>>[]>()
        )).ReturnsAsync(StockFixtures.GetTestStocks().First());

        var findLocation = new Location
        {
            LocationId = 1
        };

        _ = _locationServiceMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Location, bool>>>(), null))
            .ReturnsAsync(findLocation);

        var history = new MoveStockHistory();

        _ = _moveStockHistoryServiceMock.Setup(x => x.AddAsync(It.IsAny<MoveStockHistory>()))
            .Callback<MoveStockHistory>(x => history = x);

        var findProductStockMap = new ProductPurchaseOrderStockMapping();

        _ = _productPurchaseOrderStockMappingServiceMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, bool>>>(), It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, object>>[]>()))
            .ReturnsAsync(findProductStockMap);

        // Act
        var result = _controller.EditStock("ABC123", siteLocation, recieved, productPurchaseOrder, customCost, productQuantity, discountPercentage, totalCost, groupName);

        // Assert
        _ = Assert.IsType<Task<IActionResult>>(result);
    }
    [Fact]
    public async Task Edit_ReturnsNotFound_WhenIdIsNull()
    {
        // Arrange
        int? id = null;

        // Act
        var result = await _controller.Edit(id);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_ReturnsNotFoundResult_WhenPurchaseOrderSingleIsNull()
    {
        // Arrange
        int? id = 1;
        _ = _purchaseorderServiceMock.Setup(static m => m.GetAsync(It.IsAny<Expression<Func<PurchaseOrder, bool>>>(),
                                                   It.IsAny<Expression<Func<PurchaseOrder, object>>[]>()))
                                  .ReturnsAsync((PurchaseOrder?)null);

        // Act
        var result = await _controller.Edit(id);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_ReturnsViewResult_WhenPurchaseOrderSingleIsNotNull()
    {
        // Arrange
        int? id = 1;
        _ = _purchaseorderServiceMock.Setup(static m => m.GetAsync(It.IsAny<Expression<Func<PurchaseOrder, bool>>>(),
                                                        It.IsAny<Expression<Func<PurchaseOrder, object>>[]>()))
                                  .ReturnsAsync(PurchaseOrderFixtures.GetTestList().First());

        _ = _productPurchaseOrderServiceMock.Setup(static m => m.GetListAsync(
                                                It.IsAny<Expression<Func<ProductPurchaseOrder, bool>>>()
                                                , It.IsAny<Expression<Func<ProductPurchaseOrder, string>>[]>()
                                                , It.IsAny<Expression<Func<ProductPurchaseOrder, object>>[]>()))
                                        .ReturnsAsync([]);

        _ = _stocksServiceMock.Setup(static s => s.GetListAsync(
                It.IsAny<Expression<Func<Stock, bool>>>(),
                It.IsAny<Expression<Func<Stock, string>>[]>(),
                It.IsAny<Expression<Func<Stock, object>>[]>()
            )
        ).ReturnsAsync([]);

        _ = _productPurchaseOrderStockMappingServiceMock.Setup(static m => m.GetListAsync(
                It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, bool>>>(),
                It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, string>>[]>(),
                It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, object>>[]>()
            )
        )
        .ReturnsAsync([]);

        _ = _purchaseOrderFilesMappingServiceMock.Setup(static m => m.GetListAsync(
                                                    It.IsAny<Expression<Func<PurchaseOrderFilesMapping, bool>>>()
                                                    , It.IsAny<Expression<Func<PurchaseOrderFilesMapping, string>>[]>()
                                                    , It.IsAny<Expression<Func<PurchaseOrderFilesMapping, object>>[]>()))
                                             .ReturnsAsync([]);

        _ = _shippingProviderServiceMock.Setup(static m => m.GetAllAsync(
                                                It.IsAny<Expression<Func<ShippingProvider, string>>[]>()
                                                    , It.IsAny<Expression<Func<ShippingProvider, object>>[]>()))
                                   .ReturnsAsync([]);

        _ = _shippingMethodServiceMock.Setup(static m => m.GetListAsync(
                                    It.IsAny<Expression<Func<ShippingMethod, bool>>>()
                                    , It.IsAny<Expression<Func<ShippingMethod, string>>[]>()
                                    , It.IsAny<Expression<Func<ShippingMethod, object>>[]>()))
                                  .ReturnsAsync([]);

        _ = _productVendorMappingServiceMock.Setup(static m => m.GetListAsync(
            It.IsAny<Expression<Func<ProductVendorMapping, bool>>>()
            , It.IsAny<Expression<Func<ProductVendorMapping, string>>[]>()
            , It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()
        )).ReturnsAsync([]);

        // Act
        var result = await _controller.Edit(id);

        // Assert
        _ = Assert.IsType<ViewResult>(result);
        var resultTask = result as ViewResult;
        //TODO:: REIMPLEMENT
        Assert.NotNull(resultTask?.ViewData["ShippingPro"]);
        Assert.NotNull(resultTask?.ViewData["ShippingMeth"]);
        Assert.NotNull(resultTask?.ViewData["VendorProducts"]);
    }

    [Fact]
    public async Task Edit_ReturnsNotFoundResult_WhenIdIsZero()
    {
        // Arrange
        int id = 0;
        var viewModel = new PurchaseOrderViewModel
        {
            PurchaseOrderSingle = PurchaseOrderFixtures.GetTestList().First()
        };
        // Act
        var result = await _controller.Edit(id, viewModel, null, null);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_ReturnsNotFoundResult_WhenPurchaseOrderIsNull()
    {
        // Arrange
        int id = 1;
        PurchaseOrderViewModel? purchaseOrderView = null;
        _ = _purchaseorderServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<PurchaseOrder, bool>>>(), It.IsAny<Expression<Func<PurchaseOrder, object>>[]>())).ReturnsAsync(PurchaseOrderFixtures.GetTestList().First());

        // Act
        var result = await _controller.Edit(id, purchaseOrderView, null, null);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_ReturnsRedirectToActionResult_WhenModelStateIsValid()
    {
        // Arrange
        int id = 1;
        var viewModel = new PurchaseOrderViewModel
        {
            PurchaseOrderSingle = PurchaseOrderFixtures.GetTestList().First()
        };
        _ = _purchaseorderServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<PurchaseOrder, bool>>>(), It.IsAny<Expression<Func<PurchaseOrder, object>>[]>())).ReturnsAsync(new PurchaseOrder());

        _ = _productPurchaseOrderServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Expression<Func<ProductPurchaseOrder, bool>>>()
        , It.IsAny<Expression<Func<ProductPurchaseOrder, string>>[]>()
        , It.IsAny<Expression<Func<ProductPurchaseOrder, object>>[]>())).ReturnsAsync([]);

        _ = _shippingMethodServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<ShippingMethod, bool>>>(), It.IsAny<Expression<Func<ShippingMethod, object>>[]>())).ReturnsAsync(new ShippingMethod());

        _ = _shippingProviderServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<ShippingProvider, bool>>>(), It.IsAny<Expression<Func<ShippingProvider, object>>[]>())).ReturnsAsync(new ShippingProvider());

        // Act
        var result = await _controller.Edit(id, viewModel, null, null);

        // Assert
        _ = Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task DeleteFile_WithValidId_ReturnsRedirectToActionResult()
    {
        // Arrange
        int id = 1;
        var getPOFileMapping = new PurchaseOrderFilesMapping
        {
            PurchaseOrderFilesMappingId = id,
            PurchaseOrderId = 2,
            FileId = 3,
            PurchaseOrder = new PurchaseOrder(),
            Files = new Files()
        };
        var getFile = new Files { FileId = getPOFileMapping.FileId };

        _ = _purchaseOrderFilesMappingServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<PurchaseOrderFilesMapping, bool>>>(),
            It.IsAny<Expression<Func<PurchaseOrderFilesMapping, object>>[]>()
        )).ReturnsAsync(getPOFileMapping);

        _ = _fileServiceMock.Setup(static x => x.RemoveAsync(It.IsAny<int>())).ReturnsAsync(1);
        _ = _purchaseOrderFilesMappingServiceMock.Setup(static x => x.RemoveAsync(It.IsAny<int>())).ReturnsAsync(1);

        _ = _fileServiceMock.Setup(static x => x.UploadToAzureAsync(
            It.IsAny<IFormFile>(), It.IsAny<FileType>()
        )).ReturnsAsync("");

        // Act
        var result = await _controller.DeleteFile(id);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Edit", redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        Assert.Equal(getPOFileMapping.PurchaseOrderId, redirectToActionResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task DeleteFile_WithInvalidId_ReturnsViewResult()
    {
        // Arrange
        int? id = null;

        // Act
        var result = await _controller.DeleteFile(id);

        // Assert
        _ = Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task GetVendorInformation_WithNullId_ReturnsOkResult()
    {
        // Arrange
        string? id = null;

        // Act
        var result = await _controller.GetVendorInformation(id);

        // Assert
        _ = Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task GetVendorInformation_WithValidId_ReturnsJsonResult()
    {
        // Arrange
        string id = "1";
        var getDbVendor = new Vendor { VendorId = 1, PhoneNumber = "1234567890", VendorName = "New", VendorNumber = "1", BusinessEmail = "test@test.com" };
        var getPVM = new List<ProductVendorMapping>
        {
            new() { ProductVendorMappingId = 1, ProductId = 1, VendorId = 1, Product = new Product() },
            new() { ProductVendorMappingId = 2, ProductId = 2, VendorId = 1, Product = new Product() }
        };

        _ = _vendorServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<Vendor, bool>>>(), It.IsAny<Expression<Func<Vendor, object>>[]>()))
            .ReturnsAsync(getDbVendor);

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
            It.IsAny<Expression<Func<ProductVendorMapping, string>>[]>(),
            It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()
        )).ReturnsAsync(getPVM);

        // Act
        var result = await _controller.GetVendorInformation(id);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        _ = jsonResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductsbyVendorId_WithNullId_ReturnsOkResult()
    {
        // Arrange
        string? id = null;

        // Act
        var result = await _controller.GetProductsbyVendorId(id);

        // Assert
        _ = Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task GetProductsbyVendorId_WithValidId_ReturnsJsonResult()
    {
        // Arrange
        string id = "1";
        var getDbVendor = new Vendor { VendorId = 1, VendorName = "new", VendorNumber = "1" };
        var getPVM = new List<ProductVendorMapping>
        {
            new() { ProductVendorMappingId = 1, ProductId = 1, VendorId = 1, Product = new Product() },
            new() { ProductVendorMappingId = 2, ProductId = 2, VendorId = 1, Product = new Product() }
        };

        _ = _vendorServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<Vendor, bool>>>(), It.IsAny<Expression<Func<Vendor, object>>[]>()))
            .ReturnsAsync(getDbVendor);

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
            It.IsAny<Expression<Func<ProductVendorMapping, string>>[]>(),
            It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()
        )).ReturnsAsync(getPVM);

        // Act
        var result = await _controller.GetProductsbyVendorId(id);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var products = Assert.IsType<List<ProductVendorMapping>>(jsonResult.Value);
        Assert.Equal(getPVM.Count, products.Count);
        foreach (var p in products)
        {
            Assert.Equal(getDbVendor.VendorId, p.VendorId);
        }
    }

    [Fact]
    public async Task GetPVMCost_ReturnsJsonResult()
    {
        // Arrange
        int productId = 1;
        int vendorId = 1;
        var product = new ProductVendorMapping
        {
            ProductVendorMappingId = 1
        };
        _ = _productVendorMappingServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(), It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetPVMDetails(productId, vendorId);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        _ = jsonResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPVMAverageCost_ReturnsJsonResult_WithAverageCost()
    {
        // Arrange
        string id = "1";
        var cost = 10.0m;
        var pvmId = 1;

        var pvm = new ProductVendorMapping { ProductVendorMappingId = pvmId, Product = new Product { Cost = cost } };

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(), It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
            .ReturnsAsync(pvm);

        var productPurchaseOrders = new List<ProductPurchaseOrder> {
            new() { ProductVendorMapping = pvm, PurchaseOrder = new PurchaseOrder { POStatus = Status.Close } },
            new() { ProductVendorMapping = pvm, PurchaseOrder = new PurchaseOrder { POStatus = Status.Close } },
            new() { ProductVendorMapping = pvm, PurchaseOrder = new PurchaseOrder { POStatus = Status.Close } }
        };

        _ = _productPurchaseOrderServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Expression<Func<ProductPurchaseOrder,
        bool>>>(), It.IsAny<Expression<Func<ProductPurchaseOrder, string>>[]>(),
        It.IsAny<Expression<Func<ProductPurchaseOrder, object>>[]>()))
            .ReturnsAsync(productPurchaseOrders);

        // Act
        var result = await _controller.GetPVMAverageCost(id);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        string json = JsonConvert.SerializeObject(jsonResult.Value);
        Assert.Equal(productPurchaseOrders.Average(static x => x.CustomCost), Convert.ToDecimal(JObject.Parse(json)["getaverage"]));
        Assert.Equal(cost, Convert.ToDecimal(JObject.Parse(json)["getcost"]));
        Assert.Equal(pvmId, Convert.ToInt32(JObject.Parse(json)["pvmid"]));
    }

    [Fact]
    public async Task GetPVMAverageCost_ReturnsJsonResult_WithDefaultValues_WhenExceptionIsThrown()
    {
        // Arrange
        string id = "1";

        _ = _productVendorMappingServiceMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(), It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
            .Throws<Exception>();

        // Act

        // Assert
        _ = await Assert.ThrowsAsync<Exception>(async () => await _controller.GetPVMAverageCost(id));
    }

    [Fact]
    public async Task MethodsByShipPro_ReturnsJsonResult_WithCorrectData()
    {
        // Arrange
        int shipId = 1; // set your test ship id
        var expectedData = new List<ShippingMethod> {
            new() { ShippingMethodName = "Method A", ShippingProviderId = shipId },
            new() { ShippingMethodName = "Method B", ShippingProviderId = shipId },
            new() { ShippingMethodName = "Method C", ShippingProviderId = shipId }
        };

        _ = _shippingMethodServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Expression<Func<ShippingMethod, bool>>>(),
            It.IsAny<Expression<Func<ShippingMethod, string>>[]>(),
            It.IsAny<Expression<Func<ShippingMethod, object>>[]>()
        )).ReturnsAsync(expectedData);

        // Act
        var result = await _controller.MethodsByShipPro(shipId);

        // Assert
        // Assert
        Assert.NotNull(result);
        _ = Assert.IsType<JsonResult>(result);
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.Equal(expectedData, jsonResult.Value);

    }

    [Fact]
    public async Task GetCurrentStock_ReturnsStocks()
    {
        // Arrange
        var psku = "ABC123";
        var expectedStocks = new List<object>
        {
            new { StockId = 1, Loc = "Site 1: Location 1", groupname = "Current Stock Location" },
            new { StockId = 2, Loc = "Site 2: Location 2", groupname = "Current Stock Location" },
            new { StockId = 3, Loc = "Site 3: Location 3", groupname = "Current Stock Location" }
        };

        _ = _stocksServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Func<IQueryable<Stock>, IQueryable<object>>>()
        )).ReturnsAsync(expectedStocks);


        // Act
        var result = await _controller.getCurrentStock(psku);

        // Assert
        var okResult = Assert.IsType<JsonResult>(result);
        var actualStocks = JsonConvert.SerializeObject(okResult.Value);
        Assert.Equal(actualStocks, JsonConvert.SerializeObject(expectedStocks));
    }

    [Fact]
    public async Task GetAllLocations_ReturnsJsonResult_WithExpectedData()
    {
        // Arrange
        string psku = "exampleSKU";

        var queryResult = new List<object>{
            new
            {
                LocationId = 1,
                Loc = "Location 1",
                groupname = "Site 1"
            },
            new
            {
                LocationId = 2,
                Loc = "Location 2",
                groupname = "Site 3"
            }
        };


        var expectedResult = new JsonResult(queryResult);

        _ = _locationServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<Location>, IQueryable<object>>>()))
            .ReturnsAsync(queryResult);

        // Act
        var result = await _controller.GetAllLocations(psku);

        // Assert
        Assert.NotNull(result);
        _ = Assert.IsType<JsonResult>(result);
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.Equal(expectedResult.Value, jsonResult.Value);
    }

    [Fact]
    public async Task GetStockList_ReturnsPurchaseOrderViewModel()
    {
        // Arrange
        var psku = "1";
        var productPurchaseOrderId = Guid.NewGuid();
        var productPurchaseOrderStockMapping = new ProductPurchaseOrderStockMapping
        {
            ProductPurchaseOrderId = 1
        };
        var expected = new PurchaseOrderViewModel
        {
            ProductPurchaseOrderStockMappings = [productPurchaseOrderStockMapping]
        };

        _ = _productPurchaseOrderStockMappingServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, bool>>>(),
                It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, string>>[]>(),
                It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, object>>[]>()
            )
        ).ReturnsAsync([productPurchaseOrderStockMapping]);

        // Act
        var actual = await _controller.GetStockList(psku);

        // Assert
        Assert.Equal(expected.ProductPurchaseOrderStockMappings.Count, actual.ProductPurchaseOrderStockMappings.Count);
        Assert.Equal(expected.ProductPurchaseOrderStockMappings[0].ProductPurchaseOrderId, actual.ProductPurchaseOrderStockMappings[0].ProductPurchaseOrderId);
    }

    [Fact]
    public async Task GetStockList_NullData_ReturnsPurchaseOrderViewModel()
    {
        // Arrange
        var psku = "1";

        _ = _productPurchaseOrderStockMappingServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, bool>>>(),
                It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, string>>[]>(),
                It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, object>>[]>()
            )
        ).ReturnsAsync((List<ProductPurchaseOrderStockMapping>?)null);

        // Act
        var result = await _controller.GetStockList(psku);

        // Assert
        Assert.Null(result.ProductPurchaseOrderSingle);
    }

    [Fact]
    public void GetStockList_ReturnsJsonResult_WithDefaultValues_WhenExceptionIsThrown()
    {
        var expected = new PurchaseOrderViewModel();

        _ = _productPurchaseOrderStockMappingServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, bool>>>(),
                It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, string>>[]>(),
                It.IsAny<Expression<Func<ProductPurchaseOrderStockMapping, object>>[]>()
            )
        ).Throws<Exception>();

        // Act

        // Assert
        //Assert.ThrowsAsync<Exception>(async () => await _controller.GetStockList(psku));
    }

    [Fact]
    public void ClosePO_ReturnsRedirectToActionResult()
    {
        // Arrange
        int? id = 1;
        var expected = nameof(_controller.Index);

        _ = _purchaseorderServiceMock.Setup(static x => x.Close(It.IsAny<int>()));

        // Act
        var actual = _controller.ClosePO(id);

        // Assert
        var result = Assert.IsType<RedirectToActionResult>(actual);
        Assert.Equal(expected, result.ActionName);
    }

    [Fact]
    public async Task CancelPO_ReturnsRedirectToActionResult()
    {
        // Arrange
        int? id = 1;
        var purchaseOrder = new PurchaseOrder { PurchaseOrderId = id.Value, POStatus = Status.Cancelled };
        var productPurchaseOrders = new List<ProductPurchaseOrder>
        {
            new()
            {
                TotalOrdered = 5,
                TotalRecieved = 2,
                PurchaseOrder = purchaseOrder,
                ProductVendorMapping = new ProductVendorMapping { ProductId = 1 }
            },
            new()
            {
                TotalOrdered = 3,
                TotalRecieved = 1,
                PurchaseOrder = purchaseOrder,
                ProductVendorMapping = new ProductVendorMapping { ProductId = 2 }
            }
        };
        var products = new List<Product>
        {
            new() { ProductId = 1, OnOrder = 1 },
            new() { ProductId = 2, OnOrder = 2 }
        };

        _ = _purchaseorderServiceMock
            .Setup(static x => x.GetAsync(It.IsAny<Expression<Func<PurchaseOrder, bool>>>(), It.IsAny<Expression<Func<PurchaseOrder, object>>[]>()))
            .ReturnsAsync(purchaseOrder);

        _ = _productPurchaseOrderServiceMock
            .Setup(static x => x.GetListAsync(
                It.IsAny<Expression<Func<ProductPurchaseOrder, bool>>>(),
               It.IsAny<Expression<Func<ProductPurchaseOrder, string>>[]>(),
                It.IsAny<Expression<Func<ProductPurchaseOrder, object>>[]>()
            ))
            .ReturnsAsync(productPurchaseOrders);

        _ = _productServiceMock
            .Setup(static x => x.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(products.First());

        // Act
        var result = await _controller.CancelPO(id);

        // Assert
        _ = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Index), ((RedirectToActionResult)result).ActionName);
        Assert.Equal(Status.Cancelled, purchaseOrder.POStatus);
        Assert.Equal(purchaseOrder.ModifyByUser, _controller.User.Identity?.Name);
        _purchaseorderServiceMock.Verify(static x => x.UpdateAsync(It.IsAny<PurchaseOrder>()), Times.Once);
        _productServiceMock.Verify(static x => x.UpdateAsync(It.IsAny<Product>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AddProduct_ValidData_RedirectsToEditPage()
    {
        // Arrange
        var purchaseOrderId = 1;
        var productVendorMappingId = 2;
        var customCost = 10.0M;
        var totalOrdered = 5;
        var totalReceived = 0;

        var product = new Product { ProductId = 3, Cost = 20.0M };
        var productVendorMapping = new ProductVendorMapping { ProductVendorMappingId = productVendorMappingId, ProductId = product.ProductId };
        var purchaseOrder = new PurchaseOrder { PurchaseOrderId = purchaseOrderId, POStatus = Status.InProgress };
        var purchaseOrderView = new PurchaseOrderViewModel
        {
            PurchaseOrderSingle = new PurchaseOrder { PurchaseOrderId = purchaseOrderId },
            ProductPurchaseOrderSingle = new ProductPurchaseOrder { ProductVendorMappingId = productVendorMappingId, CustomCost = customCost, TotalOrdered = totalOrdered, TotalRecieved = totalReceived }
        };

        var productsWithStock = new Product { ProductId = product.ProductId, StockTotalAvailable = 5 };

        _ = _stocksServiceMock.Setup(static s => s.GetAsync(
            It.IsAny<Func<IQueryable<Stock>, IQueryable<Product>>>()
        )).ReturnsAsync(productsWithStock);


        _ = _purchaseorderServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<PurchaseOrder, bool>>>(), It.IsAny<Expression<Func<PurchaseOrder, object>>[]>())).ReturnsAsync(purchaseOrder);
        _ = _purchaseorderServiceMock.Setup(static s => s.UpdateAsync(It.IsAny<PurchaseOrder>()));


        _ = _productVendorMappingServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(), It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>())).ReturnsAsync(productVendorMapping);


        _ = _productServiceMock.Setup(static s => s.GetAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<Product, object>>[]>()
            )
        ).ReturnsAsync(product);
        _ = _productServiceMock.Setup(static s => s.UpdateAsync(It.IsAny<Product>()));


        _ = _productPurchaseOrderServiceMock.Setup(static s => s.AddAsync(It.IsAny<ProductPurchaseOrder>())).ReturnsAsync(new ProductPurchaseOrder());
        _ = _productPurchaseOrderServiceMock.Setup(static s => s.GetListAsync(It.IsAny<Expression<Func<ProductPurchaseOrder, bool>>>(), It.IsAny<Expression<Func<ProductPurchaseOrder, string>>[]>(), It.IsAny<Expression<Func<ProductPurchaseOrder, object>>[]>()))
            .ReturnsAsync([]);


        // Act
        var result = await _controller.AddProduct(purchaseOrderId, purchaseOrderView);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Edit), redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        Assert.Equal(purchaseOrderId, redirectToActionResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task GenerateBarcode_Returns_JsonResult_With_ImageData()
    {
        // Arrange
        int purchaseOrderId = 1;
        var purchaseOrder = new PurchaseOrder { PurchaseOrderId = purchaseOrderId };
        _ = _purchaseorderServiceMock.Setup(static x => x.GetAsync(It.IsAny<Expression<Func<PurchaseOrder, bool>>>(), It.IsAny<Expression<Func<PurchaseOrder, object>>[]>()))
            .ReturnsAsync(purchaseOrder);

        // Act
        var result = await _controller.GenerateBarcode(purchaseOrderId);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var img = Assert.IsType<string>(jsonResult.Value);
        Assert.StartsWith("data:image/png;base64,", img);
    }

    [Fact]
    public async Task GenerateBarcode_Returns_JsonResult_With_PleaseSelectProduct_When_PurchaseOrderId_Is_0()
    {
        // Arrange
        int purchaseOrderId = 0;

        // Act
        var result = await _controller.GenerateBarcode(purchaseOrderId);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var message = Assert.IsType<string>(jsonResult.Value);
        Assert.Equal("please select a product", message);
    }

    [Fact]
    public async Task SaveFile_Returns_OkResult_When_Inputs_Are_Null()
    {
        // Act
        var result = await _controller.SaveFile(null, null, null);

        // Assert
        _ = Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task SaveFile_Returns_RedirectToActionResult_When_Extension_Is_Not_Valid()
    {
        // Arrange
        var file = new Mock<IFormFile>();
        _ = file.Setup(static x => x.FileName).Returns("invalid.txt");

        // Act
        var result = await _controller.SaveFile("123", file.Object, "test.jpg");

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Edit", redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        Assert.Equal("123", redirectToActionResult.RouteValues["id"]);
    }

    [Fact]
    public async Task GetPurchaseOrderList_ReturnsExpectedResult()
    {
        // Arrange
        string productFilter = "";
        string statusFilter = "";
        string startDateInput = DateTime.Now.ToString();
        string endDateInput = DateTime.Now.ToString();
        bool isEstimateDate = false;
        bool islastDate = false;
        List<ProductPurchaseOrder> productPurchaseOrders =
        [
            new() { ProductVendorMapping = new ProductVendorMapping { ProductId = 1 } },
            new() { ProductVendorMapping = new ProductVendorMapping { ProductId = 2 } }
        ];

        List<PurchaseOrder> purchaseOrders =
        [
            new() { PurchaseOrderId = 1, POStatus = Status.InProgress, ModifyDate = new DateTime(2022, 1, 15) },
            new() { PurchaseOrderId = 2, POStatus = Status.InProgress, ModifyDate = new DateTime(2022, 1, 20) },
            new() { PurchaseOrderId = 3, POStatus = Status.InProgress, ModifyDate = new DateTime(2022, 1, 25) }
        ];

        var request = new Mock<HttpRequest>();
        _ = request.Setup(static x => x.Form).Returns(new FormCollection(new Dictionary<string, StringValues>
        {
            { "draw", "1" },
            { "start", "0" },
            { "length", "10" },
            { "order[0][column]", "Sku" },
            { "columns[Sku][name]", "Sku" },
            { "order[0][dir]", "asc" },
            { "search[value]", "1" }
        }));

        _controller.Request.Form = request.Object.Form;

        _ = _productPurchaseOrderServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Func<IQueryable<ProductPurchaseOrder>, IQueryable<ProductPurchaseOrder>>>()
        )).ReturnsAsync(productPurchaseOrders);

        _ = _purchaseorderServiceMock.Setup(static x => x.QueryFilter(
            It.IsAny<Func<IQueryable<PurchaseOrder>, IQueryable<PurchaseOrder>>>()
        )).Returns(PurchaseOrderFixtures.GetTestList().AsQueryable());

        // Act
        var result = await _controller.GetPurchaseOrderList(productFilter, statusFilter, startDateInput, endDateInput, isEstimateDate, islastDate) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        //var data = Assert.IsType<PurchaseOrderViewModel>(result);
        //Assert.Equal(3, data.PurchaseOrders.Count());

        Assert.NotNull(result);
        var data = result.Value?.GetType().GetProperty("data")?.GetValue(result.Value, null);
        var productList = Assert.IsType<List<PurchaseOrder>>(data);
        _ = productList.Should().NotBeNullOrEmpty();

        _purchaseorderServiceMock.Verify(
            static x => x.QueryFilter(It.IsAny<Func<IQueryable<PurchaseOrder>, IQueryable<PurchaseOrder>>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Create_VendorNotSelected_ReturnsModelState_Error()
    {
        // Arrange
        var viewModel = new PurchaseOrderViewModel
        {
            PurchaseOrderSingle = new PurchaseOrder() { VendorId = 0, ShippingMethodId = 1, ShippingProviderId = 1 },
        };
        var productSkus = "SKU001";
        var productAverages = "1.99";
        var productCosts = "1.49";
        var vendorId = "1";
        var shippingMethodId = "2";
        var shippingProviderId = "3";
        var productQuantity = "10";
        var expectedDate = "10/10/2024";
        var product = new Product
        {
            Sku = "SKU001"
        };
        var _ProductVendorMapping = ProductVendorMappingFixtures.GetTestList();

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetVendorsAsync()).ReturnsAsync([]);
        _ = _purchaseorderServiceMock.Setup(static x => x.IsExistsAsync(
            It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
            .ReturnsAsync(false);

        _ = _productServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(product);

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
            It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
            .ReturnsAsync(new ProductVendorMapping());

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<ProductVendorMapping>, IQueryable<ProductVendorMapping>>>())).ReturnsAsync(_ProductVendorMapping);

        _ = _vendorServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<Vendor, bool>>>(),
            It.IsAny<Expression<Func<Vendor, object>>[]>()))
            .ReturnsAsync(new Vendor() { VendorName = "new", VendorNumber = "1" });

        _ = _shippingProviderServiceMock.Setup(static x => x.GetAllAsync(
            It.IsAny<Expression<Func<ShippingProvider, string>>[]>(),
            It.IsAny<Expression<Func<ShippingProvider, object>>[]>()))
            .ReturnsAsync([]);

        _ = _shippingMethodServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Expression<Func<ShippingMethod, bool>>>(),
            It.IsAny<Expression<Func<ShippingMethod, string>>[]>(),
            It.IsAny<Expression<Func<ShippingMethod, object>>[]>()))
        .ReturnsAsync([]);

        _ = _purchaseorderServiceMock.Setup(static x => x.AddAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(new PurchaseOrder());


        // Act
        var result = await _controller.Create(viewModel, productSkus, productAverages, productCosts, vendorId, shippingMethodId, shippingProviderId, productQuantity, null, "", "", "", expectedDate, "", "");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(viewModel, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey("PurchaseOrderSingle.VendorId"));

        var modelStateEntry = _controller.ModelState["PurchaseOrderSingle.VendorId"];
        var errorMessages = modelStateEntry?.Errors.Select(static e => e.ErrorMessage).ToList() ?? new List<string>();

        Assert.Contains("A vendor needs to be selected..", errorMessages);

    }

    [Fact]
    public async Task Create_ShippingProviderNotSelected_ReturnsModelState_Error()
    {
        // Arrange
        var viewModel = new PurchaseOrderViewModel
        {
            PurchaseOrderSingle = new PurchaseOrder() { VendorId = 1, ShippingMethodId = 1, ShippingProviderId = 0 },
        };
        var productSkus = "SKU001";
        var productAverages = "1.99";
        var productCosts = "1.49";
        var vendorId = "1";
        var shippingMethodId = "2";
        var shippingProviderId = "3";
        var productQuantity = "10";
        var expectedDate = "10/10/2024";
        var product = new Product
        {
            Sku = "SKU001"
        };
        var _ProductVendorMapping = ProductVendorMappingFixtures.GetTestList();

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetVendorsAsync()).ReturnsAsync([]);
        _ = _purchaseorderServiceMock.Setup(static x => x.IsExistsAsync(
            It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
            .ReturnsAsync(false);

        _ = _productServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(product);

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
            It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
            .ReturnsAsync(new ProductVendorMapping());

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<ProductVendorMapping>, IQueryable<ProductVendorMapping>>>())).ReturnsAsync(_ProductVendorMapping);

        _ = _vendorServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<Vendor, bool>>>(),
            It.IsAny<Expression<Func<Vendor, object>>[]>()))
            .ReturnsAsync(new Vendor() { VendorName = "new", VendorNumber = "1" });

        _ = _shippingProviderServiceMock.Setup(static x => x.GetAllAsync(
            It.IsAny<Expression<Func<ShippingProvider, string>>[]>(),
            It.IsAny<Expression<Func<ShippingProvider, object>>[]>()))
            .ReturnsAsync([]);

        _ = _shippingMethodServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Expression<Func<ShippingMethod, bool>>>(),
            It.IsAny<Expression<Func<ShippingMethod, string>>[]>(),
            It.IsAny<Expression<Func<ShippingMethod, object>>[]>()))
        .ReturnsAsync([]);

        _ = _purchaseorderServiceMock.Setup(static x => x.AddAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(new PurchaseOrder());


        // Act
        var result = await _controller.Create(viewModel, productSkus, productAverages, productCosts, vendorId, shippingMethodId, shippingProviderId, productQuantity, null, "", "", "", expectedDate, "", "");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(viewModel, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey("PurchaseOrderSingle.ShippingProviderId"));

        var modelStateEntry = _controller.ModelState["PurchaseOrderSingle.ShippingProviderId"];
        var errorMessages = modelStateEntry?.Errors.Select(static e => e.ErrorMessage).ToList() ?? new List<string>();

        Assert.Contains("A Shipping Provider needs to be selected..", errorMessages);

    }

    [Fact]
    public async Task Create_ShippingMethodNotSelected_ReturnsModelState_Error()
    {
        // Arrange
        var viewModel = new PurchaseOrderViewModel
        {
            PurchaseOrderSingle = new PurchaseOrder() { VendorId = 1, ShippingMethodId = 0, ShippingProviderId = 1 },
        };
        var productSkus = "SKU001";
        var productAverages = "1.99";
        var productCosts = "1.49";
        var vendorId = "1";
        var shippingMethodId = "2";
        var shippingProviderId = "3";
        var productQuantity = "10";
        var expectedDate = "10/10/2024";
        var product = new Product
        {
            Sku = "SKU001"
        };
        var _ProductVendorMapping = ProductVendorMappingFixtures.GetTestList();

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetVendorsAsync()).ReturnsAsync([]);
        _ = _purchaseorderServiceMock.Setup(static x => x.IsExistsAsync(
            It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
            .ReturnsAsync(false);

        _ = _productServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(product);

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
            It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
            .ReturnsAsync(new ProductVendorMapping());

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<ProductVendorMapping>, IQueryable<ProductVendorMapping>>>())).ReturnsAsync(_ProductVendorMapping);

        _ = _vendorServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<Vendor, bool>>>(),
            It.IsAny<Expression<Func<Vendor, object>>[]>()))
            .ReturnsAsync(new Vendor() { VendorName = "new", VendorNumber = "1" });

        _ = _shippingProviderServiceMock.Setup(static x => x.GetAllAsync(
            It.IsAny<Expression<Func<ShippingProvider, string>>[]>(),
            It.IsAny<Expression<Func<ShippingProvider, object>>[]>()))
            .ReturnsAsync([]);

        _ = _shippingMethodServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Expression<Func<ShippingMethod, bool>>>(),
            It.IsAny<Expression<Func<ShippingMethod, string>>[]>(),
            It.IsAny<Expression<Func<ShippingMethod, object>>[]>()))
        .ReturnsAsync([]);

        _ = _purchaseorderServiceMock.Setup(static x => x.AddAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(new PurchaseOrder());


        // Act
        var result = await _controller.Create(viewModel, productSkus, productAverages, productCosts, vendorId, shippingMethodId, shippingProviderId, productQuantity, null, "", "", "", expectedDate, "", "");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(viewModel, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey("PurchaseOrderSingle.ShippingMethodId"));

        var modelStateEntry = _controller.ModelState["PurchaseOrderSingle.ShippingMethodId"];
        var errorMessages = modelStateEntry.Errors.Select(static e => e.ErrorMessage).ToList();

        Assert.Contains("A Shipping Method needs to be selected..", errorMessages);

    }

    [Fact]
    public async Task Create_productskusIsNull_ReturnsModelState_Error()
    {
        // Arrange
        var viewModel = new PurchaseOrderViewModel
        {
            PurchaseOrderSingle = new PurchaseOrder() { VendorId = 1, ShippingMethodId = 1, ShippingProviderId = 1 },
        };
        var productAverages = "1.99";
        var productCosts = "1.49";
        var vendorId = "1";
        var shippingMethodId = "2";
        var shippingProviderId = "3";
        var productQuantity = "10";
        var expectedDate = "10/10/2024";
        var product = new Product
        {
            Sku = "SKU001"
        };
        var _ProductVendorMapping = ProductVendorMappingFixtures.GetTestList();
        _ = _productVendorMappingServiceMock.Setup(static x => x.GetVendorsAsync()).ReturnsAsync([]);
        _ = _purchaseorderServiceMock.Setup(static x => x.IsExistsAsync(
            It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
            .ReturnsAsync(false);

        _ = _productServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(product);

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
            It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
            .ReturnsAsync(new ProductVendorMapping());

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<ProductVendorMapping>, IQueryable<ProductVendorMapping>>>())).ReturnsAsync(_ProductVendorMapping);

        _ = _vendorServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<Vendor, bool>>>(),
            It.IsAny<Expression<Func<Vendor, object>>[]>()))
            .ReturnsAsync(new Vendor() { VendorName = "new", VendorNumber = "1" });

        _ = _shippingProviderServiceMock.Setup(static x => x.GetAllAsync(
            It.IsAny<Expression<Func<ShippingProvider, string>>[]>(),
            It.IsAny<Expression<Func<ShippingProvider, object>>[]>()))
            .ReturnsAsync([]);

        _ = _shippingMethodServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Expression<Func<ShippingMethod, bool>>>(),
            It.IsAny<Expression<Func<ShippingMethod, string>>[]>(),
            It.IsAny<Expression<Func<ShippingMethod, object>>[]>()))
        .ReturnsAsync([]);

        _ = _purchaseorderServiceMock.Setup(static x => x.AddAsync(It.IsAny<PurchaseOrder>())).ReturnsAsync(new PurchaseOrder());


        // Act
        var result = await _controller.Create(viewModel, null, productAverages, productCosts, vendorId, shippingMethodId, shippingProviderId, productQuantity, null, "", "", "", expectedDate, "", "");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(viewModel, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey("productSUK"));

        var modelStateEntry = _controller.ModelState["productSUK"];
        var errorMessages = modelStateEntry.Errors.Select(static e => e.ErrorMessage).ToList();

        Assert.Contains("No Products Were Assigned to the PO", errorMessages);

    }

    [Fact]
    public void Create_While_AddTo_PurchaseorderService_ExcptionThrow()
    {
        // Arrange
        var viewModel = new PurchaseOrderViewModel
        {
            PurchaseOrderSingle = new PurchaseOrder() { VendorId = 1, ShippingMethodId = 1, ShippingProviderId = 1 },
        };
        var product = new Product
        {
            Sku = "SKU001"
        };
        var _ProductVendorMapping = ProductVendorMappingFixtures.GetTestList();
        _ = _productVendorMappingServiceMock.Setup(static x => x.GetVendorsAsync()).ReturnsAsync([]);
        _ = _purchaseorderServiceMock.Setup(static x => x.IsExistsAsync(
            It.IsAny<Expression<Func<PurchaseOrder, bool>>>()))
            .ReturnsAsync(false);

        _ = _productServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(product);

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
            It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
            .ReturnsAsync(new ProductVendorMapping());

        _ = _productVendorMappingServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<ProductVendorMapping>, IQueryable<ProductVendorMapping>>>())).ReturnsAsync(_ProductVendorMapping);

        _ = _vendorServiceMock.Setup(static x => x.GetAsync(
            It.IsAny<Expression<Func<Vendor, bool>>>(),
            It.IsAny<Expression<Func<Vendor, object>>[]>()))
            .ReturnsAsync(new Vendor() { VendorName = "new", VendorNumber = "1" });

        _ = _shippingProviderServiceMock.Setup(static x => x.GetAllAsync(
            It.IsAny<Expression<Func<ShippingProvider, string>>[]>(),
            It.IsAny<Expression<Func<ShippingProvider, object>>[]>()))
            .ReturnsAsync([]);

        _ = _shippingMethodServiceMock.Setup(static x => x.GetListAsync(
            It.IsAny<Expression<Func<ShippingMethod, bool>>>(),
            It.IsAny<Expression<Func<ShippingMethod, string>>[]>(),
            It.IsAny<Expression<Func<ShippingMethod, object>>[]>()))
        .ReturnsAsync([]);

        _ = _purchaseorderServiceMock.Setup(static x => x.AddAsync(It.IsAny<PurchaseOrder>())).Throws<Exception>();

        // Assert
        //Assert.ThrowsAsync<Exception>(async () => await _controller.Create(viewModel, null, productAverages, productCosts, vendorId, shippingMethodId, shippingProviderId, productQuantity, null, "", "",""));

    }
}