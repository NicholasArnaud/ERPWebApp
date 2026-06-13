using ERPWebApp.Controllers.Shipping;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace ERPWebApp.UnitTests.Controllers.Shipping
{
    public class WarehouseControllerTests
    {

        private readonly Mock<IWarehouseService> _warehouseServiceMock;
        private readonly Mock<ILogger<WarehouseController>> _loggerMock;
        private readonly WarehouseController _controller;

        public WarehouseControllerTests()
        {
            _warehouseServiceMock = new Mock<IWarehouseService>();
            _loggerMock = new Mock<ILogger<WarehouseController>>();

            _controller = new WarehouseController(_warehouseServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewWithModel()
        {
            // Arrange
            var warehouseList = new List<Warehouse>
            {
                new() { WarehouseId = 1, WarehouseName = "Test Location 1" },
                new() { WarehouseId = 2, WarehouseName = "Test Location 2" }
            };
            _ = _warehouseServiceMock.Setup(static s => s.GetAllAsync(
           It.IsAny<Expression<Func<Warehouse, string>>[]>(),
           It.IsAny<Expression<Func<Warehouse, object>>[]>()))
           .ReturnsAsync(warehouseList);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Warehouse>>(viewResult.Model);
            Assert.Equal(warehouseList.Count, model.Count());
        }



        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var warehouse = new Warehouse
            {
                WarehouseName = "New Warehouse",
                Company = "Test Company"
            };

            _ = _warehouseServiceMock.Setup(s => s.GetAsync(
            It.IsAny<Expression<Func<Warehouse, bool>>>(),
            It.IsAny<Expression<Func<Warehouse, object>>[]>()))
            .ReturnsAsync(new Warehouse { WarehouseId = 1, WarehouseName = "Test Warehouse" });


            // Act
            var result = await _controller.Create(warehouse);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _warehouseServiceMock.Verify(s => s.AddAsync(warehouse), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsViewWithTimeZones()
        {
            // Arrange
            var timeZones = TimeZoneInfo.GetSystemTimeZones().Select(static tz => new SelectListItem { Value = tz.Id, Text = tz.DisplayName }).ToList();

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var selectList = Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["TimeZone"]);
            Assert.NotEmpty(selectList); 
        }

        [Fact]
        public async Task Details_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _ = _warehouseServiceMock.Setup(static s => s.GetAsync(
            It.IsAny<Expression<Func<Warehouse, bool>>>(),
            It.IsAny<Expression<Func<Warehouse, object>>[]>()))
            .ReturnsAsync((Warehouse?)null);

            // Act
            var result = await _controller.Details(99);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var warehouse = new Warehouse { WarehouseId = 1 };
            _ = _warehouseServiceMock.Setup(static s => s.IsExistsAsync(It.IsAny<Expression<Func<Warehouse, bool>>>())).ReturnsAsync(false);

            // Act
            var result = await _controller.Edit(1, warehouse);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            _ = _warehouseServiceMock.Setup(static s => s.GetAsync(
             It.IsAny<Expression<Func<Warehouse, bool>>>(),
             It.IsAny<Expression<Func<Warehouse, object>>[]>()))
             .ReturnsAsync((Warehouse?)null);

            // Act
            var result = await _controller.Delete(99);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ExistingId_RedirectsToIndex()
        {
            // Arrange
            _ = _warehouseServiceMock.Setup(static s => s.IsExistsAsync(It.IsAny<Expression<Func<Warehouse, bool>>>())).ReturnsAsync(true);
            _ = _warehouseServiceMock.Setup(static s => s.RemoveAsync(It.IsAny<int>())).ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _warehouseServiceMock.Verify(static s => s.RemoveAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_ExistingId_CallsRemoveAndRedirects()
        {
            // Arrange
            var existingWarehouseId = 1; // Assume warehouse with ID 1 exists
            _ = _warehouseServiceMock.Setup(s => s.IsExistsAsync(It.IsAny<Expression<Func<Warehouse, bool>>>())).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteConfirmed(existingWarehouseId);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            _warehouseServiceMock.Verify(s => s.RemoveAsync(existingWarehouseId), Times.Once); // Verify RemoveAsync was called once
        }

        [Fact]
        public async Task Create_Post_InvalidPhoneNumber_ReturnsViewWithErrors()
        {
            // Arrange: Provide data that violates the [Phone] attribute
            var warehouse = new Warehouse
            {
                WarehouseName = "Test Location",  
                DefaultWarehouse = true,         
                Country = "US",                  
                StreetAddress1 = "123 Main St",  
                City = "SomeCity",               
                State = "SomeState",            
                PostalCode = "12345",            
                PhoneNumber = "NotAPhone",       
                Email = "test@example.com",      
                TimeZone = "America/New_York",   
                SameAsReturnAddress = true       
            };

            // Simulate the model validation failing for phone number
            _controller.ModelState.AddModelError("PhoneNumber", "The PhoneNumber field is not a valid phone number.");

            // Act
            var result = await _controller.Create(warehouse);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsAssignableFrom<Warehouse>(viewResult.Model);
            Assert.Equal(warehouse, returnedModel);
            Assert.True(_controller.ModelState["PhoneNumber"]?.Errors.Count > 0);

            // Ensure no database call is made
            _warehouseServiceMock.Verify(static s => s.AddAsync(It.IsAny<Warehouse>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_InvalidEmail_ReturnsViewWithErrors()
        {
            // Arrange: Provide data that violates the [EmailAddress] attribute
            var warehouse = new Warehouse
            {
                WarehouseName = "Test Location",
                DefaultWarehouse = true,
                Country = "US",
                StreetAddress1 = "123 Main St",
                City = "SomeCity",
                State = "SomeState",
                PostalCode = "12345",
                PhoneNumber = "123-456-7890", // valid phone
                Email = "InvalidEmail",       // Not a valid email format
                TimeZone = "America/New_York",
                SameAsReturnAddress = true
            };

            _controller.ModelState.AddModelError("Email", "The Email field is not a valid e-mail address.");

            // Act
            var result = await _controller.Create(warehouse);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsAssignableFrom<Warehouse>(viewResult.Model);
            Assert.Equal(warehouse, returnedModel);
            Assert.True(_controller.ModelState["Email"]?.Errors.Count > 0);

            // Ensure no database call is made
            _warehouseServiceMock.Verify(static s => s.AddAsync(It.IsAny<Warehouse>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_MissingRequiredField_ReturnsViewWithErrors__AlternateScenario()
        {
            // Arrange: Omit a required field like LocationName
            var warehouse = new Warehouse
            {
                WarehouseId = 1,
                DefaultWarehouse = true,
                Country = "US",
                StreetAddress1 = "123 Main St",
                City = "SomeCity",
                State = "SomeState",
                PostalCode = "12345",
                PhoneNumber = "123-456-7890",
                Email = "test@example.com",
                TimeZone = "America/New_York",
                SameAsReturnAddress = true
                // LocationName is missing, which is [Required]
            };

            // Assume the warehouse exists
            _ = _warehouseServiceMock.Setup(static s => s.IsExistsAsync(It.IsAny<Expression<Func<Warehouse, bool>>>())).ReturnsAsync(true);

            _controller.ModelState.AddModelError("LocationName", "The LocationName field is required.");

            // Act
            var result = await _controller.Edit(1, warehouse);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsAssignableFrom<Warehouse>(viewResult.Model);
            Assert.Equal(warehouse, returnedModel);
            Assert.True(_controller.ModelState["LocationName"]?.Errors.Count > 0);

            // Ensure no database update call is made
            _warehouseServiceMock.Verify(static s => s.UpdateAsync(It.IsAny<Warehouse>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_InvalidPostalCode_ReturnsViewWithErrors__AlternateScenario()
        {
            // Arrange: Provide a postal code longer than 10 characters
            var warehouse = new Warehouse
            {
                WarehouseName = "Test Location",
                DefaultWarehouse = true,
                Country = "US",
                StreetAddress1 = "123 Main St",
                City = "SomeCity",
                State = "SomeState",
                PostalCode = "123456789012345", // Too long, [StringLength(10)] required
                PhoneNumber = "123-456-7890",
                Email = "test@example.com",
                TimeZone = "America/New_York",
                SameAsReturnAddress = true
            };

            _controller.ModelState.AddModelError("PostalCode", "The field Postal Code must be a string or array type with a maximum length of '10'.");

            // Act
            var result = await _controller.Create(warehouse);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsAssignableFrom<Warehouse>(viewResult.Model);
            Assert.Equal(warehouse, returnedModel);
            Assert.True(_controller.ModelState["PostalCode"]?.Errors.Count > 0);

            // Ensure no database call is made
            _warehouseServiceMock.Verify(static s => s.AddAsync(It.IsAny<Warehouse>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_InvalidEmail_ReturnsViewWithErrors__AlternateScenario()
        {
            // Arrange: Provide data that violates the [EmailAddress] attribute
            var warehouse = new Warehouse
            {
                WarehouseName = "Test Location",
                DefaultWarehouse = true,
                Country = "US",
                StreetAddress1 = "123 Main St",
                City = "SomeCity",
                State = "SomeState",
                PostalCode = "12345",
                PhoneNumber = "123-456-7890", // valid phone
                Email = "InvalidEmail",       // Not a valid email format
                TimeZone = "America/New_York",
                SameAsReturnAddress = true
            };

            _controller.ModelState.AddModelError("Email", "The Email field is not a valid e-mail address.");

            // Act
            var result = await _controller.Create(warehouse);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsAssignableFrom<Warehouse>(viewResult.Model);
            Assert.Equal(warehouse, returnedModel);
            Assert.True(_controller.ModelState["Email"]?.Errors.Count > 0);

            // Ensure no database call is made
            _warehouseServiceMock.Verify(static s => s.AddAsync(It.IsAny<Warehouse>()), Times.Never);
        }

        


    }
}
