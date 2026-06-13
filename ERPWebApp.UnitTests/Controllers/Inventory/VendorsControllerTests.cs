using ERPWebApp.Controllers.Inventory;
using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers.Inventory
{
    [Trait("Category", "execute")]
    public class VendorsControllerTests
    {
        private readonly Mock<IVendorService> _vendorServiceMock = new();
        private readonly VendorsController _controller;
        public VendorsControllerTests()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.Administrator),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var mockHttpContext = new Mock<HttpContext>();
            var mockTempDataProvider = new Mock<ITempDataProvider>();

            _controller = new VendorsController(_vendorServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal
                    }
                },
                TempData = new TempDataDictionary(mockHttpContext.Object, mockTempDataProvider.Object)
            };
        }

        [Theory]
        [InlineData(RoleList.ExternalViewer)]
        [InlineData(RoleList.Administrator)]
        [InlineData(RoleList.ExternalUser)]
        public async Task Index_ReturnsViewForRole(string Role)
        {
            // Arrange
            VendorsController controller = _controller;
            if (Role != RoleList.Administrator)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, "1"),
                    new(ClaimTypes.Name, "testuser"),
                    new(ClaimTypes.Role, Role),
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var claimsPrincipal = new ClaimsPrincipal(identity);
                controller = new VendorsController(_vendorServiceMock.Object)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext
                        {
                            User = claimsPrincipal
                        }
                    },
                };
            }

            _ = _vendorServiceMock.Setup(static x => x.GetListAsync<Vendor>(It.IsAny<Func<IQueryable<Vendor>, IQueryable<Vendor>>>()))
            .ReturnsAsync(VendorFixtures.GetTestList());

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            _ = Assert.IsType<List<Vendor>>(viewResult.Model);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToggleActive_ReturnsSites(bool isActive)
        {
            // Arrange
            var vendor = VendorFixtures.GetTestList().Where(x => x.IsActive = isActive).ToList();
            _ = _vendorServiceMock.Setup(x => x.GetList(
                It.IsAny<Expression<Func<Vendor, bool>>>(),
                It.IsAny<Expression<Func<Vendor, string>>[]>(),
                It.IsAny<Expression<Func<Vendor, object>>[]>()
            )).Returns(vendor);

            // Act
            var result =  _controller.ToggleActive(isActive);

            // Assert
            Assert.Equal(vendor, result);
            _vendorServiceMock.Verify(x => x.GetList(
                It.IsAny<Expression<Func<Vendor, bool>>>(),
                It.IsAny<Expression<Func<Vendor, string>>[]>(),
                It.IsAny<Expression<Func<Vendor, object>>[]>()
            ), Times.Once);
        }

        [Fact]
        public void PartialViewTableShow_ReturnsPartialIndexView()
        {
            var vendorList = VendorFixtures.GetTestList();
            // Arrange
            _ = _vendorServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<Vendor>, IQueryable<Vendor>>>()))
            .ReturnsAsync(vendorList);

            // Act
            var result =  _controller.PartialViewTableShow() as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PartialIndex", result.ViewName);
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsView()
        {
            // Arrange
            var expectedVendor = new Vendor { VendorId = 1, VendorName = "Example Vendor", VendorNumber = "1" };
            _ = _vendorServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Vendor, bool>>>(),
                It.IsAny<Expression<Func<Vendor, object>>[]>()
            )).ReturnsAsync(expectedVendor);


            // Act
            var result = await _controller.Details(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedVendor, result.Model);
        }

        [Fact]
        public async Task Details_WithNullId_ReturnsNotFound()
        {
            // Arrange
            // Act
            var result = await _controller.Details(null) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _ = _vendorServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Vendor, bool>>>(),
                It.IsAny<Expression<Func<Vendor, object>>[]>()
            )).ReturnsAsync((Vendor?)null);

            // Act
            var result = await _controller.Details(999) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_RedirectsToIndexOnValidModel()
        {
            // Arrange
            var validSite = VendorFixtures.GetTestList().FirstOrDefault();
            _ = _vendorServiceMock.Setup(x => x.IsExistsAsync(It.IsAny<Expression<Func<Vendor, bool>>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Create(validSite,null) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            _vendorServiceMock.Verify(service => service.AddAsync(validSite), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsViewOnInvalidModel()
        {
            // Arrange
            var invalidVender = new Vendor() { VendorName = "", VendorNumber = ""};
            _ = _vendorServiceMock.Setup(x => x.IsExistsAsync(It.IsAny<Expression<Func<Vendor, bool>>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Create(invalidVender,null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(invalidVender, result.Model);
            _vendorServiceMock.Verify(service => service.AddAsync(invalidVender), Times.Never);
        }

        [Fact]
        public async Task Edit_ReturnsNotFoundForUnauthorizedUser()
        {
            // Arrange

            // Act
            var result = await _controller.Edit(null) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            _vendorServiceMock.Verify(static service => service.GetAsync(
                It.IsAny<Expression<Func<Vendor, bool>>>(),
                It.IsAny<Expression<Func<Vendor, object>>[]>()
            ), Times.Never);
        }

        [Fact]
        public async Task Edit_ReturnsViewForAuthorizedUser()
        {
            // Arrange
            var testSiteId = 1;

            _ = _vendorServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Vendor, bool>>>(),
                It.IsAny<Expression<Func<Vendor, object>>[]>()
            )).ReturnsAsync(new Vendor() { VendorId = 1, VendorName = "new", VendorNumber = "1" });

            // Act
            var result = await _controller.Edit(testSiteId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            _vendorServiceMock.Verify(static service => service.GetAsync(
                It.IsAny<Expression<Func<Vendor, bool>>>(),
                It.IsAny<Expression<Func<Vendor, object>>[]>()
            ), Times.Once);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenIdMismatch()
        {
            // Arrange
            var testSite = new Vendor { VendorId = 1, VendorName = "TestSite", VendorNumber = "1" };

            // Act
            var result = await _controller.Edit(2, testSite) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Edit_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var testSite = new Vendor { VendorId = 1, VendorName = "TestSite" , VendorNumber = "1"};
            _controller.ModelState.AddModelError("VendorName", "VendorName is required");

            // Act
            var result = await _controller.Edit(1, testSite) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testSite, result.Model);
        }

        [Fact]
        public async Task Delete_WithNullId_ReturnsNotFound()
        {
            // Arrange

            // Act
            var result = await _controller.Delete(null) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Delete_WithNonExistingId_ReturnsNotFound()
        {
            // Arrange
            _ = _vendorServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Vendor, bool>>>(),
                It.IsAny<Expression<Func<Vendor, object>>[]>()
            )).ReturnsAsync((Vendor?)null);

            // Act
            var result = await _controller.Delete(123) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            _vendorServiceMock.Verify(static service => service.GetAsync(
                It.IsAny<Expression<Func<Vendor, bool>>>(),
                It.IsAny<Expression<Func<Vendor, object>>[]>()
            ), Times.Once);
        }

        [Fact]
        public async Task Delete_WithExistingId_ReturnsView()
        {
            // Arrange
            var existingSite = new Vendor { VendorId = 1, VendorName = "New", VendorNumber = "1" };
            _ = _vendorServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Vendor, bool>>>(),
                It.IsAny<Expression<Func<Vendor, object>>[]>()
            )).ReturnsAsync(existingSite);

            // Act
            var result = await _controller.Delete(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingSite, result.Model);
            _vendorServiceMock.Verify(static service => service.GetAsync(
                It.IsAny<Expression<Func<Vendor, bool>>>(),
                It.IsAny<Expression<Func<Vendor, object>>[]>()
            ), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsRedirectToIndex()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await _controller.DeleteConfirmed(id) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(_controller.Index), result.ActionName);
            _vendorServiceMock.Verify(service => service.RemoveAsync(id), Times.Once);
        }
    }
}