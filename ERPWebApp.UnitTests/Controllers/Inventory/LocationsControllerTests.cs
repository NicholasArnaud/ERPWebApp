using ERPWebApp.Controllers.Inventory;
using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using System.Linq.Expressions;
using System.Security.Claims;
using ERPWebApp.Data.DTOModels.LocationDtos;
using ERPWebApp.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERPWebApp.UnitTests.Controllers.Inventory
{
    [Trait("Category", "execute")]
    public class LocationsControllerTests
    {
        private readonly Mock<ILocationService> _locationServiceMock = new();
        private readonly Mock<ISiteService> _siteServiceMock = new();
        private readonly Mock<IStocksService> _stocksServiceMock = new();
        private readonly LocationsController _controller;
        public LocationsControllerTests()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.Administrator),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller = new LocationsController(_locationServiceMock.Object, _siteServiceMock.Object, _stocksServiceMock.Object)
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

        [Theory]
        [InlineData(RoleList.ExternalViewer)]
        [InlineData(RoleList.Administrator)]
        [InlineData(RoleList.ExternalUser)]
        public async Task GetLocations_ReturnsExpectedData_ForRole(string Role)
        {
            // Arrange
            LocationsController controller = _controller;
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
                controller = new LocationsController(
                    _locationServiceMock.Object,
                    _siteServiceMock.Object,
                    _stocksServiceMock.Object
                )
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

            controller.Request.Form = request.Object.Form;

            var locationList = LocationFixtures.GetTestLocations()
                .Select(static x => new LocationList(
                        x.LocationId,
                        x.LocationName,
                        x.Sites.SiteName,
                        x.LocationDescription,
                        x.Type.ToString(),
                        x.IsActive,
                        x.IsExternal,
                        "yes"
                )).ToList();

            _ = _locationServiceMock.Setup(static x => x.GetLocationsAsync(
                true,
                true,
                It.IsAny<int>(),
                "yes",
                new SearchParameters()
            )).ReturnsAsync((locationList, locationList.Count));

            _ = _siteServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Func<IQueryable<Site>, IQueryable<SelectListItem>>>()
            )).ReturnsAsync(
                SiteFixtures.GetTestSites()
                    .Select(static x => new SelectListItem
                    {
                        Value = x.SiteId.ToString(),
                        Text = x.SiteName
                    }
                    ).ToList()
            );

            // Act
            var result = await controller.GetLocations(true, 123) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange

            // Act
            var result = await _controller.Details(null);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenLocationNotFound()
        {
            // Arrange
            _ = _locationServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Expression<Func<Location, object>>[]>()
            )).ReturnsAsync((Location?)null);


            // Act
            var result = await _controller.Details(123); // Replace with an ID that doesn't exist

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewWithLocation_WhenLocationExists()
        {
            // Arrange
            var locationId = 1;
            var expectedLocation = new Location { LocationId = locationId};

            _ = _locationServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Expression<Func<Location, object>>[]>()
            )).ReturnsAsync(expectedLocation);


            // Act
            var result = await _controller.Details(locationId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedLocation, viewResult.Model);
        }

        [Theory]
        [InlineData(RoleList.ExternalViewer)]
        [InlineData(RoleList.Administrator)]
        [InlineData(RoleList.ExternalUser)]
        public void Create_ReturnsView_ForAdministrator(string Role)
        {
            // Arrange
            LocationsController controller = _controller;
            if (Role != RoleList.Administrator)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, "1"),
                    new(ClaimTypes.Name, "testuser"),
                    new(ClaimTypes.Role, Role)
                };

                if(Role == RoleList.ExternalViewer){
                    claims.Add(new(ClaimTypes.Role, RoleList.InventoryManager));
                    claims.Add(new(ClaimTypes.Role, RoleList.ShippingManager));
                }

                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var claimsPrincipal = new ClaimsPrincipal(identity);
                controller = new LocationsController(
                    _locationServiceMock.Object,
                    _siteServiceMock.Object,
                    _stocksServiceMock.Object
                )
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

            _ = _siteServiceMock.Setup(static x => x.GetList(
                It.IsAny<Func<IQueryable<Site>, IQueryable<SelectListItem>>>()
            )).Returns(
                SiteFixtures.GetTestSites()
                    .Select(static x => new SelectListItem
                    {
                        Value = x.SiteId.ToString(),
                        Text = x.SiteName
                    }
                    ).ToList()
            );
            // Act
            var result = controller.Create() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ViewData["Site"]);
        }

        [Fact]
        public async Task Create_Returns_RedirectToActionResult_When_ModelStateIsValid()
        {
            // Arrange

            _ = _locationServiceMock.Setup(static x => x.IsExists(
                   It.IsAny<Expression<Func<Location, bool>>>()
            )).Returns(false);

            _ = _siteServiceMock.Setup(static x => x.IsExists(It.IsAny<Expression<Func<Site, bool>>>())).Returns(true);
            // Act
            var result = await _controller.Create(LocationFixtures.GetTestLocations().FirstOrDefault()) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(_controller.Index), result.ActionName);
        }

        [Fact]
        public async Task Create_Returns_When_ModelStateIsValid_Location_Exists()
        {
            // Arrange

            _ = _locationServiceMock.Setup(static x => x.IsExists(
                   It.IsAny<Expression<Func<Location, bool>>>()
            )).Returns(true);

            _ = _siteServiceMock.Setup(static x => x.IsExists(It.IsAny<Expression<Func<Site, bool>>>())).Returns(true);
            _ = _siteServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Func<IQueryable<Site>, IQueryable<SelectListItem>>>()
            )).ReturnsAsync(
                SiteFixtures.GetTestSites()
                    .Select(static x => new SelectListItem
                    {
                        Value = x.SiteId.ToString(),
                        Text = x.SiteName
                    }
                    ).ToList()
            );
           
            // Act
            var result = await _controller.Create(LocationFixtures.GetTestLocations().FirstOrDefault());

            // Assert
            _ = result.Should().NotBeNull();
            Assert.True(_controller.ModelState.ContainsKey("LocationName")); 
            var modelStateEntry = _controller.ModelState["LocationName"];
            Assert.NotNull(modelStateEntry);
            var error = Assert.Single(modelStateEntry!.Errors);
            Assert.Equal("A location with this name already exists.", error.ErrorMessage);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            int? nullId = null;

            // Act
            var result = await _controller.Delete(nullId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenLocationNotFound()
        {
            // Arrange
            int id = 1;
            _ = _locationServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Expression<Func<Location, object>>[]>()
            )).ReturnsAsync((Location?)null);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsView_WhenLocationExists()
        {
            // Arrange
            int id = 1;
            var location = new Location { LocationId = id };
            _ = _locationServiceMock.Setup(static service => service.GetAsync(
                It.IsAny<Expression<Func<Location, bool>>>(),
                It.IsAny<Expression<Func<Location, object>>[]>()
            )).ReturnsAsync(location);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(location, viewResult.Model);
        }

        [Fact]
        public async Task DeleteConfirmed_LocationNotActive_RedirectsToIndex()
        {
            // Arrange
            _ = _locationServiceMock.Setup(static x => x.IsExists(It.IsAny<Expression<Func<Location, bool>>>())).Returns(true);
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
            // Act
            var result = await _controller.DeleteConfirmed(3) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            var errorMessage = "Location is still ACTIVE. Please mark as INACTIVE and try the operation again.";
            Assert.Equal(errorMessage, _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task DeleteConfirmed_LocationActive_RemainsOnIndex()
        {
            // Arrange
            _ = _locationServiceMock.Setup(static x => x.IsExists(It.IsAny<Expression<Func<Location, bool>>>())).Returns(false);
            _ = _locationServiceMock.Setup(static x => x.RemoveAsync(1)).ReturnsAsync(1);
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.DeleteConfirmed(1) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_ExceptionThrown_RedirectsToIndexWithError()
        {
            // Arrange
            _ = _locationServiceMock.Setup(static x => x.IsExists(null)).Returns(LocationFixtures.GetTestLocations().Any(static x => x.LocationId == 1));
            _ = _locationServiceMock.Setup(static x => x.RemoveAsync(1)).Throws<Exception>();
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
            // Act
            var result = await _controller.DeleteConfirmed(1) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Location is still being used elsewhere.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenIdMismatch()
        {
            // Arrange
            var location = new Location { LocationId = 1 };
            var result = await _controller.Edit(2, location);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WhenIdIsNull_ReturnsNotFound()
        {
            // Arrange
            int? id = null;

            // Act
            var result = await _controller.Edit(id);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WhenLocationNotFound_ReturnsNotFound()
        {
            // Arrange
            int id = 1; // Assuming a valid ID
            _ = _locationServiceMock.Setup(static x => x.GetAsync(
                    It.IsAny<Expression<Func<Location, bool>>>(),
                    It.IsAny<Expression<Func<Location, object>>[]>()
            )).ReturnsAsync((Location?)null);

            // Act
            var result = await _controller.Edit(id);

            // Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WhenUserIsAdministrator_ReturnsViewWithSiteData()
        {
            // Arrange
            int id = 1; 
            var location = new Location { LocationId = id, SiteId = 1 }; 
            var sites = new List<Site> { new() { SiteId = 1, SiteName = "Site 1" } };

            _ = _locationServiceMock.Setup(static x => x.GetAsync(
                   It.IsAny<Expression<Func<Location, bool>>>(),
                   It.IsAny<Expression<Func<Location, object>>[]>()
            )).ReturnsAsync(location);

            _ = _siteServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Func<IQueryable<Site>, IQueryable<SelectListItem>>>()
            )).ReturnsAsync(
                SiteFixtures.GetTestSites()
                    .Select(static x => new SelectListItem
                    {
                        Value = x.SiteId.ToString(),
                        Text = x.SiteName
                    }
                    ).ToList()
            );

            // Act
            var result = await _controller.Edit(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(location, viewResult.Model);
            Assert.NotNull(viewResult.ViewData["Site"]);
        }

        [Fact]
        public async Task Edit_WhenDefaultCase_ReturnsViewWithLocation()
        {
            // Arrange
            int id = 1;
            var location = new Location { LocationId = id, SiteId = 1 };
            var sites = new List<Site> { new() { SiteId = 1, SiteName = "Site 1" } };


            _ = _locationServiceMock.Setup(static x => x.GetAsync(
                   It.IsAny<Expression<Func<Location, bool>>>(),
                   It.IsAny<Expression<Func<Location, object>>[]>()
            )).ReturnsAsync(location);

            _ = _siteServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Func<IQueryable<Site>, IQueryable<SelectListItem>>>()
            )).ReturnsAsync(
                SiteFixtures.GetTestSites()
                    .Select(static x => new SelectListItem
                    {
                        Value = x.SiteId.ToString(),
                        Text = x.SiteName
                    }
                    ).ToList()
            );

            // Act
            var result = await _controller.Edit(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(location, viewResult.Model);
        }
    }
}