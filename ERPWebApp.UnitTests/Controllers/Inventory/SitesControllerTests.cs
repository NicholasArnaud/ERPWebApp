using ERPWebApp.Controllers.Inventory;
using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers.Inventory
{
    [Trait("Category", "execute")]
    public class SitesControllerTests
    {
        private readonly Mock<ISiteService> _siteServiceMock = new();
        private readonly SitesController _controller;
        public SitesControllerTests()
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

            _controller = new SitesController(_siteServiceMock.Object)
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
            SitesController controller = _controller;
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
                controller = new SitesController(_siteServiceMock.Object)
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

            _ = _siteServiceMock.Setup(static x => x.GetListAsync<Site>(It.IsAny<Func<IQueryable<Site>, IQueryable<Site>>>()))
            .ReturnsAsync(SiteFixtures.GetTestSites());

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            _ = Assert.IsType<List<Site>>(viewResult.Model);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ToggleActive_ReturnsSites(bool isActive)
        {
            // Arrange
            var sites = SiteFixtures.GetTestSites().Where(x => x.IsActive = isActive).ToList();
            _ = _siteServiceMock.Setup(x => x.GetListAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, string>>[]>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            )).ReturnsAsync(sites);

            // Act
            var result = await _controller.ToggleActive(isActive);

            // Assert
            Assert.Equal(sites, result);
            _siteServiceMock.Verify(x => x.GetListAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, string>>[]>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            ), Times.Once);
        }

        [Fact]
        public async Task PartialViewTableShow_ReturnsPartialIndexView()
        {
            var siteData = SiteFixtures.GetTestSites();
            // Arrange
            _ = _siteServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<Site>, IQueryable<Site>>>()))
            .ReturnsAsync(siteData);

            // Act
            var result = await _controller.PartialViewTableShow() as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PartialIndex", result.ViewName);
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsView()
        {
            // Arrange
            var expectedSite = new Site { SiteId = 1, SiteName = "Example Site" };
            _ = _siteServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            )).ReturnsAsync(expectedSite);


            // Act
            var result = await _controller.Details(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedSite, result.Model);
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
            _ = _siteServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            )).ReturnsAsync((Site?)null);

            // Act
            var result = await _controller.Details(999) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_RedirectsToIndexOnValidModel()
        {
            // Arrange
            var validSite = SiteFixtures.GetTestSites().FirstOrDefault();

            // Act
            var result = await _controller.Create(validSite) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            _siteServiceMock.Verify(service => service.AddAsync(validSite), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsViewOnInvalidModel()
        {
            // Arrange
            var invalidSite = new Site();
            _controller.ModelState.AddModelError("SiteName", "SiteName is required");

            // Act
            var result = await _controller.Create(invalidSite) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(invalidSite, result.Model);
            _siteServiceMock.Verify(service => service.AddAsync(invalidSite), Times.Never);
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
            _siteServiceMock.Verify(static service => service.GetAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            ), Times.Never);
        }

        [Fact]
        public async Task Edit_ReturnsViewForAuthorizedUser()
        {
            // Arrange
            var testSiteId = 1;

            _ = _siteServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            )).ReturnsAsync(new Site() { SiteId = 1, SiteName = "new" });

            // Act
            var result = await _controller.Edit(testSiteId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            _siteServiceMock.Verify(static service => service.GetAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            ), Times.Once);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenIdMismatch()
        {
            // Arrange
            var testSite = new Site { SiteId = 1, SiteName = "TestSite" };

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
            var testSite = new Site { SiteId = 1, SiteName = "TestSite" };
            _controller.ModelState.AddModelError("SiteName", "SiteName is required");

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
            _ = _siteServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            )).ReturnsAsync((Site?)null);

            // Act
            var result = await _controller.Delete(123) as NotFoundResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            _siteServiceMock.Verify(static service => service.GetAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            ), Times.Once);
        }

        [Fact]
        public async Task Delete_WithExistingId_ReturnsView()
        {
            // Arrange
            var existingSite = new Site { SiteId = 1, };
            _ = _siteServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            )).ReturnsAsync(existingSite);

            // Act
            var result = await _controller.Delete(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingSite, result.Model);
            _siteServiceMock.Verify(static service => service.GetAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            ), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsRedirectToIndexForInactiveSite()
        {
            // Arrange
            var id = 1;
            _ = _siteServiceMock.Setup(s => s.GetAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            )).ReturnsAsync(new Site { IsActive = false });

            // Act
            var result = await _controller.DeleteConfirmed(id) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(_controller.Index), result.ActionName);
            _siteServiceMock.Verify(service => service.RemoveAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsRedirectToIndexForActiveSite()
        {
            // Arrange
            var id = 1;
            _ = _siteServiceMock.Setup(s => s.GetAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            )).ReturnsAsync(new Site { IsActive = true });

            // Act
            var result = await _controller.DeleteConfirmed(id) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(_controller.Index), result.ActionName);
            Assert.Equal("Site is still active! Mark the Site as INACTIVE and try the operation again.", _controller.TempData["ErrorMessage"]);
            _siteServiceMock.Verify(service => service.RemoveAsync(id), Times.Never);
        }

        [Fact]
        public async Task DeleteConfirmed_ReturnsRedirectToIndexOnException()
        {
            // Arrange
            var id = 1;
            _ = _siteServiceMock.Setup(s => s.GetAsync(
                It.IsAny<Expression<Func<Site, bool>>>(),
                It.IsAny<Expression<Func<Site, object>>[]>()
            )).ThrowsAsync(new Exception("Simulated exception"));

            // Act
            var result = await _controller.DeleteConfirmed(id) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(_controller.Index), result.ActionName);
            Assert.Equal("Site is still being used elsewhere.", _controller.TempData["ErrorMessage"]);
            _siteServiceMock.Verify(service => service.RemoveAsync(id), Times.Never);
        }
    }
}