using ERPWebApp.Controllers;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Mappings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace ERPWebApp.UnitTests.Controllers
{
    [Trait("Category", "execute")]
    public class ShipStationStoresControllerTests
    {
        private readonly Mock<IShipStationStoreService> _shipStationStoreServiceMock;
        private readonly ShipStationStoresController _controller;
        private readonly Mock<IWebhooks> _webhooksMock;
        private readonly Mock<ILogger<ShipStationStoresController>> _loggerMock;
        public ShipStationStoresControllerTests()
        {
            _shipStationStoreServiceMock = new Mock<IShipStationStoreService>();
            _loggerMock = new Mock<ILogger<ShipStationStoresController>>();
            _webhooksMock = new Mock<IWebhooks>();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.BasicUser),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller = new ShipStationStoresController(_shipStationStoreServiceMock.Object, _loggerMock.Object, _webhooksMock.Object)
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
        public async Task Index_Returns_View_With_StoreList()
        {
            // Arrange
            var expectedStoreList = new List<ShipStationStore>
            {
                new() { /* Add necessary properties */ },
                new() { /* Add necessary properties */ }
            };

            _ = _shipStationStoreServiceMock.Setup(static x => x.GetAllAsync(
                It.IsAny<Expression<Func<ShipStationStore, string>>[]>(),
                It.IsAny<Expression<Func<ShipStationStore, object>>[]>()))
                .ReturnsAsync(expectedStoreList);


            // Act
            var result = await _controller.Index() as ViewResult;
            var model = result?.Model as List<ShipStationStore>;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(expectedStoreList.Count, model?.Count);
        }

        [Fact]
        public async Task Index_Returns_BadRequest_On_Service_Failure()
        {
            // Arrange
            _ = _shipStationStoreServiceMock.Setup(x => x.GetAllAsync(
                    It.IsAny<Expression<Func<ShipStationStore, string>>[]>(),
                    It.IsAny<Expression<Func<ShipStationStore, object>>[]>()))
                    .ThrowsAsync(new Exception("Simulated service failure"));

            // Act
            _ = await Assert.ThrowsAsync<Exception>(async () => await _controller.Index());
        }

        [Fact]
        public async Task Details_ValidId_ReturnsView()
        {
            // Arrange
            var dummyStore = new ShipStationStore
            {
                ShipStationStoreId = 1,
                StoreId = 1001,
                StoreName = "Sample Store 1",
                Email = "sample1@store.com",
                PublicEmail = "public.sample1@store.com",
                IsActive = true,
                HasIncreasedPricing = false,
                StoreType = StoreType.AMAZON,
                StoreFiles =
                [
                        new() {StoreFileId = 1,ShipStationStoreId = 1001,  FileId = 1, Files =
                             new Files
                             {
                                     FileId = 1,
                                     FileName = "example_file_1.jpg",
                                     ContentType = "image/jpeg",
                                     Content = Encoding.UTF8.GetBytes("Sample file content 1"),
                                     FileType = FileType.Image,
                                     ProductId = 101,
                                     IsThumbnail = true,
                                     IsDetailed = false,
                                     FileUrl = "https://example.com/files/example_file_1.jpg"
                             },
                        },
                        new() {StoreFileId = 1, ShipStationStoreId =1001, FileId = 2, Files =
                             new Files
                             {
                                 FileId = 1,
                                 FileName = "example_file_1.jpg",
                                 ContentType = "image/jpeg",
                                 Content = Encoding.UTF8.GetBytes("Sample file content 1"),
                                 FileType = FileType.Image,
                                 ProductId = 101,
                                 IsThumbnail = true,
                                 IsDetailed = false,
                                 FileUrl = "https://example.com/files/example_file_1.jpg"
                             },
                        }
                ],

            };

            int validId = 1;

            _ = _shipStationStoreServiceMock.Setup(static x => x.GetAsync(
                It.IsAny<Func<IQueryable<ShipStationStore>, IQueryable<ShipStationStore>>>()))
                .ReturnsAsync(dummyStore);


            // Act
            var result = await _controller.Details(validId);

            // Assert
            _ = Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            // Arrange
            int? nullId = null;

            // Act
            var result = await _controller.Details(nullId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            int validId = 1;

            _ = _shipStationStoreServiceMock.Setup(static repo => repo.GetAsync(
                     It.IsAny<Func<IQueryable<ShipStationStore>, IQueryable<ShipStationStore>>>()))
                    .ReturnsAsync((ShipStationStore?)null);
            // Act
            var result = await _controller.Details(validId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        private void ShipStationStoresMock()
        {
            // Mock data for ship station stores
            var mockStoreList = new List<ShipStationJson>
            {
                new (1, "Store A","", "", true),
                new (2, "Store B","", "", true)
            };
            
            var objectResult = new ObjectResult(mockStoreList) { StatusCode = 200 };

            _ = _shipStationStoreServiceMock.Setup(static x => x.GetShipStationStores())
                .ReturnsAsync(mockStoreList); 
        }

        [Fact]
        public async Task Create_ReturnsViewResult_WithViewBagStores()
        {
            // Arrange
            ShipStationStoresMock();

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            // Verify ViewBag Stores
            var selectList = Assert.IsType<SelectList>(viewResult.ViewData["Stores"]);
            Assert.Equal(2, selectList.Count());
        }

        [Fact]
        public async Task Create_Returns_RedirectToIndex_When_ModelState_IsValid()
        {
            // Arrange
            var shipStationStore = new ShipStationStore { /* populate with valid data */ };

            // Act
            var result = await _controller.Create(shipStationStore) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async Task Create_Returns_View_With_ModelStateErrors_When_ModelState_IsInvalid()
        {
            // Arrange
            var shipStationStore = new ShipStationStore { /* populate with invalid data */ };
            _controller.ModelState.AddModelError("SomeKey", "Some error message");

            // Act
            var result = await _controller.Create(shipStationStore) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(shipStationStore, result.Model);
        }

        [Fact]
        public async Task Edit_WithValidId_ReturnsViewWithShipStationStore()
        {
            // Arrange
            int validId = 1;
            var shipStationStore = new ShipStationStore { ShipStationStoreId = validId, StoreType = StoreType.AMAZON };

            _ = _shipStationStoreServiceMock.Setup(static s => s.GetAsync(
                        It.IsAny<Func<IQueryable<ShipStationStore>, IQueryable<ShipStationStore>>>()))
                       .ReturnsAsync(shipStationStore);

            // Act
            var result = await _controller.Edit(validId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ShipStationStore>(viewResult.Model);
            Assert.Equal(shipStationStore, model);

            Assert.NotNull(viewResult.ViewData["StoreTypes"]);
            var storeTypes = Assert.IsType<SelectList>(viewResult.ViewData["StoreTypes"]);
            Assert.Contains(storeTypes, static s => s.Text == StoreType.AMAZON.ToString());
        }

        [Fact]
        public async Task Edit_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int? invalidId = null;
            _ = _shipStationStoreServiceMock.Setup(static s => s.GetAsync(
                        It.IsAny<Func<IQueryable<ShipStationStore>, IQueryable<ShipStationStore>>>()))
                       .ReturnsAsync((ShipStationStore?)null);

            // Act
            var result = await _controller.Edit(invalidId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ValidData_ReturnsRedirectToActionResult()
        {
            // Arrange
            var shipStationStore = new ShipStationStore
            {
                // Set properties with valid data for testing
                // Ensure id matches ShipStationStoreId
            };

            _ = _shipStationStoreServiceMock.Setup(static x => x.GetAsync(
                It.IsAny<Expression<Func<ShipStationStore, bool>>>(),
                It.IsAny<Expression<Func<ShipStationStore, object>>[]>()))
                .ReturnsAsync(new ShipStationStore());

            _ = _shipStationStoreServiceMock.Setup(static x =>
                            x.UpdateAsync(It.IsAny<ShipStationStore>()))
                           .ReturnsAsync(1);

            // Act
            var result = await _controller.Edit(shipStationStore.ShipStationStoreId, shipStationStore) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(_controller.Index), result.ActionName);
        }

        [Fact]
        public async Task Edit_InvalidModelState_ReturnsViewResult()
        {
            // Arrange

            var shipStationStore = new ShipStationStore();
            _controller.ModelState.AddModelError("key", "error message");

            // Act
            var result = await _controller.Edit(shipStationStore.ShipStationStoreId, shipStationStore) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(shipStationStore, result.Model);
        }

        [Fact]
        public async Task Edit_StoreNotFound_ReturnsValidationError()
        {
            // Arrange  
            var shipStationStore = new ShipStationStore
            {
                ShipStationStoreId = 1,
                ContactName = "Test Contact",
                PhoneNumber = "1234567890",
                FaxNumber = "0987654321",
                Address = "Test Address",
                Email = "test@example.com",
                PublicEmail = "public@example.com",
                IsActive = true,
                HasIncreasedPricing = false,
                StoreType = 0,
                StoreName = "Test Store",
                Notes = "Test Notes"
            };

            _ = _shipStationStoreServiceMock.Setup(static x => x.GetAsync(
                It.IsAny<Expression<Func<ShipStationStore, bool>>>(),
                It.IsAny<Expression<Func<ShipStationStore, object>>[]>()))
                .ReturnsAsync((ShipStationStore?)null);

            // Act  
            var result = await _controller.Edit(shipStationStore.ShipStationStoreId, shipStationStore);

            // Assert  
            var viewResult = Assert.IsType<ViewResult>(result);
            var modelState = viewResult.ViewData.ModelState;
            Assert.True(modelState.ContainsKey(string.Empty));
            var error = modelState[string.Empty]?.Errors.First();
            Assert.NotNull(error);
            Assert.Equal("No ShipStation store with a matching Id was found.", error?.ErrorMessage);
        }

        [Fact]
        public async Task Edit_ConcurrencyException_ReturnsValidationError()
        {
            // Arrange  
            var shipStationStore = new ShipStationStore
            {
                ShipStationStoreId = 1,
                ContactName = "Test Contact",
                PhoneNumber = "1234567890",
                FaxNumber = "0987654321",
                Address = "Test Address",
                Email = "test@example.com",
                PublicEmail = "public@example.com",
                IsActive = true,
                HasIncreasedPricing = false,
                StoreType = 0,
                StoreName = "Test Store",
                Notes = "Test Notes"
            };

            _ = _shipStationStoreServiceMock.Setup(static x => x.GetAsync(
                It.IsAny<Expression<Func<ShipStationStore, bool>>>(),
                It.IsAny<Expression<Func<ShipStationStore, object>>[]>()))
                .ReturnsAsync(shipStationStore);

            _ = _shipStationStoreServiceMock.Setup(static x => x.UpdateAsync(It.IsAny<ShipStationStore>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            _ = _shipStationStoreServiceMock.Setup(static x => x.IsExistsAsync(It.IsAny<Expression<Func<ShipStationStore, bool>>>()))
                .ReturnsAsync(true);

            // Act  
            var result = await _controller.Edit(shipStationStore.ShipStationStoreId, shipStationStore);

            // Assert  
            var viewResult = Assert.IsType<ViewResult>(result);
            var modelState = viewResult.ViewData.ModelState;
            Assert.True(modelState.ContainsKey(string.Empty));
            var error = modelState[string.Empty]?.Errors.First();
            Assert.NotNull(error);
            Assert.Equal("A concurrency error occurred. Please try again.", error!.ErrorMessage);
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsViewResult()
        {
            // Arrange
            int validId = 1;
            var expectedShipStationStore = new ShipStationStore();
            _ = _shipStationStoreServiceMock.Setup(static x => x.GetAsync(
                           It.IsAny<Expression<Func<ShipStationStore, bool>>>(),
                           It.IsAny<Expression<Func<ShipStationStore, object>>[]>()))
                           .ReturnsAsync(expectedShipStationStore);


            // Act
            var result = await _controller.Delete(validId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedShipStationStore, viewResult.Model);
        }

        [Fact]
        public async Task Delete_WithNullId_ReturnsNotFoundResult()
        {
            // Arrange
            int? nullId = null;

            // Act
            var result = await _controller.Delete(nullId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            int invalidId = 999;
            _ = _shipStationStoreServiceMock.Setup(static x => x.GetAsync(
                          It.IsAny<Expression<Func<ShipStationStore, bool>>>(),
                          It.IsAny<Expression<Func<ShipStationStore, object>>[]>()))
                         .ReturnsAsync((ShipStationStore?)null);

            // Act
            var result = await _controller.Delete(invalidId);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsRedirectToActionResult_WhenRemovalIsSuccessful()
        {
            // Arrange
            int id = 1;
            _ = _shipStationStoreServiceMock.Setup(s => s.RemoveAsync(id)).ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteConfirmed(id) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(_controller.Index), result.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsActionResult_WhenRemovalFails()
        {
            // Arrange
            int id = 1;
            _ = _shipStationStoreServiceMock.Setup(s =>
                 s.RemoveAsync(id))
                .ThrowsAsync(new Exception("Failed to remove item"));

            // Act
            _ = await Assert.ThrowsAsync<Exception>(async () => await _controller.DeleteConfirmed(id));
        }

        [Fact]
        public async Task DeleteFile_ReturnsRedirectToActionResult_WhenStoreFileExists()
        {
            // Arrange
            int storeFileId = 1;
            var storeFile = new ShipStationStoreFile() { FileId = 1, StoreFileId = 1, ShipStationStoreId = 1001 };
            _ = _shipStationStoreServiceMock.Setup(service =>
                 service.GetStoreFileAsync(storeFileId))
                .ReturnsAsync(storeFile);

            // Act
            var result = await _controller.DeleteFile(storeFileId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(_controller.Edit), result.ActionName);
            Assert.NotNull(result.RouteValues);
            Assert.Equal(storeFile.ShipStationStoreId, result.RouteValues["id"]);
        }

        [Fact]
        public async Task DeleteFile_ReturnsRedirectToActionResult_WhenStoreFileDoesNotExist()
        {
            // Arrange
            int storeFileId = 2;

            _ = _shipStationStoreServiceMock.Setup(service =>
                 service.GetStoreFileAsync(storeFileId))
                .ReturnsAsync((ShipStationStoreFile?)null);

            // Act
            var result = await _controller.DeleteFile(storeFileId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(_controller.Edit), result.ActionName);
            Assert.Null(result.RouteValues?["id"]);
        }
    }
}
