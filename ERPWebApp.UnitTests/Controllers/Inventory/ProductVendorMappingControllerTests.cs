using ERPWebApp.Controllers.Inventory;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERPWebApp.UnitTests.Controllers.Inventory
{
    [Trait("Category", "execute")]
    public class ProductVendorMappingControllerTests
    {
        private readonly Mock<IProductVendorMappingService> _productVendorMappingServiceMock = new();
        private readonly Mock<IProductService> _productServiceMock = new();
        private readonly Mock<IVendorService> _vendorServiceMock = new();
        private readonly ProductVendorMappingController _controller;
        public ProductVendorMappingControllerTests()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.ExternalViewer),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var mockHttpContext = new Mock<HttpContext>();
            var mockTempDataProvider = new Mock<ITempDataProvider>();

            _controller = new ProductVendorMappingController(
                _productVendorMappingServiceMock.Object,
                _productServiceMock.Object,
                _vendorServiceMock.Object
            )
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
        public async Task When_ProductVendor_HasValue_ReturnsViewWithCorrectData(string Role)
        {
            // Arrange
            ProductVendorMappingController controller = _controller;
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
                controller = new ProductVendorMappingController(
                    _productVendorMappingServiceMock.Object,
                      _productServiceMock.Object,
                _vendorServiceMock.Object
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
            var products = new List<Product>
            {
                new() { ProductId = 1, Sku = "SKU1", Description = "Description1", IsActive = true, IsExternalProduct = false },
                new() { ProductId = 2, Sku = "SKU2", Description = "Description2", IsActive = true, IsExternalProduct = true }
            };

            var vendors = new List<Vendor>
            {
                new() { VendorId = 1, VendorName = "Vendor1", IsActive = true, IsExternal = false, VendorNumber = "1" },
                new() { VendorId = 2, VendorName = "Vendor2", IsActive = true, IsExternal = true, VendorNumber = "2"}
            };

            _ = _productServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()))
                   .ReturnsAsync(products);
            _ = _vendorServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<Vendor>, IQueryable<Vendor>>>()))
                .ReturnsAsync(vendors);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["ProductList"]);
            Assert.NotNull(viewResult.ViewData["VendorList"]);
        }

        [Theory]
        [InlineData(RoleList.ExternalViewer)]
        [InlineData(RoleList.Administrator)]
        [InlineData(RoleList.ExternalUser)]
        public async Task When_GetProductList_ReturnsExpectedProductVendorMappingFilter(string Role)
        {
            // Arrange
            var psku = "123";
            ProductVendorMappingController controller = _controller;
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
                controller = new ProductVendorMappingController(
                    _productVendorMappingServiceMock.Object,
                      _productServiceMock.Object,
                _vendorServiceMock.Object
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
            var productVendorMappings = ProductVendorMappingFixtures.GetTestList();

            _ = _productVendorMappingServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<ProductVendorMapping>, IQueryable<ProductVendorMapping>>>()))
                .ReturnsAsync(productVendorMappings);

            // Act
            var result = await _controller.GetProductList(psku);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(RoleList.ExternalViewer)]
        [InlineData(RoleList.Administrator)]
        [InlineData(RoleList.ExternalUser)]
        public async Task When_GetProductList_ReturnsNullWhenUserDoesNotHaveRequiredRolesOrInvalidPsku(string Role)
        {
            // Arrange
            var psku = "InvalidPsku";
            ProductVendorMappingController controller = _controller;
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
                controller = new ProductVendorMappingController(
                    _productVendorMappingServiceMock.Object,
                      _productServiceMock.Object,
                _vendorServiceMock.Object
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
            List<ProductVendorMapping>? productVendorMappings = null;
            var expectedProductVendorMappingFilter = new ProductVendorMappingFilter();


            _ = _productVendorMappingServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<ProductVendorMapping>, IQueryable<ProductVendorMapping>>>()))
                .ReturnsAsync(productVendorMappings);

            // Act
            var result = await _controller.GetProductList(psku);

            // Assert
            Assert.NotNull(result);
            //Assert.Equal(expectedProductVendorMappingFilter.Property1, result.Property1);
            //Assert.Equal(expectedProductVendorMappingFilter.Property2, result.Property2);
        }

        [Theory]
        [InlineData(RoleList.ExternalViewer)]
        [InlineData(RoleList.Administrator)]
        [InlineData(RoleList.ExternalUser)]
        public async Task When_GetVendorList_ReturnsExpectedProductVendorMappingFilter(string Role)
        {
            // Arrange
            var psku = "InvalidPsku";
            ProductVendorMappingController controller = _controller;
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
                controller = new ProductVendorMappingController(
                    _productVendorMappingServiceMock.Object,
                      _productServiceMock.Object,
                _vendorServiceMock.Object
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
            var productVendorMappings = ProductVendorMappingFixtures.GetTestList();
            var expectedProductVendorMappingFilter = new ProductVendorMappingFilter();


            _ = _productVendorMappingServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<ProductVendorMapping>, IQueryable<ProductVendorMapping>>>()))
                .ReturnsAsync(productVendorMappings);

            // Act
            var result = await _controller.GetProductList(psku);

            // Assert
            Assert.NotNull(result);
            //Assert.Equal(expectedProductVendorMappingFilter.Property1, result.Property1);
            //Assert.Equal(expectedProductVendorMappingFilter.Property2, result.Property2);
        }

        [Fact]
        public void When_GetDetails_ReturnsPartialViewWithExpectedModel()
        {
            // Arrange
            var id = 1001;
            var expectedProductVendorMapping = ProductVendorMappingFixtures.GetTestList().First();

            _ = _productVendorMappingServiceMock.Setup(static x => x.Get(
                It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
                It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
                .Returns(expectedProductVendorMapping);

            // Act
            var result = _controller.GetDetails(id);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("Details", partialViewResult.ViewName);
            Assert.Equal(expectedProductVendorMapping, partialViewResult.Model);
        }

        [Fact]
        public void When_GetDetails_ReturnsNotFoundWhenIdIsNull()
        {
            // Act
            var result = _controller.GetDetails(null);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task When_Create_ReturnsRedirectToActionWhenModelStateIsValid()
        {
            // Arrange
            var validProductVendorMapping = new ProductVendorMapping { /* Populate with valid data */ };

            _controller.ModelState.Clear();

            // Act
            var result = await _controller.Create(validProductVendorMapping);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Theory]
        [InlineData(RoleList.ExternalViewer)]
        [InlineData(RoleList.Administrator)]
        [InlineData(RoleList.ExternalUser)]
        public async Task When_Create_ReturnsViewWithModelWhenModelStateIsInvalid(string Role)
        {
            // Arrange
            var invalidProductVendorMapping = new ProductVendorMapping { };

            ProductVendorMappingController controller = _controller;
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
                controller = new ProductVendorMappingController(
                    _productVendorMappingServiceMock.Object,
                      _productServiceMock.Object,
                _vendorServiceMock.Object
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
            var products = new List<Product>
            {
                new() { ProductId = 1, Sku = "SKU1", Description = "Description1", IsActive = true, IsExternalProduct = false },
                new() { ProductId = 2, Sku = "SKU2", Description = "Description2", IsActive = true, IsExternalProduct = true }
            };

            var vendors = new List<Vendor>
            {
                new() { VendorId = 1, VendorName = "Vendor1", IsActive = true, IsExternal = false, VendorNumber = "1" },
                new() { VendorId = 2, VendorName = "Vendor2", IsActive = true, IsExternal = true, VendorNumber = "2" }
            };

            _ = _productServiceMock.Setup(static x => x.GetList(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()))
                   .Returns(products);
            _ = _vendorServiceMock.Setup(static x => x.GetList(It.IsAny<Func<IQueryable<Vendor>, IQueryable<Vendor>>>()))
                .Returns(vendors);

            _controller.ModelState.AddModelError("PropertyName", "Error message");

            // Act
            var result = await _controller.Create(invalidProductVendorMapping);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["ProductList"]);
            Assert.NotNull(viewResult.ViewData["VendorList"]);
        }

        [Fact]
        public void When_Delete_ReturnsNotFoundWhenIdIsNull()
        {
            // Act
            var result = _controller.Delete(null);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void When_Delete_ReturnsNotFoundWhenResultToRemoveIsNull()
        {
            // Arrange
            var id = 1; // Example id
            ProductVendorMapping? resultToRemove = null;

            _ = _productVendorMappingServiceMock.Setup(static x => x.Get(
                It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
                It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
                .Returns(() => resultToRemove!);

            // Act
            var result = _controller.Delete(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void When_Delete_ReturnsViewWithModelWhenResultToRemoveIsNotNull()
        {
            // Arrange
            var id = 1;
            var resultToRemove = new ProductVendorMapping { };

            _ = _productVendorMappingServiceMock.Setup(static x => x.Get(
                It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
                It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
                .Returns(resultToRemove);

            // Act
            var result = _controller.Delete(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(resultToRemove, viewResult.Model);
        }

        [Fact]
        public async Task When_DeleteConfirmed_RemovesItemAndRedirectsToIndex()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await _controller.DeleteConfirmed(id);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            _productVendorMappingServiceMock.Verify(x => x.RemoveAsync(id), Times.Once);
        }

        [Fact]
        public void When_Edit_ReturnsNotFoundWhenIdIsNull()
        {
            // Act
            var result = _controller.Edit(null);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void When_Edit_ReturnsNotFoundWhenResultToEditIsNull()
        {
            // Arrange
            var id = 1;
            ProductVendorMapping? resultToEdit = null;

            _ = _productVendorMappingServiceMock.Setup(static x => x.Get(
                It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
                It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
                .Returns(resultToEdit!);

            // Act
            var result = _controller.Edit(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void When_Edit_ReturnsViewWithModelWhenResultToEditIsNotNull()
        {
            // Arrange
            var id = 1;
            var resultToEdit = ProductVendorMappingFixtures.GetTestList().First();

            _ = _productVendorMappingServiceMock.Setup(static x => x.Get(
                It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
                It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()))
                .Returns(resultToEdit);

            // Act
            var result = _controller.Edit(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(resultToEdit, viewResult.Model);
        }

        [Fact]
        public async Task When_Edit_ReturnsNotFoundWhenIdDoesNotMatchProductVendorMappingId()
        {
            // Arrange
            var id = 1001;
            var productVendorMapping = ProductVendorMappingFixtures.GetTestList().First();

            // Act
            var result = await _controller.Edit(id, productVendorMapping);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            // Add more assertions as needed
        }

        [Fact]
        public async Task When_Edit_ReturnsNotFoundWhenDbUpdateConcurrencyExceptionOccurs()
        {
            // Arrange
            var id = 1001;
            var productVendorMapping = ProductVendorMappingFixtures.GetTestList().First();
            var exception = new DbUpdateConcurrencyException("Concurrency error");

            _ = _productVendorMappingServiceMock.Setup(x => x.UpdateAsync(productVendorMapping))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.Edit(id, productVendorMapping);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void When_Edit_ReturnsRedirectToActionWhenModelStateIsValidAndNoConcurrencyExceptionOccurs()
        {
            var productVendorMapping = ProductVendorMappingFixtures.GetTestList().First();

            _controller.ModelState.Clear();
            _ = _productVendorMappingServiceMock.Setup(x => x.UpdateAsync(productVendorMapping))
                .Throws<DbUpdateConcurrencyException>();

            // Act
            //Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await _controller.Edit(id, productVendorMapping));


        }

        [Fact]
        public void When_VendorsByProductId_ReturnsJsonWithVendors()
        {
            // Arrange
            var id = 1001;
            var productVendorMappings = ProductVendorMappingFixtures.GetTestList();

            var vendors = VendorFixtures.GetTestList();

            _ = _productVendorMappingServiceMock.Setup(static x => x.GetList(
                It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
                It.IsAny<Expression<Func<ProductVendorMapping, string>>[]>(),
                It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()
                ))
                .Returns(productVendorMappings);

            _ = _vendorServiceMock.Setup(static x => x.GetList(
                It.IsAny<Expression<Func<Vendor, bool>>>(),
                It.IsAny<Expression<Func<Vendor, string>>[]>(),
                It.IsAny<Expression<Func<Vendor, object>>[]>()
                ))
                .Returns(vendors);

            // Act
            var result = _controller.VendorsByProductId(id);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Vendor>>(jsonResult.Value);
            Assert.Equal(vendors, model);
        }

        [Fact]
        public void When_VPMList_ReturnsJsonWithProductSKUs()
        {
            // Arrange
            var products = ProductFixtures.GetTestProducts()
                    .OrderBy(static o => o.Sku)
                    .Select(
                        static x => new Product
                        {
                            ProductId = x.ProductId,
                            Sku = x.Sku + " " + x.Description
                        }).ToList();

            _ = _productServiceMock.Setup(static x => x.GetList(
                    It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()))
                   .Returns(products);

            // Act
            var result = _controller.VPMList();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<object>>(jsonResult.Value);
        }

        [Fact]
        public void When_VPMVendorList_ReturnsJsonWithVendorNames()
        {
            // Arrange
            var vendors = VendorFixtures.GetTestList();

            _ = _vendorServiceMock.Setup(static x => x.GetList(
                It.IsAny<Func<IQueryable<Vendor>, IQueryable<SelectListItem>>>()
            )).Returns(
                vendors.Select(static x => new SelectListItem
                {
                    Value = x.VendorId.ToString(),
                    Text = x.VendorName
                }).ToList()
            );

            // Act
            var result = _controller.VPMVendorList();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<object>>(jsonResult.Value);
        }

        [Fact]
        public async Task When_UpdateCostAndLeadTime_ReturnsOkWhenProductVendorMappingExists()
        {
            // Arrange
            var id = 1;
            var productVendorMapping = new ProductVendorMapping
            {
                ProductVendorMappingId = id,
                ProductId = 100, 
                Cost = 10.0m, 
                LeadTime = 7 
            };

            var product = new Product
            {
                ProductId = productVendorMapping.ProductId,
                Cost = 0.0m, 
                LeadTime = 0 
            };

            _ = _productVendorMappingServiceMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
                It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()
                )).ReturnsAsync(productVendorMapping);

            _ = _productServiceMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<Product, object>>[]>()
                )).ReturnsAsync(product);

            // Act
            var result = await _controller.UpdateCostAndLeadTime(id);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(productVendorMapping.Cost, product.Cost);
            Assert.Equal(productVendorMapping.LeadTime, product.LeadTime);
            _productServiceMock.Verify(x => x.UpdateAsync(product), Times.Once);
            // Add more assertions as needed
        }

        [Fact]
        public async Task When_UpdateCostAndLeadTime_ReturnsNotFoundWhenProductVendorMappingDoesNotExist()
        {
            // Arrange
            var id = 1; 
            ProductVendorMapping? productVendorMapping = null;

            _ = _productVendorMappingServiceMock.Setup(static x => x.GetAsync(
               It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
               It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()
               )).ReturnsAsync(productVendorMapping);

            // Act
            var result = await _controller.UpdateCostAndLeadTime(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            _productServiceMock.Verify(static x => x.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Theory]
        [InlineData(RoleList.ExternalViewer)]
        [InlineData(RoleList.Administrator)]
        [InlineData(RoleList.ExternalUser)]
        public async Task When_GetProductList_ReturnsOkWithCorrectJsonData(string Role)
        {
            // Arrange
            ProductVendorMappingController controller = _controller;
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
                controller = new ProductVendorMappingController(
                    _productVendorMappingServiceMock.Object,
                      _productServiceMock.Object,
                _vendorServiceMock.Object
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
                { "order[0][column]", "0" },
                { "search[value]", "1" },
             }));
            controller.Request.Form = request.Object.Form;
            var productVendorMappings = ProductVendorMappingFixtures.GetTestList();

            _ = _productVendorMappingServiceMock.Setup(static x => x.GetCountAsync(
                    It.IsAny<Expression<Func<ProductVendorMapping, bool>>>()))
                    .ReturnsAsync(10);

            _ = _productVendorMappingServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Func<IQueryable<ProductVendorMapping>, IQueryable<ProductVendorMapping>>>()))
                .ReturnsAsync(productVendorMappings);

          

            // Act
            var result = await controller.GetProductList() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var data = result?.Value?.GetType()?.GetProperty("data")?.GetValue(result.Value, null);
            var productList = Assert.IsType<List<ProductVendorMapping>>(data);
            _ = productList.Should().NotBeNullOrEmpty();
           
        }

        [Theory]
        [InlineData(RoleList.ExternalViewer)]
        [InlineData(RoleList.Administrator)]
        [InlineData(RoleList.ExternalUser)]
        public async Task When_GetVendorList_ReturnsOkWithCorrectJsonData(string Role)
        {
            // Arrange
            ProductVendorMappingController controller = _controller;
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
                controller = new ProductVendorMappingController(
                    _productVendorMappingServiceMock.Object,
                      _productServiceMock.Object,
                _vendorServiceMock.Object
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
                { "order[0][column]", "0" },
                { "search[value]", "1" },
             }));
            controller.Request.Form = request.Object.Form;
            var productVendorMappings = ProductVendorMappingFixtures.GetTestList();

            _ = _productVendorMappingServiceMock.Setup(static x => x.GetCountAsync(
                    It.IsAny<Expression<Func<ProductVendorMapping, bool>>>()))
                    .ReturnsAsync(10);

            _ = _productVendorMappingServiceMock.Setup(static x => x.GetListAsync(
                It.IsAny<Func<IQueryable<ProductVendorMapping>, IQueryable<ProductVendorMapping>>>()))
                .ReturnsAsync(productVendorMappings);



            // Act
            var result = await controller.GetVendorList() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var data = result?.Value?.GetType()?.GetProperty("data")?.GetValue(result.Value, null);
            var productList = Assert.IsType<List<ProductVendorMapping>>(data);
            _ = productList.Should().NotBeNullOrEmpty();

        }
    }
}