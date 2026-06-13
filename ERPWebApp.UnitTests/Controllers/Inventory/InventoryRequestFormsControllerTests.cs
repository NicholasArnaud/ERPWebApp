using ERPWebApp.Controllers.Inventory;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers.Inventory
{
    [Trait("Category", "execute")]
    public class InventoryRequestFormsControllerTests
    {
        private readonly Mock<ISubCategoryService> _subCategoryServiceMock = new();
        private readonly Mock<IEmployeeService> _employeeServiceMock = new();
        private readonly Mock<IProductService> _productServiceMock = new();
        private readonly Mock<IStocksService> _stocksServiceMock = new();
        private readonly Mock<IInventoryRequestFormService> _inventoryRequestFormServiceMock = new();
        private readonly Mock<ILocationService> _locationServiceMock = new();
        private readonly Mock<IOrderService> _orderServiceMock = new();

        private readonly InventoryRequestFormsController _controller;
        private readonly ITempDataDictionary tempData;
        private readonly ClaimsPrincipal claimsPrincipal;

        public InventoryRequestFormsControllerTests()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.Administrator),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            claimsPrincipal = new ClaimsPrincipal(identity);
            ITempDataProvider tempDataProvider = Mock.Of<ITempDataProvider>();
            TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
            tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());
            _controller = new InventoryRequestFormsController(
                _subCategoryServiceMock.Object,
                _employeeServiceMock.Object,
                _productServiceMock.Object,
                _stocksServiceMock.Object,
                _inventoryRequestFormServiceMock.Object,
                _locationServiceMock.Object,
                _orderServiceMock.Object
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
        public void Index_ReturnsViewResult_WithCorrectData()
        {
            // Arrange
            var subCategoryList = SubCategoriesFixtures.GetTestSubCategories();

            _ = _subCategoryServiceMock.Setup(static s => s.GetList(
                It.IsAny<Func<IQueryable<SubCategory>, IQueryable<SelectListItem>>>()
            )).Returns(
                subCategoryList.Select(static x => new SelectListItem
                {
                    Value = x.SubCategoryId.ToString(),
                    Text = x.Description
                }).ToList()
            );

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            var actual = result?.ViewData["permission"] as string;
            //actual.Should().Be("yes");
            var subCategoryListInViewData = result?.ViewData["SubCategoryList"] as SelectList;
            Assert.NotNull(subCategoryListInViewData);
            Assert.Equal(subCategoryList.Count(), subCategoryListInViewData.Count());
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsViewResult()
        {
            // Arrange
            var expectedInventoryRequestForm = new InventoryRequestForm { InventoryRequestFormId = 1 };
            _ = _inventoryRequestFormServiceMock
                .Setup(static s => s.GetAsync(
                    It.IsAny<Expression<Func<InventoryRequestForm, bool>>>(),
                    It.IsAny<Expression<Func<InventoryRequestForm, object>>[]>()
                ))
                .ReturnsAsync(expectedInventoryRequestForm);

            // Act
            var result = await _controller.Details(1);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<InventoryRequestForm>(viewResult.ViewData.Model);
            Assert.Equal(expectedInventoryRequestForm, model);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            _ = _inventoryRequestFormServiceMock
                .Setup(static s => s.GetAsync(
                    It.IsAny<Expression<Func<InventoryRequestForm, bool>>>(),
                    It.IsAny<Expression<Func<InventoryRequestForm, object>>[]>()
                ))
                .ReturnsAsync((InventoryRequestForm?)null);

            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.NotNull(result);
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsViewResult_WithCorrectData()
        {
            // Arrange
            var expectedEmployees = EmployeeFixtures.GetTestEmployeeices();

            var selectListEmployees = expectedEmployees
                .Select(static e => new SelectListItem
                {
                    Value = e.EmployeeId.ToString(),
                    Text = e.FullName + " - " + e.EmployeeReferenceNumber
                });
            _ = _employeeServiceMock
                .Setup(static s => s.GetListAsync<SelectListItem>(
                   It.IsAny<Func<IQueryable<Employee>, IQueryable<SelectListItem>>>()
                )).ReturnsAsync([.. selectListEmployees]);

            var expectedProducts = new List<Product>
            {
                new() { ProductId = 1, Sku = "ABC", Description = "Product 1", IsActive = true },
                new() { ProductId = 2, Sku = "DEF", Description = "Product 2", IsActive = true }
            };

            _ = _locationServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Expression<Func<Location, string>>[]>(),
                It.IsAny<Expression<Func<Location, object>>[]>()
            )).ReturnsAsync([]);

            // Act
            var result = await _controller.Create() as ViewResult;

            // Assert
            Assert.NotNull(result);

            Assert.NotNull(result.ViewData["RequestedEmployeeId"]);
            var requestedEmployeeIdSelectList = Assert.IsType<SelectList>(result.ViewData["RequestedEmployeeId"]);
            Assert.Equal(selectListEmployees.Count(), requestedEmployeeIdSelectList.Count());

            Assert.NotNull(result.ViewData["PickedEmployeeId"]);
            var pickedEmployeeIdSelectList = Assert.IsType<SelectList>(result.ViewData["PickedEmployeeId"]);
            Assert.Equal(selectListEmployees.Count(), pickedEmployeeIdSelectList.Count());

        }

        [Fact]
        public async Task Create_WithValidInventoryRequestForm_RedirectsToIndex()
        {
            // Arrange

            var inventoryRequestForm = new InventoryRequestForm
            {
                InventoryRequestFormId = 1,
                ProductId = 1,
                QuantityNeeded = 10,
                PickReason = "Reason",
                RequestedByEmployeeId = 1,
                OrderNumber = "Order123",
                ToLocationId = 1
            };

            _inventoryRequestFormServiceMock
                .Setup(s => s.AddAsync(inventoryRequestForm))
                .Verifiable();

            _controller.ModelState.Clear();

            // Act
            var result = await _controller.Create(inventoryRequestForm);

            // Assert
            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Null(redirectToActionResult.ControllerName);
            _inventoryRequestFormServiceMock.Verify(d => d.AddAsync(inventoryRequestForm), Times.Once);
        }

        [Fact]
        public async Task Edit_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var inventoryRequestForm = new InventoryRequestForm
            {
                InventoryRequestFormId = 1,
                ProductId = 1,
                QuantityNeeded = 10,
                RequestedByEmployeeId = 1
            };

            // Act
            var result = await _controller.Edit(2, inventoryRequestForm);

            // Assert
            Assert.NotNull(result);
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WithValidInventoryRequestForm_RedirectsToIndex()
        {
            // Arrange

            var inventoryRequestForm = new InventoryRequestForm
            {
                InventoryRequestFormId = 1,
                ProductId = 1,
                QuantityNeeded = 10,
                RequestedByEmployeeId = 1,
                CreatedDate = DateTime.Now,
                PickReason = "Reason",
                IsPicked = true,
                PickedByEmployeeId = 2,
                PickedDate = DateTime.Now,
                IsFromExtrasLocation = false,
                StockId = 1,
                FromLocation = "Stock Location",
                IsReceived = false,
                ReceivedDate = DateTime.Now
            };

            _ = _stocksServiceMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Stock, bool>>>(),
                It.IsAny<Expression<Func<Stock, object>>[]>()
            )).ReturnsAsync(StockFixtures.GetTestStocks().First());

            _ = _locationServiceMock.Setup(x => x.GetAsync(
               It.IsAny<Expression<Func<Location, bool>>>(),
               It.IsAny<Expression<Func<Location, object>>[]>()
           )).ReturnsAsync(LocationFixtures.GetTestLocations().First());

            _inventoryRequestFormServiceMock
                .Setup(s => s.UpdateAsync(inventoryRequestForm))
                .Verifiable();

            _controller.ModelState.Clear();

            // Act
            var result = await _controller.Edit(1, inventoryRequestForm);

            // Assert
            Assert.NotNull(result);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Null(redirectToActionResult.ControllerName);
            _inventoryRequestFormServiceMock.Verify();
        }

        [Fact]
        public async Task Edit_WithValidInventoryRequestForm_WhileUpdateExceptionThrows_RedirectsNotFound()
        {
            // Arrange

            var inventoryRequestForm = new InventoryRequestForm
            {
                InventoryRequestFormId = 1,
                ProductId = 1,
                QuantityNeeded = 10,
                RequestedByEmployeeId = 1,
                CreatedDate = DateTime.Now,
                PickReason = "Reason",
                IsPicked = true,
                PickedByEmployeeId = 2,
                PickedDate = DateTime.Now,
                IsFromExtrasLocation = false,
                StockId = 1,
                FromLocation = "Stock Location",
                IsReceived = false,
                ReceivedDate = DateTime.Now
            };

            _ = _stocksServiceMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Stock, bool>>>(),
                It.IsAny<Expression<Func<Stock, object>>[]>()
            )).ReturnsAsync(StockFixtures.GetTestStocks().First());

            _ = _locationServiceMock.Setup(x => x.GetAsync(
               It.IsAny<Expression<Func<Location, bool>>>(),
               It.IsAny<Expression<Func<Location, object>>[]>()
           )).ReturnsAsync(LocationFixtures.GetTestLocations().First());

            _ = _inventoryRequestFormServiceMock.Setup(x => x.IsExists(It.IsAny<Expression<Func<InventoryRequestForm, bool>>>())).Returns(false);

            _ = _inventoryRequestFormServiceMock
                .Setup(s => s.UpdateAsync(inventoryRequestForm))
                .Throws<DbUpdateConcurrencyException>();

            _controller.ModelState.Clear();

            // Act
            var result = await _controller.Edit(1, inventoryRequestForm);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsViewResult()
        {
            // Arrange
            int id = 1;
            var inventoryRequestForm = new InventoryRequestForm { InventoryRequestFormId = id };
            _ = _inventoryRequestFormServiceMock
                .Setup(static service => service.GetAsync(
                    It.IsAny<Expression<Func<InventoryRequestForm, bool>>>(),
                    It.IsAny<Expression<Func<InventoryRequestForm, object>>[]>()
                    )).ReturnsAsync(inventoryRequestForm);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            _ = Assert.IsType<ViewResult>(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(inventoryRequestForm, viewResult.Model);
        }

        [Fact]
        public async Task Delete_WithNullId_ReturnsNotFoundResult()
        {
            // Arrange
            int? id = null;

            // Act
            var result = await _controller.Delete(id);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            int id = 1;
            _ = _inventoryRequestFormServiceMock
                .Setup(static service => service.GetAsync(
                     It.IsAny<Expression<Func<InventoryRequestForm, bool>>>(),
                    It.IsAny<Expression<Func<InventoryRequestForm, object>>[]>()
                    )).ReturnsAsync((InventoryRequestForm?)null);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ValidId_RedirectsToIndex()
        {
            // Arrange
            var id = 123; // Specify a valid id

            // Act
            var result = await _controller.DeleteConfirmed(id);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task CloseInventoryRequestForm_ReturnsNotFound_WhenInventoryRequestFormIdIsNull()
        {
            // Arrange
            int? inventoryRequestFormId = null;

            // Act
            var result = await _controller.CloseInventoryRequestForm(inventoryRequestFormId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CloseInventoryRequestForm_ReturnsNotFound_WhenSelectedInventoryRequestFormIsNull()
        {
            // Arrange
            int inventoryRequestFormId = 1;
            _ = _inventoryRequestFormServiceMock.Setup(static x => x.QueryFilter(
                It.IsAny<Func<IQueryable<InventoryRequestForm>, IQueryable<InventoryRequestForm>>>()
                )).Returns(Enumerable.Empty<InventoryRequestForm>().AsQueryable());

            // Act
            var result = await _controller.CloseInventoryRequestForm(inventoryRequestFormId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void LoadTable_ReturnsOkResultWithJsonData()
        {
            // Arrange
            var formCollectionMock = new Mock<IFormCollection>();
            _ = formCollectionMock.Setup(static f => f["draw"]).Returns("1");
            _ = formCollectionMock.Setup(static f => f["start"]).Returns("0");
            _ = formCollectionMock.Setup(static f => f["length"]).Returns("10");

            _controller.ControllerContext.HttpContext.Request.Form = formCollectionMock.Object;

            _ = _inventoryRequestFormServiceMock.Setup(static x => x.QueryFilter(
               It.IsAny<Func<IQueryable<InventoryRequestForm>, IQueryable<InventoryRequestForm>>>()
               )).Returns(InventoryRequestFormFixtures.GetTestFiles().AsQueryable());

            // Act
            var result = _controller.LoadTable("12345", 1, "2023-05-07 - 2023-05-10");

            // Assert
            _ = Assert.IsType<OkObjectResult>(result);

            var okResult = (OkObjectResult)result;
            Assert.NotNull(okResult.Value);
            dynamic jsonData = okResult.Value;
        }
    }
}