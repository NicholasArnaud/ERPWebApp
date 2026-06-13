using ERPWebApp.Controllers;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Orders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.UnitTests.Controllers;

[Trait("Category", "execute")]
public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _orderServiceMock = new();
    private readonly Mock<IOrderTagService> _orderTagServiceMock = new();
    private readonly Mock<IOrderShippingService> _orderShippingServiceMock = new();
    private readonly Mock<IOrderFulfillmentService> _orderFulfillmentServiceMock = new();
    private readonly Mock<IShipStationStoreService> _shipstationStoreServiceMock = new();
    private readonly Mock<IShippingScanoutService> _shippingScanoutServiceMock = new();
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly Mock<IDepartmentService> _departmentServiceMock = new();
    private readonly Mock<IOrderItemService> _orderItemServiceMock = new();
    private readonly Mock<IWebhooks> _webhooksMock = new();
    private readonly Mock<IOrderBatchService> _orderBatchServiceMock = new();
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock = new(Mock.Of<IUserStore<IdentityUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
    private readonly Mock<ILogger<OrdersController>> _loggerMock = new();
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.BasicUser),
            };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        ITempDataProvider tempDataProvider = Mock.Of<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        _controller = new OrdersController(
                    _orderServiceMock.Object,
                    _orderTagServiceMock.Object,
                    _orderShippingServiceMock.Object,
                    _orderFulfillmentServiceMock.Object,
                    _shipstationStoreServiceMock.Object,
                    _shippingScanoutServiceMock.Object,
                    _productServiceMock.Object,
                    _departmentServiceMock.Object,
                    _webhooksMock.Object,
                    _orderBatchServiceMock.Object,
                    _userManagerMock.Object,
                    _loggerMock.Object,
                    _orderItemServiceMock.Object
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


    #region Unit Tests for Index

    [Fact]
    public async Task Index_WithValidStoreIdAndProductId_ReturnsViewModelWithCorrectData()
    {
        string orderNumber = "ON123";
        string itemId = "Item new";
        OrderStatus[] orderStatus = [OrderStatus.shipped, OrderStatus.awaiting_payment];
        int storeId = 1;
        int[] productId = [2, 3];
        int[] departmentId = [3, 2];
        string orderStartDate = "2023-01-01";
        string orderEndDate = "2023-12-31";
        int[]? orderTagId = null;

        var mockStoreList = ShipStationStoreFixtures.GetTestShipStationStores();
        var mockProductList = ProductFixtures.GetTestProducts();
        var mockDepartmentList = DepartmentsFixtures.GetTestDepartments();

        _ = _shipstationStoreServiceMock.Setup(static s => s.GetListAsync(It.IsAny<Func<IQueryable<ShipStationStore>, IQueryable<ShipStationStore>>>()))
                                    .ReturnsAsync(mockStoreList);
        _ = _productServiceMock.Setup(static s => s.GetListAsync(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()))
                           .ReturnsAsync(mockProductList);
        _ = _departmentServiceMock.Setup(static s => s.GetListAsync(It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()))
                              .ReturnsAsync(mockDepartmentList);


        var result = await _controller.Index(orderNumber, orderStatus, storeId, orderTagId, productId, departmentId, itemId, orderStartDate, orderEndDate);


        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<OrderViewModel>(viewResult.Model);

        Assert.Equal(orderNumber, model.OrderNumber);
        Assert.Equal(orderStatus, model.OrderStatus);
        Assert.Equal(storeId, model.StoreId);
        Assert.Equal(productId, model.ProductIds);
        Assert.Equal(departmentId, model.DepartmentIds);
        Assert.Equal(orderStartDate, model.OrderStartDate);
        Assert.Equal(orderEndDate, model.OrderEndDate);

        Assert.NotNull(model.StoreNames);
        Assert.NotEmpty(model.StoreNames);
        Assert.Equal(model.StoreNames.Count(), mockStoreList.Count());

        Assert.NotNull(model.Products);
        Assert.NotEmpty(model.Products);
        Assert.Equal(model.Products.Count(), mockProductList.Count());

        Assert.NotNull(model.Departments);
        Assert.NotEmpty(model.Departments);
        Assert.Equal(model.Departments.Count(), mockDepartmentList.Count());
    }


    #endregion


    #region Unit Tests for GetOrders

    [Fact]
    public async Task GetOrders_StandardRequest_ReturnsJsonResult()
    {
        // Arrange
        var mockOrders = OrderFixtures.GetTestOrders();
        var orderNumber = "123";
        OrderStatus[] orderStatus = [OrderStatus.shipped, OrderStatus.awaiting_payment];
        int[] productId = [2, 3];
        int[] departmentId = [3, 2];
        var storeId = 1;
        var orderStartDate = "2023-01-01";
        var orderEndDate = "2023-12-31";
        var shipByDate = "";
        int[]? orderTagId = null;
        string itemName = "New item";

        _ = _orderServiceMock.Setup(static s => s.GetOrdersAsync(
            It.IsAny<int>(), // start
            It.IsAny<int>(), // length
            It.IsAny<List<string>>(), // orderNumbers
            It.IsAny<string>(), // itemName
            It.IsAny<OrderStatus[]>(), // orderStatus
            It.IsAny<int>(), // storeId
            It.IsAny<int[]>(), // productIds
            It.IsAny<int[]>(), // departmentIds
            It.IsAny<int[]>(), // orderTagId
            It.IsAny<string>(), // orderStartDate
            It.IsAny<string>(), // orderEndDate
            It.IsAny<string>(), // shipByDate
            It.IsAny<string>(), // sortColumn
            It.IsAny<string>(), // sortDir
            It.IsAny<int?>(), // orderBatchId
            It.IsAny<List<string>>(), // excludeItemNames
            It.IsAny<bool>() // includeBatchedOrders
        )).ReturnsAsync((mockOrders, mockOrders.Count));

        _ = _orderBatchServiceMock.Setup(static s => s.GetOrderBatchNumbersByOrderIds(It.IsAny<List<int>>()))
            .ReturnsAsync([]);

        // Act
        var result = await _controller.GetOrders(1, 0, 10, orderNumber, itemName, orderStatus, storeId, productId, departmentId, orderTagId, orderStartDate, orderEndDate, shipByDate);

        // Assert
        Assert.Equal(orderStatus, _controller.ViewData["OrderStatus"]);
        Assert.Equal(orderNumber, _controller.ViewData["OrderNumber"]);
        Assert.Equal(storeId, _controller.ViewData["StoreId"]);
        Assert.Equal(productId, _controller.ViewData["ProductIds"]);
        Assert.Equal(departmentId, _controller.ViewData["DepartmentIds"]);
        Assert.Equal(orderStartDate, _controller.ViewData["OrderStartDate"]);
        Assert.Equal(orderEndDate, _controller.ViewData["OrderEndDate"]);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
        var resultObject = JObject.FromObject(jsonResult.Value);
        Assert.Equal(1, resultObject.Value<int>("draw"));
        Assert.Equal(mockOrders.Count, resultObject.Value<int>("recordsTotal"));
        Assert.Equal(mockOrders.Count, resultObject.Value<int>("recordsFiltered"));
        Assert.NotNull(resultObject["data"]);
    }

    [Fact]
    public async Task GetOrders_EmptyParameters_ReturnsJsonResult()
    {
        // Arrange
        var mockOrders = OrderFixtures.GetTestOrders();

        _ = _orderServiceMock.Setup(static s => s.GetOrdersAsync(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<List<string>>(),
            It.IsAny<string>(),
            It.IsAny<OrderStatus[]>(),
            It.IsAny<int>(),
            It.IsAny<int[]>(),
            It.IsAny<int[]>(),
            It.IsAny<int[]>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<List<string>>(),
            It.IsAny<bool>()
        )).ReturnsAsync((mockOrders, mockOrders.Count));

        _ = _orderBatchServiceMock.Setup(static s => s.GetOrderBatchNumbersByOrderIds(It.IsAny<List<int>>()))
            .ReturnsAsync([]);

        // Act
        var result = await _controller.GetOrders(1, 0, 10, "", "", null, 0, null, null, null, null, null, null);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
        var resultObject = JObject.FromObject(jsonResult.Value);
        Assert.Equal(1, resultObject.Value<int>("draw"));
        Assert.Equal(mockOrders.Count, resultObject.Value<int>("recordsTotal"));
        Assert.Equal(mockOrders.Count, resultObject.Value<int>("recordsFiltered"));
        Assert.NotNull(resultObject["data"]);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, int.MaxValue)]
    public async Task GetOrders_BoundaryValues_ReturnsJsonResult(int start, int length)
    {
        // Arrange
        var mockOrders = OrderFixtures.GetTestOrders();
        OrderStatus[] orderStatus = [OrderStatus.shipped, OrderStatus.awaiting_payment];
        int[] productId = [2, 3];
        int[] departmentId = [3, 2];

        if (start < 0 || length <= 0)
        {
            _ = _orderServiceMock.Setup(static s => s.GetOrdersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<OrderStatus[]>(),
                It.IsAny<int>(),
                It.IsAny<int[]>(),
                It.IsAny<int[]>(),
                It.IsAny<int[]>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<List<string>>(),
                It.IsAny<bool>()
            )).ReturnsAsync(([], 0));
        }
        else
        {
            _ = _orderServiceMock.Setup(static s => s.GetOrdersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<OrderStatus[]>(),
                It.IsAny<int>(),
                It.IsAny<int[]>(),
                It.IsAny<int[]>(),
                It.IsAny<int[]>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<List<string>>(),
                It.IsAny<bool>()
            )).ReturnsAsync((mockOrders, mockOrders.Count));

            _ = _orderBatchServiceMock.Setup(static s => s.GetOrderBatchNumbersByOrderIds(It.IsAny<List<int>>()))
                .ReturnsAsync([]);
        }

        // Act
        var result = await _controller.GetOrders(1, start, length, null, "", orderStatus, 1, productId, departmentId, null, null, null, null);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
        var resultObject = JObject.FromObject(jsonResult.Value);
        Assert.Equal(1, resultObject.Value<int>("draw"));
        
        if (start < 0 || length <= 0)
        {
            Assert.Equal(0, resultObject.Value<int>("recordsTotal"));
            Assert.Equal(0, resultObject.Value<int>("recordsFiltered"));
        }
        else
        {
            Assert.Equal(mockOrders.Count, resultObject.Value<int>("recordsTotal"));
            Assert.Equal(mockOrders.Count, resultObject.Value<int>("recordsFiltered"));
            Assert.NotNull(resultObject["data"]);
        }
    }

    #endregion


    #region Unit Tests for GetOrderUpdatesBulk

    [Fact]
    public async Task GetOrderUpdatesBulk_ValidInput_ReturnsRedirectResult()
    {
        string validJson = JsonConvert.SerializeObject(new List<int> { 1, 2, 3 });
        _ = _orderServiceMock.Setup(static s => s.GetListAsync(It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
                         .ReturnsAsync([]);

        _controller.ViewData.Add("orderNumber", "001");
        _controller.ViewData.Add("OrderStatus", OrderStatus.on_hold);
        _controller.ViewData.Add("StoreId", 1);
        _controller.ViewData.Add("ProductId", 1);
        _controller.ViewData.Add("DepartmentId", 1);
        _controller.ViewData.Add("OrderStartDate", "2020-02-01");
        _controller.ViewData.Add("OrderEndDate", "2020-03-01");

        var result = await _controller.GetOrderUpdatesBulk(validJson);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        Assert.NotNull(redirectResult.RouteValues);
        Assert.NotNull(redirectResult.RouteValues["orderNumber"]);
        Assert.NotNull(redirectResult.RouteValues["OrderStatus"]);
        Assert.NotNull(redirectResult.RouteValues["StoreId"]);
        Assert.NotNull(redirectResult.RouteValues["ProductId"]);
        Assert.NotNull(redirectResult.RouteValues["DepartmentId"]);
        Assert.NotNull(redirectResult.RouteValues["OrderStartDate"]);
        Assert.NotNull(redirectResult.RouteValues["OrderEndDate"]);
    }

    [Fact]
    public async Task GetOrderUpdatesBulk_InvalidJsonFormat_ReturnsBadRequest()
    {
        string invalidJson = "invalid json";

        var exception = await Record.ExceptionAsync(() => _controller.GetOrderUpdatesBulk(invalidJson));

        Assert.NotNull(exception);
        _ = Assert.IsType<JsonReaderException>(exception);
        Assert.Contains("Unexpected character encountered while parsing value", exception.Message);
    }

    [Fact]
    public async Task GetOrderUpdatesBulk_NonIntegerValuesInJson_ReturnsBadRequest()
    {
        string nonIntegerJson = JsonConvert.SerializeObject(new List<string> { "a", "b", "c" });

        var exception = await Record.ExceptionAsync(() => _controller.GetOrderUpdatesBulk(nonIntegerJson));

        Assert.NotNull(exception);
        _ = Assert.IsType<JsonReaderException>(exception);
        Assert.Contains("Could not convert string to integer", exception.Message);
    }

    #endregion


    #region Unit Tests for GetRateEstimate

    [Fact]
    public async Task GetRateEstimate_ValidInput_ReturnsJsonResult()
    {
        // Use a consistent order from fixtures
        var validOrder = OrderFixtures.GetTestOrders().First(o => o.orderId == 1234567890);

        _ = _orderShippingServiceMock.Setup(s => s.GetOrderByOrderIdAndKeyCustomSelectAsync(validOrder.orderId, validOrder.orderKey))
                                 .ReturnsAsync(validOrder);

        _ = _orderShippingServiceMock.Setup(s => s.GetRateEstimate(validOrder))
                                 .ReturnsAsync(validOrder);

        var result = await _controller.GetRateEstimate(validOrder);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public async Task GetRateEstimate_InvalidInput_ReturnsViewWithDetails()
    {
        // Use a consistent order from fixtures
        var validOrder = OrderFixtures.GetTestOrders().First(o => o.orderId == 1234567890);

        _ = _orderShippingServiceMock.Setup(s => s.GetOrderByOrderIdAndKeyCustomSelectAsync(validOrder.orderId, validOrder.orderKey))
                                 .ReturnsAsync(validOrder);
        _controller.ModelState.AddModelError("Error", "Invalid input");

        var result = await _controller.GetRateEstimate(validOrder);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(_controller.Details), viewResult.ViewName);

        Assert.NotNull(viewResult.Model);
        _ = Assert.IsType<Order>(viewResult.Model);
    }

    [Fact]
    public async Task GetRateEstimate_InvalidModelState_ReturnsViewWithDetails()
    {
        // Use a consistent order from fixtures
        var validOrder = OrderFixtures.GetTestOrders().First(o => o.orderId == 1234567890);

        _ = _orderShippingServiceMock.Setup(s => s.GetOrderByOrderIdAndKeyCustomSelectAsync(validOrder.orderId, validOrder.orderKey))
                                 .ReturnsAsync(validOrder);
        _controller.ModelState.AddModelError("Error", "Model state error");

        var result = await _controller.GetRateEstimate(validOrder);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(nameof(_controller.Details), viewResult.ViewName);

        Assert.NotNull(viewResult.Model);
        _ = Assert.IsType<Order>(viewResult.Model);
    }

    #endregion


    //#region Unit Tests for GetRateEstimateBulk

    //[Fact]
    //public async Task GetRateEstimateBulk_ValidInput_ReturnsRedirectResult()
    //{
    //    var validJson = JsonConvert.SerializeObject(Enumerable.Range(1, 10).ToList());
    //    _ = _orderServiceMock.Setup(s => s.GetListAsync(It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
    //                     .ReturnsAsync([]);

    //    var result = await _controller.GetRateEstimateBulk(validJson);

    //    var redirectToAction = Assert.IsType<RedirectToActionResult>(result);
    //    Assert.Equal(nameof(_controller.Index), redirectToAction.ActionName);
    //}

    //[Fact]
    //public async Task GetRateEstimateBulk_InvalidJsonFormat_ReturnsBadRequest()
    //{
    //    string invalidJson = "invalid json";

    //    var exception = await Record.ExceptionAsync(() => _controller.GetRateEstimateBulk(invalidJson));

    //    Assert.NotNull(exception);
    //    _ = Assert.IsType<JsonReaderException>(exception);
    //    Assert.Contains("Unexpected character encountered while parsing value", exception.Message);
    //}

    //[Fact]
    //public async Task GetRateEstimateBulk_ProcessesEachOrder()
    //{
    //    var orderIds = Enumerable.Range(1, 5).ToList();
    //    var orders = orderIds.Select(id => new Order { ERPOrderId = id, orderNumber = $"Order{id}" }).ToList();

    //    _ = _orderServiceMock.Setup(s => s.GetListAsync(It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()))
    //                     .ReturnsAsync(orders);

    //    _ = await _controller.GetRateEstimateBulk(JsonConvert.SerializeObject(orderIds));

    //    foreach (var order in orders)
    //    {
    //        _orderShippingServiceMock.Verify(s => s.GetRateEstimate(It.Is<Order>(o => o.ERPOrderId == order.ERPOrderId && o.advancedOptions.labelMessageReference1 == order.orderNumber)), Times.Once);
    //    }
    //}

    //#endregion


    #region Unit Tests for AddOrRemoveTag

    [Fact]
    public async Task AddOrRemoveTag_AddTag_UpdatesOrder()
    {
        // Use fixtures for order and tags
        var order = OrderFixtures.GetTestOrders().First(o => o.ERPOrderId == 3);
        var tagList = OrderTagFixtures.GetTestOrderTags();
        var tagId = tagList.First().tagId;

        _ = _orderServiceMock.Setup(s => s.GetAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<Expression<Func<Order, object>>[]>()))
                         .ReturnsAsync(order);
        _ = _orderTagServiceMock.Setup(s => s.GetAllAsync(null, null))
                            .ReturnsAsync(tagList);
        _ = _orderServiceMock.Setup(w => w.AddOrRemoveShipStationTagAsync(order.orderId, tagId, true))
                     .ReturnsAsync("");

        var result = await _controller.AddOrRemoveTag(order.ERPOrderId, [tagId], true);

        _orderServiceMock.Verify(s => s.UpdateAsync(It.Is<Order>(o => o.Tags.Any(t => t.tagId == tagId))), Times.Once);
    }

    [Fact]
    public async Task AddOrRemoveTag_RemoveTag_UpdatesOrder()
    {
        // Use fixtures for order and tags
        var order = OrderFixtures.GetTestOrders().First(o => o.ERPOrderId == 3);
        var tag = OrderTagFixtures.GetTestOrderTags().First();
        
        // Add the tag to the order first so we can test removing it
        order.Tags = [tag];

        _ = _orderServiceMock.Setup(s => s.GetAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<Expression<Func<Order, object>>[]>()))
                         .ReturnsAsync(order);
        _ = _orderTagServiceMock.Setup(s => s.GetAllAsync(null, null))
                            .ReturnsAsync([tag]);
        _ = _orderServiceMock.Setup(w => w.AddOrRemoveShipStationTagAsync(order.orderId, tag.tagId, false))
                     .ReturnsAsync("");

        var result = await _controller.AddOrRemoveTag(order.ERPOrderId, [tag.tagId], false);

        _orderServiceMock.Verify(s => s.UpdateAsync(It.Is<Order>(o => !o.Tags.Any(t => t.tagId == tag.tagId))), Times.Once);
    }

    [Fact]
    public async Task AddOrRemoveTag_InvalidERPOrderId_ReturnsBadRequest()
    {
        var invalidERPOrderId = -1;

        _ = _orderServiceMock.Setup(s => s.GetAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<Expression<Func<Order, object>>[]>()))
                         .ReturnsAsync((Order?)null);

        var exception = await Record.ExceptionAsync(() => _controller.AddOrRemoveTag(invalidERPOrderId, [1], true));

        Assert.NotNull(exception);
        _ = Assert.IsType<NullReferenceException>(exception);
        Assert.Contains("Object reference not set to an instance of an object", exception.Message);
    }

    #endregion


    #region Unit Tests for AddOrRemoveTagBulk

    [Fact]
    public async Task AddOrRemoveTagBulk_AddTags_UpdatesOrders()
    {
        var validJson = JsonConvert.SerializeObject(new List<int> { 1, 2 });
        var tagIds = new[] { 1 };
        var orders = new List<Order> { new() { ERPOrderId = 1 }, new() { ERPOrderId = 2 } };

        _ = _orderServiceMock.Setup(static x => x.GetAsync(It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>()!))
                 .ReturnsAsync((Order?)null);
        _ = _orderServiceMock.Setup(static x => x.AddOrRemoveShipStationTagAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                     .ReturnsAsync("");

        var result = await _controller.AddOrRemoveTagBulk(validJson, tagIds, true);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task AddOrRemoveTagBulk_InvalidJsonFormat_ReturnsBadRequest()
    {
        string invalidJson = "invalid json";

        var exception = await Record.ExceptionAsync(() => _controller.AddOrRemoveTagBulk(invalidJson, [1], true));

        Assert.NotNull(exception);
        _ = Assert.IsType<JsonReaderException>(exception);
        Assert.Contains("Unexpected character encountered while parsing value", exception.Message);
    }

    [Fact]
    public async Task AddOrRemoveTagBulk_TooManyIterations_ReturnsBadRequest()
    {
        var validJson = JsonConvert.SerializeObject(Enumerable.Range(1, 11).ToList());
        int[] tagIds = Enumerable.Range(1, 10).ToArray();

        var result = await _controller.AddOrRemoveTagBulk(validJson, tagIds, true);

        _ = Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion


    #region Unit Tests for GenerateLabelShipEngine

    [Fact]
    public async Task GenerateLabelShipEngine_LabelGenerationSuccess_ReturnsJsonWithLabel()
    {
        // Arrange
        var inputOrder = OrderFixtures.GetTestOrders().First(o => o.ERPOrderId == 5); // Match with OrderShipmentFixtures ERPOrderId
        var shipment = OrderShipmentFixtures.GetTestOrderShipment().First(s => s.ERPOrderId == inputOrder.ERPOrderId);
        
        // Create a result order with label data
        var resultOrder = OrderFixtures.GetTestOrders().First(o => o.ERPOrderId == inputOrder.ERPOrderId);
        resultOrder.orderShipments = [shipment];

        // Setup mocks
        _ = _orderShippingServiceMock.Setup(service => 
            service.GetOrderByOrderIdAndKeyCustomSelectAsync(inputOrder.orderId, inputOrder.orderKey))
            .ReturnsAsync(inputOrder);
        
        _ = _orderShippingServiceMock.Setup(service => 
            service.GenerateLabelShipEngine(
                It.Is<Order>(o => o.ERPOrderId == inputOrder.ERPOrderId), 
                It.IsAny<string>()!))
            .ReturnsAsync(resultOrder);
        
        _ = _orderBatchServiceMock.Setup(service => 
            service.UpdateOrderBatchItemsStatusToCompletedAsync(inputOrder.orderNumber))
            .Returns(Task.CompletedTask);
        _ = _orderServiceMock.Setup(service =>
        service.GetOrderShipToAddressAsync(resultOrder.orderId, resultOrder.orderKey)).ReturnsAsync(resultOrder.shipTo);

        // Act
        var result = await _controller.GenerateLabelAndShip(inputOrder, null);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
        var resultObj = JObject.FromObject(jsonResult.Value);
        
        Assert.True(resultObj.Value<bool>("success"));
        Assert.Equal(shipment.labelData.Substring("data:application/pdf;base64,".Length), resultObj.Value<string>("labelData"));
        Assert.Equal($"Labels-{inputOrder.orderNumber}.pdf", resultObj.Value<string>("filename"));

        // Verify batch status was updated
        _orderBatchServiceMock.Verify(
            service => service.UpdateOrderBatchItemsStatusToCompletedAsync(inputOrder.orderNumber),
            Times.Once
        );
    }

    [Fact]
    public async Task GenerateLabelShipEngine_LabelGenerationFailure_ReturnsJsonWithError()
    {
        // Arrange
        var inputOrder = OrderFixtures.GetTestOrders().First(o => o.ERPOrderId == 2); // Match with OrderShipmentFixtures ERPOrderId

        // Ensure the mock is properly configured
        _ = _orderShippingServiceMock.Setup(service =>
            service.GetOrderByOrderIdAndKeyCustomSelectAsync(inputOrder.orderId, inputOrder.orderKey))
            .ReturnsAsync(inputOrder);

        _ = _orderShippingServiceMock.Setup(service =>
            service.GenerateLabelShipEngine(
                It.IsAny<Order>()!,
                It.IsAny<string>()!))
            .ThrowsAsync(new HttpRequestException("Label generation failed"));

        // Act
        var result = await _controller.GenerateLabelAndShip(inputOrder, null);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
        var resultObj = JObject.FromObject(jsonResult.Value);
        
        Assert.False(resultObj.Value<bool>("success"));

        // Verify batch status was not updated
        _orderBatchServiceMock.Verify(
            service => service.UpdateOrderBatchItemsStatusToCompletedAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    #endregion


    #region Unit Tests for FindMissingOrder

    [Fact]
    public async Task FindMissingOrder_OrderFound_RedirectsToDetails()
    {
        var orderNumber = "validOrderNumber";
        var storeId = 1;
        var mockOrder = new Order { ERPOrderId = 123, orderNumber = orderNumber };

        _ = _shipstationStoreServiceMock.Setup(s => s.Get(It.IsAny<Expression<Func<ShipStationStore, bool>>>(), null))
                                    .Returns(new ShipStationStore { ShipStationStoreId = storeId });
        _ = _orderServiceMock.Setup(s => s.FindMissingOrder(orderNumber, It.IsAny<int>()))
                         .ReturnsAsync(mockOrder);

        var result = await _controller.FindMissingOrder(orderNumber, storeId, null);

        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Details), redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        Assert.Equal(mockOrder.ERPOrderId, redirectToActionResult.RouteValues["id"]);
    }

    [Fact]
    public async Task FindMissingOrder_InvalidStoreWithValidOrderNumber_ReturnsNotFound()
    {
        var nonExistingStoreId = 999;
        var orderNumber = "validOrderNumber";

        _ = _shipstationStoreServiceMock.Setup(static s => s.Get(It.IsAny<Expression<Func<ShipStationStore, bool>>>(), null))
                                    .Returns((ShipStationStore)null!);

        var result = await _controller.FindMissingOrder(orderNumber, nonExistingStoreId, null);

        _ = Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion


    #region Unit Tests for Details

    [Fact]
    public async Task Details_ValidId_ReturnsViewWithOrder()
    {
        var validId = 3; // Using a valid ERPOrderId from fixtures
        var mockOrder = OrderFixtures.GetTestOrders().First(o => o.ERPOrderId == validId);
        mockOrder.orderStatus = OrderStatus.shipped;
        // Ensure all required objects are initialized to prevent null reference exceptions
        mockOrder.orderShipments = [new() { OrderShipmentId = 1, voided = false }];
        mockOrder.orderFulfillments = [];
        mockOrder.items = mockOrder.items ?? [];
        mockOrder.advancedOptions = mockOrder.advancedOptions ?? new OrderAdvancedOptions();
        mockOrder.weight = mockOrder.weight ?? new OrderWeight();
        
        var shipstationTagList = new List<OrderTag> { new() { tagId = 1, name = "Tag1" } };
        var shippingScanout = new ShippingScanout { CreateDate = DateTime.Now, OrderShipmentId = 1 };
        var storesList = ShipStationStoreFixtures.GetTestShipStationStores();
        var batchNumberList = new Dictionary<int, List<string>> { { mockOrder.ERPOrderId, ["BatchA", "BatchB"] } };
        var orderBatchItem = new OrderBatchItem { 
            OrderBatch = new OrderBatch { 
                CreateDate = DateTime.Now 
            } 
        };

        // Set up all required mocks
        _ = _orderTagServiceMock.Setup(s => s.GetAllAsync(null, null))
            .ReturnsAsync(shipstationTagList);
        _ = _orderServiceMock.Setup(s => s.GetOrderUpdates(It.Is<List<int>>(ids => ids.Contains(validId))))
            .ReturnsAsync([mockOrder]);
        _ = _shippingScanoutServiceMock.Setup(s => s.GetAsync(It.IsAny<Expression<Func<ShippingScanout, bool>>>(), null))
            .ReturnsAsync(shippingScanout);
        _ = _shipstationStoreServiceMock.Setup(s => s.GetAllAsync(null, null))
            .ReturnsAsync(storesList);
        _ = _orderBatchServiceMock.Setup(s => s.GetOrderBatchNumberByOrderId(mockOrder.ERPOrderId))
            .ReturnsAsync(batchNumberList);
        _ = _orderBatchServiceMock.Setup(s => s.GetOrderBatchItemByERPOrderId(validId))
            .ReturnsAsync(orderBatchItem);

        // Execute test
        var result = await _controller.Details(validId);

        // Assert results
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Order>(viewResult.Model);
        Assert.Equal(mockOrder, model);

        Assert.NotNull(_controller.ViewData["ScanOutTime"]);
        Assert.NotEmpty(_controller.ViewData["ScanOutTime"]?.ToString() ?? string.Empty);
        Assert.Equal(shippingScanout.CreateDate.ToString(), _controller.ViewData["ScanOutTime"]);
        
        // Verify BatchNumbers were set in ViewData
        Assert.NotNull(_controller.ViewData["BatchNumbers"]);
        Assert.Equal(batchNumberList[mockOrder.ERPOrderId], _controller.ViewData["BatchNumbers"]);
        
        // Verify BatchTime was set
        Assert.NotNull(_controller.ViewData["BatchTime"]);
        Assert.Equal(orderBatchItem.OrderBatch.CreateDate.ToString(), _controller.ViewData["BatchTime"]);
        
        // Check tags in ViewBag
        Assert.IsAssignableFrom<SelectList>(_controller.ViewBag.Tags);
        var selectList = (SelectList)_controller.ViewBag.Tags;
        Assert.True(selectList.Any());

        var firstTag = selectList.FirstOrDefault(x => x.Value == "1");
        Assert.NotNull(firstTag);
        Assert.Equal("Tag1", firstTag.Text);
    }

    [Fact]
    public async Task Details_NullId_ReturnsNotFound()
    {
        var result = await _controller.Details(null);

        _ = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_InvalidId_ReturnsNotFound()
    {
        var invalidId = 999;
        var shipstationTagList = new List<OrderTag> { new() { tagId = 1, name = "Tag1" } };

        _ = _orderTagServiceMock.Setup(static s => s.GetAllAsync(null, null)).ReturnsAsync(shipstationTagList);
        _ = _orderServiceMock.Setup(static s => s.GetOrderUpdates(It.IsAny<List<int>>()))
                         .ReturnsAsync([]); // Return empty list to simulate order not found

        var result = await _controller.Details(invalidId);

        _ = Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Unit Tests for GetProductBy

    [Fact]
    public async Task GetProductBy_ValidProductId_ReturnsCorrectDTO()
    {
        int productId = 1;
        var product = new Product
        {
            ProductId = productId,
            Sku = "ABC123",
            Description = "Test Product",
            Cost = 10.00M,
            FulfillmentCost = 5.00M,
            LaborCost = 2.00M,
            OverseasCost = 3.00M,
            ProductImages = [new() { IsDefault = true, FileUrl = "image-url" }]
        };

        _ = _productServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), null))
            .ReturnsAsync(product);

        var result = await _controller.GetProductBy(productId);

        var actionResult = Assert.IsType<ActionResult<DuplicateOrderItemDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<DuplicateOrderItemDTO>(okResult.Value);
        Assert.Equal(productId, dto.ProductId);
        Assert.Equal(1, dto.Quantity);
        Assert.Equal("ABC123", dto.Sku);
        Assert.Equal("Test Product", dto.Name);
        Assert.Equal(20.00M, dto.UnitPrice);
        Assert.Equal("image-url", dto.ImageUrl);
        Assert.NotNull(dto.Product);
    }

    [Fact]
    public async Task GetProductBy_ProductWithoutImage_ReturnsDTOWithoutImageUrl()
    {
        int productId = 2;
        var product = new Product
        {
            ProductId = productId,
            Sku = "DEF456",
            Description = "Product Without Image",
            Cost = 15.00M,
            FulfillmentCost = 7.00M,
            LaborCost = 3.00M,
            OverseasCost = 5.00M,
            ProductImages = null
        };

        _ = _productServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), null))
            .ReturnsAsync(product);

        var result = await _controller.GetProductBy(productId);

        var actionResult = Assert.IsType<ActionResult<DuplicateOrderItemDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<DuplicateOrderItemDTO>(okResult.Value);
        Assert.Equal(productId, dto.ProductId);
        Assert.Equal(1, dto.Quantity);
        Assert.Equal("DEF456", dto.Sku);
        Assert.Equal("Product Without Image", dto.Name);
        Assert.Equal(30.00M, dto.UnitPrice);
        Assert.Null(dto.ImageUrl);
        Assert.NotNull(dto.Product);
    }

    [Fact]
    public async Task GetProductBy_InvalidProductId_ReturnsNotFound()
    {
        int invalidProductId = -1;
        _ = _productServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), null))
            .ReturnsAsync((Product?)null);

        var result = await _controller.GetProductBy(invalidProductId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetProductBy_NullProduct_ReturnsNotFound()
    {
        int productId = 1;
        _ = _productServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<Product, bool>>>(), null))
            .ReturnsAsync((Product?)null);

        var result = await _controller.GetProductBy(productId);

        Assert.Null(result);
    }

    #endregion

    #region Unit Tests for VoidAsync

    [Fact]
    public async Task VoidAsync_ValidOrderShipment_RedirectsToDetails()
    {
        var orderShipment = OrderShipmentFixtures.GetTestOrderShipment().First(s => s.ERPOrderId == 3);
        var mockOrderShipment = OrderShipmentFixtures.GetTestOrderShipment().First(s => s.ERPOrderId == 3);
        var mockOrder = OrderFixtures.GetTestOrders().First(o => o.ERPOrderId == 3);

        orderShipment.ERPOrderId = mockOrderShipment.ERPOrderId = mockOrder.ERPOrderId;

        _ = _orderShippingServiceMock.Setup(s => s.GetAsync(It.IsAny<Expression<Func<OrderShipment, bool>>>(), null))
                                 .ReturnsAsync(mockOrderShipment);
        _ = _orderShippingServiceMock.Setup(s => s.VoidAsync(orderShipment.ERPOrderId, It.IsAny<OrderShipment>()))
                                 .ReturnsAsync(mockOrder);

        var result = await _controller.VoidAsync(orderShipment);

        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Details), redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        Assert.Equal(orderShipment.ERPOrderId, redirectToActionResult.RouteValues["id"]);
    }

    [Fact]
    public async Task VoidAsync_OrderShipmentNotFound_ReturnsNotFound()
    {
        var orderShipment = OrderShipmentFixtures.GetTestOrderShipment().First();
        orderShipment.ERPOrderId = 1;

        _ = _orderShippingServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<OrderShipment, bool>>>(), null))
                                 .ReturnsAsync((OrderShipment?)null);

        var result = await _controller.VoidAsync(orderShipment);

        _ = Assert.IsType<NotFoundResult>(result);
        return ;
    }

    #endregion


    #region Unit Tests for VoidFulfillment

    [Fact]
    public async Task VoidFulfillment_ValidOrderFulfillment_RedirectsToDetails()
    {
        // Use a test order from fixtures
        var mockOrder = OrderFixtures.GetTestOrders().First(o => o.ERPOrderId == 3);
        
        // Create order fulfillment test data
        var orderFulfillment = new OrderFulfillment { 
            OrderFulfillmentId = 1, 
            ERPOrderId = mockOrder.ERPOrderId,
            Order = mockOrder
        };
        
        var mockOrderFulfillment = new OrderFulfillment { 
            OrderFulfillmentId = 1, 
            ERPOrderId = mockOrder.ERPOrderId,
            Order = mockOrder
        };

        _ = _orderFulfillmentServiceMock.Setup(s => s.GetAsync(It.IsAny<Expression<Func<OrderFulfillment, bool>>>(), null))
                                    .ReturnsAsync(mockOrderFulfillment);
        _ = _orderFulfillmentServiceMock.Setup(s => s.VoidAsync(mockOrderFulfillment.ERPOrderId, It.IsAny<OrderFulfillment>()))
                                    .ReturnsAsync(mockOrder);

        var result = await _controller.VoidAsync(orderFulfillment);

        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.Details), redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        Assert.Equal(orderFulfillment.ERPOrderId, redirectToActionResult.RouteValues["id"]);
    }

    [Fact]
    public async Task VoidFulfillment_InvalidOrderFulfillment_ReturnsBadRequest()
    {
        // Use a test order from fixtures
        var mockOrder = OrderFixtures.GetTestOrders().First(static o => o.ERPOrderId == 3);
        
        var orderFulfillment = new OrderFulfillment { 
            OrderFulfillmentId = 1, 
            ERPOrderId = mockOrder.ERPOrderId 
        };
        
        var mockOrderFulfillment = new OrderFulfillment { 
            OrderFulfillmentId = 1, 
            ERPOrderId = mockOrder.ERPOrderId + 100 // Different ERPOrderId to trigger bad request
        };

        _ = _orderFulfillmentServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<OrderFulfillment, bool>>>(), null))
                                    .ReturnsAsync(mockOrderFulfillment);

        var result = await _controller.VoidAsync(orderFulfillment);

        _ = Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task VoidFulfillment_OrderFulfillmentNotFound_ReturnsNotFound()
    {
        // Use a test order from fixtures
        var mockOrder = OrderFixtures.GetTestOrders().First(static o => o.ERPOrderId == 3);
        
        var orderFulfillment = new OrderFulfillment { 
            OrderFulfillmentId = 999, 
            ERPOrderId = mockOrder.ERPOrderId 
        };

        _ = _orderFulfillmentServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<OrderFulfillment, bool>>>(), null))
                                    .ReturnsAsync((OrderFulfillment?)null);

        var result = await _controller.VoidAsync(orderFulfillment);

        _ = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task VoidFulfillment_DbUpdateConcurrencyException_ReturnsConflict()
    {
        // Use a test order from fixtures
        var mockOrder = OrderFixtures.GetTestOrders().First(static o => o.ERPOrderId == 3);
        
        var orderFulfillment = new OrderFulfillment { 
            OrderFulfillmentId = 1, 
            ERPOrderId = mockOrder.ERPOrderId 
        };
        
        var mockOrderFulfillment = new OrderFulfillment { 
            OrderFulfillmentId = 1, 
            ERPOrderId = mockOrder.ERPOrderId 
        };

        _ = _orderFulfillmentServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<OrderFulfillment, bool>>>(), null))
                                    .ReturnsAsync(mockOrderFulfillment);
        _ = _orderFulfillmentServiceMock.Setup(static s => s.VoidAsync(It.IsAny<int>(), It.IsAny<OrderFulfillment>()))
                                    .ThrowsAsync(new DbUpdateConcurrencyException());

        var result = await _controller.VoidAsync(orderFulfillment);

        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("A concurrency conflict occurred while voiding the fulfillment.", conflictResult.Value);
    }

    [Fact]
    public async Task VoidFulfillment_GeneralException_ReturnsInternalServerError()
    {
        // Use a test order from fixtures
        var mockOrder = OrderFixtures.GetTestOrders().First(static o => o.ERPOrderId == 3);
        
        var orderFulfillment = new OrderFulfillment { 
            OrderFulfillmentId = 1, 
            ERPOrderId = mockOrder.ERPOrderId 
        };
        
        var mockOrderFulfillment = new OrderFulfillment { 
            OrderFulfillmentId = 1, 
            ERPOrderId = mockOrder.ERPOrderId 
        };

        _ = _orderFulfillmentServiceMock.Setup(static s => s.GetAsync(It.IsAny<Expression<Func<OrderFulfillment, bool>>>(), null))
                                    .ReturnsAsync(mockOrderFulfillment);
        _ = _orderFulfillmentServiceMock.Setup(static s => s.VoidAsync(It.IsAny<int>(), It.IsAny<OrderFulfillment>()))
                                    .ThrowsAsync(new Exception("Test exception"));

        var result = await _controller.VoidAsync(orderFulfillment);

        var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, internalServerErrorResult.StatusCode);
        Assert.Equal("An error occurred while voiding the fulfillment.", internalServerErrorResult.Value);
    }

    #endregion

    #region Unit Tests for Batch Creation & Addition
    [Fact]
    public async Task BatchCreation_SuccessfulCreation_ReturnsSuccessJsonResult()
    {
        // Arrange  
        var cwaOrderIdsJson = JsonConvert.SerializeObject(new List<int> { 1, 2, 3 });
        var batchType = 1;
        var batchName = "Test Batch";
        var assignedDepartmentsJson = JsonConvert.SerializeObject(new List<AssignedDepartment>());
        var replacementSkusJson = JsonConvert.SerializeObject(new List<ReplacementSku>());
        var isDeductible = true;

        var orders = new List<Order>
            {
                new() { orderId = 1, items = [], advancedOptions = new OrderAdvancedOptions { storeId = 1 } }
            };

        _ = _orderBatchServiceMock.Setup(static s => s.GetOrdersWithProductsByERPOrderIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(orders);
        _ = _orderBatchServiceMock.Setup(static s => s.CreateBatchAsync(It.IsAny<List<int>>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<AssignedDepartment>>(), It.IsAny<List<ReplacementSku>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new BatchCreationResult { Success = true, Message = "Batch created successfully", CompleteBatchNumber = "Batch123" });

        // Act  
        var result = await _controller.BatchCreation(cwaOrderIdsJson, batchType, batchName, assignedDepartmentsJson, replacementSkusJson, isDeductible);

        // Assert  
        var jsonResult = Assert.IsType<JsonResult>(result);
        if (jsonResult.Value == null)
        {
            throw new InvalidOperationException("JsonResult.Value is null.");
        }
        var jsonData = JObject.FromObject(jsonResult.Value);
        Assert.Equal("success", jsonData["status"]?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task BatchCreation_MissingSkus_ReturnsMissingSkusJsonResult()
    {
        // Arrange  
        var cwaOrderIdsJson = JsonConvert.SerializeObject(new List<int> { 1, 2, 3 });
        var batchType = 1;
        var batchName = "Test Batch";
        var assignedDepartmentsJson = JsonConvert.SerializeObject(new List<AssignedDepartment>());
        var replacementSkusJson = JsonConvert.SerializeObject(new List<ReplacementSku>());
        var isDeductible = true;

        var orders = new List<Order>
            {
                new() { orderId = 1, items = [], advancedOptions = new OrderAdvancedOptions { storeId = 1 } }
            };

        _ = _orderBatchServiceMock.Setup(static s => s.GetOrdersWithProductsByERPOrderIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(orders);
        _ = _orderBatchServiceMock.Setup(static s => s.CreateBatchAsync(It.IsAny<List<int>>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<AssignedDepartment>>(), It.IsAny<List<ReplacementSku>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new BatchCreationResult { Success = false, MissingSkus = [new MissingSkuEntry { Sku = "SKU1" }, new MissingSkuEntry { Sku = "SKU2" }] });

        // Act  
        var result = await _controller.BatchCreation(cwaOrderIdsJson, batchType, batchName, assignedDepartmentsJson, replacementSkusJson, isDeductible);

        // Assert  
        
        var jsonResult = Assert.IsType<JsonResult>(result);
        if (jsonResult.Value == null)
        {
            throw new InvalidOperationException("JsonResult.Value is null.");
        }
        var jsonData = JObject.FromObject(jsonResult.Value);
        Assert.Equal("missing_skus", jsonData["status"]?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task BatchCreation_UnassignedDepartments_ReturnsUnassignedDepartmentsJsonResult()
    {
        // Arrange  
        var cwaOrderIdsJson = JsonConvert.SerializeObject(new List<int> { 1, 2, 3 });
        var batchType = 1;
        var batchName = "Test Batch";
        var assignedDepartmentsJson = JsonConvert.SerializeObject(new List<AssignedDepartment>());
        var replacementSkusJson = JsonConvert.SerializeObject(new List<ReplacementSku>());
        var isDeductible = true;

        var orders = new List<Order>
            {
                new() { orderId = 1, items = [], advancedOptions = new OrderAdvancedOptions { storeId = 1 } }
            };

        _ = _orderBatchServiceMock.Setup(static s => s.GetOrdersWithProductsByERPOrderIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(orders);
        _ = _orderBatchServiceMock.Setup(static s => s.CreateBatchAsync(It.IsAny<List<int>>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<AssignedDepartment>>(), It.IsAny<List<ReplacementSku>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new BatchCreationResult { Success = false, UnassignedDepartments = [1, 2] });

        // Act  
        var result = await _controller.BatchCreation(cwaOrderIdsJson, batchType, batchName, assignedDepartmentsJson, replacementSkusJson, isDeductible);

        // Assert  
        var jsonResult = Assert.IsType<JsonResult>(result);
        var jsonData = JObject.FromObject(jsonResult.Value);
        Assert.Equal("unassigned_departments", (string)(jsonData["status"] ?? string.Empty));
        var unassignedDepartments = jsonData["unassignedDepartments"]?.ToObject<List<int>>();
        Assert.NotNull(unassignedDepartments);
        Assert.Equal([1, 2], unassignedDepartments);
    }

    [Fact]
    public async Task BatchCreation_GeneralError_ReturnsErrorJsonResult()
    {
        // Arrange  
        var cwaOrderIdsJson = JsonConvert.SerializeObject(new List<int> { 1, 2, 3 });
        var batchType = 1;
        var batchName = "Test Batch";
        var assignedDepartmentsJson = JsonConvert.SerializeObject(new List<AssignedDepartment>());
        var replacementSkusJson = JsonConvert.SerializeObject(new List<ReplacementSku>());
        var isDeductible = true;

        var orders = new List<Order>
            {
                new() { orderId = 1, items = [], advancedOptions = new OrderAdvancedOptions { storeId = 1 } }
            };

        _ = _orderBatchServiceMock.Setup(static s => s.GetOrdersWithProductsByERPOrderIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(orders);
        _ = _orderBatchServiceMock.Setup(static s => s.CreateBatchAsync(It.IsAny<List<int>>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<AssignedDepartment>>(), It.IsAny<List<ReplacementSku>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new BatchCreationResult { Success = false, Message = "An error occurred" });

        // Act  
        var result = await _controller.BatchCreation(cwaOrderIdsJson, batchType, batchName, assignedDepartmentsJson, replacementSkusJson, isDeductible);

        // Assert  
        var jsonResult = Assert.IsType<JsonResult>(result);
        if (jsonResult.Value == null)
        {
            throw new InvalidOperationException("JsonResult.Value is null.");
        }
        var jsonData = JObject.FromObject(jsonResult.Value);
        Assert.Equal("error", jsonData["status"]?.ToString() ?? string.Empty);
        Assert.Equal("An error occurred", jsonData["message"]?.ToString() ?? string.Empty);
    }
    [Fact]
    public async Task AddOrdersToBatch_SuccessfulAddition_ReturnsSuccessJsonResult()
    {
        // Arrange  
        var cwaOrderIdsJson = JsonConvert.SerializeObject(new List<int> { 1, 2, 3 });
        var batchId = 1;
        var assignedDepartmentsJson = JsonConvert.SerializeObject(new List<AssignedDepartment>());
        var replacementSkusJson = JsonConvert.SerializeObject(new List<ReplacementSku>());

        var orders = new List<Order>
        {
            new() { orderId = 1, items = [], advancedOptions = new OrderAdvancedOptions { storeId = 1 } }
        };

        _ = _orderBatchServiceMock.Setup(static s => s.GetOrdersWithProductsByERPOrderIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(orders);
        _ = _orderItemServiceMock.Setup(static s => s.AssignProductIds(It.IsAny<List<OrderItem>>())).ReturnsAsync([]);
        _ = _orderBatchServiceMock.Setup(static s => s.AddOrdersToBatchAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<List<AssignedDepartment>>(), It.IsAny<List<ReplacementSku>>()))
            .ReturnsAsync(new BatchOperationResult { Success = true, Message = "Orders added successfully" });

        // Act  
        var result = await _controller.AddOrdersToBatch(cwaOrderIdsJson, batchId, assignedDepartmentsJson, replacementSkusJson);

        // Assert  
        var jsonResult = Assert.IsType<JsonResult>(result);
        if (jsonResult.Value == null)
        {
            throw new InvalidOperationException("JsonResult.Value is null.");
        }
        var jsonData = JObject.FromObject(jsonResult.Value);
        Assert.Equal("success", jsonData["status"]?.ToString() ?? string.Empty);
        Assert.Equal("Orders added successfully", jsonData["message"]?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task AddOrdersToBatch_MissingSkus_ReturnsMissingSkusJsonResult()
    {
        // Arrange  
        var cwaOrderIdsJson = JsonConvert.SerializeObject(new List<int> { 1, 2, 3 });
        var batchId = 1;
        var assignedDepartmentsJson = JsonConvert.SerializeObject(new List<AssignedDepartment>());
        var replacementSkusJson = JsonConvert.SerializeObject(new List<ReplacementSku>());

        var orders = new List<Order>
        {
            new() { orderId = 1, items = [], advancedOptions = new OrderAdvancedOptions { storeId = 1 } }
        };

        _ = _orderBatchServiceMock.Setup(static s => s.GetOrdersWithProductsByERPOrderIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(orders);
        _ = _orderItemServiceMock.Setup(static s => s.AssignProductIds(It.IsAny<List<OrderItem>>())).ReturnsAsync([]);
        _ = _orderBatchServiceMock.Setup(static s => s.AddOrdersToBatchAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<List<AssignedDepartment>>(), It.IsAny<List<ReplacementSku>>()))
            .ReturnsAsync(new BatchOperationResult { Success = false, MissingSkus = [new MissingSkuEntry { Sku = "SKU1" }, new MissingSkuEntry { Sku = "SKU2" }] });

        // Act  
        var result = await _controller.AddOrdersToBatch(cwaOrderIdsJson, batchId, assignedDepartmentsJson, replacementSkusJson);

        // Assert  
        var jsonResult = Assert.IsType<JsonResult>(result);
        if (jsonResult.Value == null)
        {
            throw new InvalidOperationException("JsonResult.Value is null.");
        }
        var jsonData = JObject.FromObject(jsonResult.Value);
        Assert.Equal("missing_skus", jsonData["status"]?.ToString() ?? string.Empty);
        Assert.Equal(["SKU1", "SKU2"], jsonData["missingSkus"]?.Select(static x => x.SelectToken("Sku")?.ToString()).ToList());
    }

    [Fact]
    public async Task AddOrdersToBatch_UnassignedDepartments_ReturnsUnassignedDepartmentsJsonResult()
    {
        // Arrange  
        var cwaOrderIdsJson = JsonConvert.SerializeObject(new List<int> { 1, 2, 3 });
        var batchId = 1;
        var assignedDepartmentsJson = JsonConvert.SerializeObject(new List<AssignedDepartment>());
        var replacementSkusJson = JsonConvert.SerializeObject(new List<ReplacementSku>());

        var orders = new List<Order>
        {
            new() { orderId = 1, items = [], advancedOptions = new OrderAdvancedOptions { storeId = 1 } }
        };

        _ = _orderBatchServiceMock.Setup(static s => s.GetOrdersWithProductsByERPOrderIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(orders);
        _ = _orderItemServiceMock.Setup(static s => s.AssignProductIds(It.IsAny<List<OrderItem>>())).ReturnsAsync([]);
        _ = _orderBatchServiceMock.Setup(static s => s.AddOrdersToBatchAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<List<AssignedDepartment>>(), It.IsAny<List<ReplacementSku>>()))
            .ReturnsAsync(new BatchOperationResult { Success = false, UnassignedDepartments = [1, 2] });

        // Act  
        var result = await _controller.AddOrdersToBatch(cwaOrderIdsJson, batchId, assignedDepartmentsJson, replacementSkusJson);

        // Assert  
        var jsonResult = Assert.IsType<JsonResult>(result);
        if (jsonResult.Value == null)
        {
            throw new InvalidOperationException("JsonResult.Value is null.");
        }
        var jsonData = JObject.FromObject(jsonResult.Value);
        Assert.Equal("unassigned_departments", jsonData["status"]?.ToString() ?? string.Empty);
        var unassignedDepartments = jsonData["unassignedDepartments"]?.ToObject<List<int>>();
        Assert.NotNull(unassignedDepartments);
        Assert.Equal([1, 2], unassignedDepartments);
    }

    [Fact]
    public async Task AddOrdersToBatch_GeneralError_ReturnsErrorJsonResult()
    {
        // Arrange  
        var cwaOrderIdsJson = JsonConvert.SerializeObject(new List<int> { 1, 2, 3 });
        var batchId = 1;
        var assignedDepartmentsJson = JsonConvert.SerializeObject(new List<AssignedDepartment>());
        var replacementSkusJson = JsonConvert.SerializeObject(new List<ReplacementSku>());

        var orders = new List<Order>
        {
            new() { orderId = 1, items = [], advancedOptions = new OrderAdvancedOptions { storeId = 1 } }
        };

        _ = _orderBatchServiceMock.Setup(static s => s.GetOrdersWithProductsByERPOrderIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(orders);
        _ = _orderItemServiceMock.Setup(static s => s.AssignProductIds(It.IsAny<List<OrderItem>>())).ReturnsAsync([]);
        _ = _orderBatchServiceMock.Setup(static s => s.AddOrdersToBatchAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<List<AssignedDepartment>>(), It.IsAny<List<ReplacementSku>>()))
            .ReturnsAsync(new BatchOperationResult { Success = false, Message = "An error occurred" });

        // Act  
        var result = await _controller.AddOrdersToBatch(cwaOrderIdsJson, batchId, assignedDepartmentsJson, replacementSkusJson);

        // Assert  
        var jsonResult = Assert.IsType<JsonResult>(result);
        if (jsonResult.Value == null)
        {
            throw new InvalidOperationException("JsonResult.Value is null.");
        }
        var jsonData = JObject.FromObject(jsonResult.Value);
        Assert.Equal("error", jsonData["status"]?.ToString() ?? string.Empty);
        Assert.Equal("An error occurred", jsonData["message"]?.ToString() ?? string.Empty);
    }
    #endregion
}