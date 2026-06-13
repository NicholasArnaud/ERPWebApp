using ERPWebApp.Controllers.Inventory;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Inventory.SkuProperties;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Models.PurchaseOrders;
using ERPWebApp.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ERPWebApp.UnitTests.Controllers.Inventory
{

    [Trait("Category", "execute")]
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock = new();
        private readonly Mock<IDepartmentService> _departmentServiceMock = new();
        private readonly Mock<ISubCategoryService> _subCategoryServiceMock = new();
        private readonly Mock<IProductFilesMappingsService> _productFilesMappingsServiceMock = new();
        private readonly Mock<IFilesService> _filesServiceMock = new();
        private readonly Mock<IProductVendorMappingService> _productVendorMappingServiceMock = new();
        private readonly Mock<IStocksService> _stocksServiceMock = new();
        private readonly Mock<ISkuCategoryService> _skuCategoryServiceMock = new();
        private readonly Mock<ISkuColorService> _skuColorServiceMock = new();
        private readonly Mock<ISkuUnitOfMeasureService> _skuUnitOfMeasureService = new();
        private readonly Mock<IProductImageService> _productImageServiceMock = new();
        private readonly Mock<IPurchaseOrderService> _purchaseOrderServiceMock = new();
        private readonly Mock<IProductTagService> _productTagServiceMock = new();

        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, RoleList.BasicUser),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var mockHttpContext = new Mock<HttpContext>();
            var mockTempDataProvider = new Mock<ITempDataProvider>();

            _controller = new ProductsController(
                _productServiceMock.Object,
                _departmentServiceMock.Object,
                _subCategoryServiceMock.Object,
                _productImageServiceMock.Object,
                _filesServiceMock.Object,
                _productVendorMappingServiceMock.Object,
                _stocksServiceMock.Object,
                _skuCategoryServiceMock.Object,
                _skuColorServiceMock.Object,
                _skuUnitOfMeasureService.Object,
                _productFilesMappingsServiceMock.Object,
                _purchaseOrderServiceMock.Object,
                _productTagServiceMock.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = claimsPrincipal,
                        Session = new MockSession()
                    }
                },
                TempData = new TempDataDictionary(mockHttpContext.Object, mockTempDataProvider.Object)
            };
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(null)]
        public async Task Index_WhenSucceed_ShouldReturnViewWithProductIndexData(int? id)
        {
            //Arrange
            var products = ProductFixtures.GetTestProducts();
            var departments = DepartmentsFixtures.GetTestDepartments();
            var subCategories = SubCategoriesFixtures.GetTestSubCategories();
            var productTags = ProductTagsRegistryFixtures.GetTestProductTags();

            _ = _productServiceMock.Setup(x => x.GetAllAsync(
                It.IsAny<Expression<Func<Product, string>>[]>(),
                It.IsAny<Expression<Func<Product, object>>[]>()
            )).ReturnsAsync(products);

            _ = _departmentServiceMock.Setup(x => x.GetListAsync(It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>())).ReturnsAsync(departments);
            _ = _subCategoryServiceMock.Setup(x => x.GetListAsync(It.IsAny<Func<IQueryable<SubCategory>, IQueryable<SubCategory>>>())).ReturnsAsync(subCategories);
            _ = _productTagServiceMock.Setup(x => x.GetListAsync(It.IsAny<Func<IQueryable<ProductTagsRegistry>, IQueryable<ProductTagsRegistry>>>())).ReturnsAsync(productTags);

            //Act
            var result = await _controller.Index(id) as ViewResult;

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ProductIndexData>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            _ = model.Products.Should().BeEquivalentTo(products);
            if (id != null)
            {
                _ = model.Departments.Should().BeEquivalentTo(products.Single(i => i.ProductId == id.Value).Departments.ToList());
            }

            _productServiceMock.Verify(p => p.GetAllAsync(
                It.IsAny<Expression<Func<Product, string>>[]>(),
                It.IsAny<Expression<Func<Product, object>>[]>()
            ), Times.Once);

            //to verify the invocation of get departments twice
            _departmentServiceMock.Verify(d => d.GetListAsync(It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()), Times.Once);

            _subCategoryServiceMock.Verify(s => s.GetListAsync(It.IsAny<Func<IQueryable<SubCategory>, IQueryable<SubCategory>>>()), Times.Once);
            _productTagServiceMock.Verify(t => t.GetListAsync(It.IsAny<Func<IQueryable<ProductTagsRegistry>, IQueryable<ProductTagsRegistry>>>()), Times.Once);

            // TO DO: Commented out these in the base method for handling the data protection issue. Will need to re-enable these if we go along with enabling data protection.
            //var departmentList = Assert.IsAssignableFrom<MultiSelectList>(viewResult.ViewData["DepartmentList"]);
            //Assert.NotNull(departmentList);
            //Assert.Equal(departmentList.Count(), departments.Count());

            //var subCatList = Assert.IsAssignableFrom<MultiSelectList>(viewResult.ViewData["SubCatList"]);
            //Assert.NotNull(subCatList);
            //Assert.Equal(subCatList.Count(), subCategories.Count());

            //var productTagList = Assert.IsAssignableFrom<MultiSelectList>(viewResult.ViewData["ProductTagList"]);
            //Assert.NotNull(productTagList);
            //Assert.Equal(productTagList.Count(), productTags.Count());
        }

        [Theory]
        [InlineData("true")]
        public void DeptList_WhenSucceed_ShouldReturnDepartmentsJsonResult(string id)
        {
            //Arrange
            var departments = DepartmentsFixtures.GetTestDepartments();
            departments = id == "true" ? departments.Where(static x => x.IsProduction).OrderBy(static x => x.DepartmentName).ToList()
                                        : departments.OrderBy(static x => x.DepartmentName).ToList();

            _ = _departmentServiceMock.Setup(static x => x.GetList(It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>())).Returns(departments);

            //Act
            var result = _controller.DeptList(id) as JsonResult;

            //Assert
            Assert.NotNull(result);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectList = Assert.IsType<SelectList>(jsonResult.Value);

            Assert.Equal(departments.Count, selectList.Count());
        }

        [Fact]
        public void GetMyProducts_WhenSucceed_ShouldReturnOkResult()
        {
            //Arrange
            var products = ProductFixtures.GetTestProducts();

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

            _ = _productServiceMock.Setup(
                static x => x.QueryFilter(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>())
            ).Returns(products.AsQueryable());

            _ = _stocksServiceMock.Setup(static x => x.QueryFilter(
                It.IsAny<Func<IQueryable<Stock>, IQueryable<Product>>>()
            )).Returns(products.AsQueryable());

            //Act
            var result = _controller.GetMyProducts(null, null, null, true, true) as OkObjectResult;

            //Assert
            Assert.NotNull(result);
            var data = result?.Value?.GetType().GetProperty("data")?.GetValue(result.Value, null);
            Assert.NotNull(data);
            var productList = Assert.IsType<List<ProductDTO>>(data);
            _ = productList.Should().NotBeNullOrEmpty();

            _productServiceMock.Verify(
                static x => x.QueryFilter(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()),
                Times.Once
            );

            _stocksServiceMock.Verify(
                static x => x.QueryFilter(It.IsAny<Func<IQueryable<Stock>, IQueryable<Product>>>()),
                Times.Once
            );
        }

        [Fact]
        public void GetDetails_WhenIdNotFound_ShouldReturnNotFoundResult()
        {
            //Arrange
            int? productId = null;

            //Act
            var result = _controller.GetDetails(productId);

            //Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetDetails_WhenSucceed_ShouldReturnPartialView()
        {
            //Arrange
            var product = ProductFixtures.GetTestProducts().FirstOrDefault(x => x.ProductId == 1);

            _ = _productServiceMock.Setup(x => x.Get(It.IsAny<Func<IQueryable<Product>, IQueryable<ProductDTO>>>()))
            .Returns(
                new ProductDTO
                {
                    ProductId = product?.ProductId ?? throw new InvalidOperationException("Product cannot be null."),
                    Sku = product.Sku
                }
            );

            //Act
            var result = _controller.GetDetails(1);

            //Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("Details", partialViewResult.ViewName);

            var model = Assert.IsType<ProductDTO>(partialViewResult.Model);
            Assert.Equal(product.ProductId, model.ProductId);
        }

        [Fact]
        public async Task Details_WhenIdNull_ShouldReturnNotFoundResult()
        {
            //Arrange
            int? productId = null;

            //Act
            var result = await _controller.Details(productId);

            //Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_WhenProductNotFound_ShouldReturnNotFoundResult()
        {
            //Arrange
            _ = _productServiceMock.Setup(static x => x.GetAsync(It.IsAny<Func<IQueryable<Product>, IQueryable<ProductDTO?>>?>()))
            .ReturnsAsync(null as ProductDTO);

            //Act
            var result = await _controller.Details(1);

            //Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_WhenSucceed_ShouldReturnViewResult()
        {
            //Arrange
            int? productId = 1;
            var product = ProductFixtures.GetTestProducts().FirstOrDefault(x => x.ProductId == productId);

            _ = _productServiceMock.Setup(x => x.GetAsync(It.IsAny<Func<IQueryable<Product>, IQueryable<ProductDTO>>>()))
            .ReturnsAsync(
                new ProductDTO
                {
                    ProductId = product?.ProductId ?? throw new InvalidOperationException("Product cannot be null."),
                    Sku = product.Sku
                }
            );

            _ = _purchaseOrderServiceMock.Setup(x => x.GetActivePurchaseOrdersByProductAsync(It.IsAny<int>()))
            .ReturnsAsync([]);

            _ = _productVendorMappingServiceMock.Setup(x => x.GetListAsync(
                It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
                It.IsAny<Expression<Func<ProductVendorMapping, string>>[]>(),
                It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()
            )).ReturnsAsync([]);

            //Act
            var result = await _controller.Details(productId);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProductDTO>(viewResult.Model);

            Assert.Equal(product.ProductId, model.ProductId);
        }

        [Fact]
        public void Create_WhenSucceed_ShouldReturnViewResult()
        {
            //Arrange
            var departments = DepartmentsFixtures.GetTestDepartments();
            var orderedDepartmentList = departments.OrderBy(static x => x.DepartmentName).ToList();
            var subCategories = SubCategoriesFixtures.GetTestSubCategories();

            _ = _departmentServiceMock.Setup(static x => x.GetList(It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>())).Returns(orderedDepartmentList);
            _ = _subCategoryServiceMock.Setup(static x => x.GetList(It.IsAny<Func<IQueryable<SubCategory>, IQueryable<SubCategory>>>())).Returns(subCategories);

            //Act
            var result = _controller.Create();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            _departmentServiceMock.Verify(static x => x.GetList(It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()), Times.Once);
            _subCategoryServiceMock.Verify(static x => x.GetList(It.IsAny<Func<IQueryable<SubCategory>, IQueryable<SubCategory>>>()), Times.Once);

            var departmentList = Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["DepartmentList"]);
            Assert.NotNull(departmentList);
            Assert.Equal(departmentList.Count(), orderedDepartmentList.Count());

            var subCatList = Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["SubCategoryList"]);
            Assert.NotNull(subCatList);
            Assert.Equal(subCatList.Count(), subCategories.Count());
        }

        [Fact]
        public async Task Create_async_WhenSucceed_ShouldReturnActionResult()
        {
            //Arrange
            var multipleFiles = new Mock<List<IFormFile>>();
            var file = new Mock<IFormFile>();
            var sourceImg = File.OpenRead(@"../../../Fixtures/files/test_sample.jpg");
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(sourceImg);
            writer.Flush();
            ms.Position = 0;
            var fileName = "test_sample.jpg";
            file.Setup(f => f.FileName).Returns(fileName).Verifiable();
            file.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream))
                .Verifiable();
            multipleFiles.Object.Add(file.Object);

            var product = ProductFixtures.GetTestProducts().FirstOrDefault(x => x.ProductId == 1) ?? new Product();
            var files = FilesFixtures.GetTestFiles();

            _ = _productServiceMock.Setup(x => x.AddAsync(product)).ReturnsAsync(product);
            _ = _filesServiceMock.Setup(x => x.AddAsync(It.IsAny<Files>())).ReturnsAsync(new Files());
            _ = _productImageServiceMock.Setup(x => x.AddAsync(It.IsAny<ProductImage>())).ReturnsAsync(new ProductImage());

            _ = _departmentServiceMock.Setup(x => x.UpdateProductDepartments(It.IsAny<string[]>(), It.IsAny<Product>()))
                                  .Returns(product);

            _ = _filesServiceMock.Setup(x => x.UploadToAzureAsync(It.IsAny<IFormFile>(), It.IsAny<FileType>())).ReturnsAsync("");
            _ = _filesServiceMock.Setup(x => x.UploadThumbnailToAzureAsync(It.IsAny<IFormFile>())).ReturnsAsync("");

            product = product ?? new Product();
            multipleFiles = multipleFiles != null ? multipleFiles : new Mock<List<IFormFile>>();
            file = file != null ? file : new Mock<IFormFile>();

            //Act
            var result = await _controller.Create(
                product,
                multipleFiles.Object,
                file.Object
            ) as RedirectToActionResult;

            //Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            _productServiceMock.Verify(x => x.AddAsync(product), Times.Once);
        }

        [Theory]
        [InlineData(new string[] { "Any", "Sales", "Marketing" }, true)]
        [InlineData(new string[] { "Sales", "Marketing" }, false)]
        public async Task GetProductsByDepartment_WhenSucceed_ShouldReturnProductIndexData(string[] departments, bool hasAny)
        {
            //Arrange
            var products = ProductFixtures.GetTestProducts();
            var selectedContainers = new List<Product>();
            if (hasAny)
            {
                _ = _productServiceMock.Setup(x => x.GetAllAsync(
                It.IsAny<Expression<Func<Product, string>>[]>(),
                It.IsAny<Expression<Func<Product, object>>[]>()
                )).ReturnsAsync(products);
            }
            else
            {
                products = products.Where(x => x.Departments.Any(x => x.DepartmentName.Contains(departments[0]))).ToList();
                _ = _productServiceMock.Setup(x => x.GetListAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                     It.IsAny<Expression<Func<Product, string>>[]>(),
                    It.IsAny<Expression<Func<Product, object>>[]>()
                )).ReturnsAsync(products);
            }

            selectedContainers.AddRange(products);

            //Act
            var result = await _controller.GetProductsByDepartment(departments);

            //Assert
            _ = result.Should().NotBeNull();
            _ = result.Should().BeOfType<ProductIndexData>();
            _ = result.Products.Should().BeEquivalentTo(products);
        }

        [Fact]
        public async Task Edit_WhenIdNull_ShouldReturnNotFoundResult()
        {
            //Arrange
            int? productId = null;

            //Act
            var result = await _controller.Edit(productId);

            //Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WhenProductNotFound_ShouldReturnNotFoundResult()
        {
            //Arrange
            int? productId = 1;
            _ = _productServiceMock.Setup(static x => x.GetAsync(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()))
            .ReturnsAsync(null as Product);

            //Act
            var result = await _controller.Edit(productId);

            //Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WhenSucceed_ShouldReturnViewResult()
        {
            //Arrange
            int? productId = 1;
            var product = ProductFixtures.GetTestProducts().FirstOrDefault(x => x.ProductId == productId);
            var departments = DepartmentsFixtures.GetTestDepartments();
            var subCategories = SubCategoriesFixtures.GetTestSubCategories()
                .Where(x => x.SubCategoryId != product.SubCategory.SubCategoryId)
                .Select(x => (object)new { SubCategoryId = x.SubCategoryId, Description = x.Description })
                .ToList();

            _ = _productServiceMock.Setup(x => x.GetAsync(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>())).ReturnsAsync(product);

            _ = _departmentServiceMock.Setup(x => x.GetList(It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>())).Returns(departments);
            _ = _subCategoryServiceMock.Setup(x => x.GetList(It.IsAny<Func<IQueryable<SubCategory>, IQueryable<object>>>())).Returns(subCategories);

            _ = _productVendorMappingServiceMock.Setup(
                x => x.GetList(
                    It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
                    It.IsAny<Expression<Func<ProductVendorMapping, string>>[]>(),
                    It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()
                )
            ).Returns([]);

            //Act
            var result = await _controller.Edit(productId);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var departmentList = Assert.IsAssignableFrom<MultiSelectList>(viewResult.ViewData["DepartmentList"]);
            Assert.NotNull(departmentList);
            Assert.Equal(departmentList.Count(), departments.Count());

            var subCatList = Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["SubCategoryList"]);
            Assert.NotNull(subCatList);

            _ = Assert.IsAssignableFrom<string>(viewResult.ViewData["IsPrimaryVendor"]).Should().Be("no");

            _productServiceMock.Verify(p => p.GetAsync(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()), Times.Once);

            _departmentServiceMock.Verify(p => p.GetList(It.IsAny<Func<IQueryable<Department>, IQueryable<Department>>>()), Times.Once);

            _subCategoryServiceMock.Verify(d => d.GetList(It.IsAny<Func<IQueryable<SubCategory>, IQueryable<object>>>()), Times.Once);

            _productVendorMappingServiceMock.Verify(d => d.GetList(
                    It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(),
                    It.IsAny<Expression<Func<ProductVendorMapping, string>>[]>(),
                    It.IsAny<Expression<Func<ProductVendorMapping, object>>[]>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task BatchUserUpload_WhenInvalidFileExtensionFound_ShouldReturnError()
        {
            // Arrange
            var file = new Mock<IFormFile>();
            var sourceImg = File.OpenRead(@"../../../Fixtures/files/incorrectExtension.csv");
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(sourceImg);
            writer.Flush();
            ms.Position = 0;
            var fileName = "incorrectExtension.csv";
            file.Setup(f => f.FileName).Returns(fileName).Verifiable();
            file.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream))
                .Verifiable();
            _ = file.Setup(_ => _.Length).Returns(ms.Length);

            // Act
            var result = await _controller.BatchUserUpload(file.Object) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Please upload a valid Excel (.xlsx) file.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task EditAsync_WhenTryUpdateModelAsyncSucceed_ShouldRedirectToAction()
        {
            //Arrange
            var product = ProductFixtures.GetTestProducts().FirstOrDefault(static x => x.ProductId == 1) ?? new Product();
            var files = FilesFixtures.GetTestFiles();
            var productFilesMappings = ProductFilesMappingsFixtures.GetTestProductFilesMappings();

            _ = _productServiceMock.Setup(
                static x => x.Get(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<Expression<Func<Product, object>>[]>()
                )
            ).Returns(product);

            var mockController = new Mock<ProductsController>(
                _productServiceMock.Object,
                _departmentServiceMock.Object,
                _subCategoryServiceMock.Object,
                _productImageServiceMock.Object,
                _filesServiceMock.Object,
                _productVendorMappingServiceMock.Object,
                _stocksServiceMock.Object,
                _skuCategoryServiceMock.Object,
                _skuColorServiceMock.Object,
                _skuUnitOfMeasureService.Object,
                _productFilesMappingsServiceMock.Object,
                _purchaseOrderServiceMock.Object,
                _productTagServiceMock.Object
            )
            {
                CallBase = true
            };

            mockController.Object.ControllerContext = _controller.ControllerContext;

            _ = mockController.Setup(
                static c => c.TryUpdateModelAsync(
                    It.IsAny<Product>()
                )
            )
            .ReturnsAsync(true);

            _ = _productImageServiceMock.Setup(static x => x.GetAsync(
                It.IsAny<Expression<Func<ProductImage, bool>>>(),
                It.IsAny<Expression<Func<ProductImage, object>>[]>()
            )).ReturnsAsync(new ProductImage());

            _ = _filesServiceMock.Setup(static x => x.RemoveAzureBlobAsync(It.IsAny<string>(), It.IsAny<FileType>())).ReturnsAsync(true);
            _ = _productImageServiceMock.Setup(static x => x.RemoveAsync(It.IsAny<int>())).ReturnsAsync(1);
            _ = _filesServiceMock.Setup(static x => x.RemoveAsync(It.IsAny<int>())).ReturnsAsync(1);
            _ = _filesServiceMock.Setup(static x => x.UploadToAzureAsync(It.IsAny<IFormFile>(), It.IsAny<FileType>())).ReturnsAsync("");
            _ = _filesServiceMock.Setup(static x => x.UploadThumbnailToAzureAsync(It.IsAny<IFormFile>())).ReturnsAsync("");
            _ = _filesServiceMock.Setup(static x => x.AddAsync(It.IsAny<Files>())).ReturnsAsync(new Files());
            _ = _productImageServiceMock.Setup(static x => x.AddAsync(It.IsAny<ProductImage>())).ReturnsAsync(new ProductImage());
            _ = _departmentServiceMock.Setup(static x => x.UpdateProductDepartments(It.IsAny<string[]>(), It.IsAny<Product>())).Returns(product);
            _ = _productServiceMock.Setup(static x => x.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(1);

            //Act
            var result = await mockController.Object.EditAsync(product, 1, null, null, 1) as RedirectToActionResult;

            //Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public void GetDelete_WhenProductNotFound_ShouldReturnNotFoundResult()
        {
            //Arrange
            _ = _productServiceMock.Setup(x => x.Get(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()))
            .Returns(null as Product);

            //Act
            var result = _controller.GetDelete(1);

            //Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetDelete_WhenSucceed_ShouldReturnPartialViewResult()
        {
            //Arrange
            var product = ProductFixtures.GetTestProducts().First();
            _ = _productServiceMock.Setup(x => x.Get(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()))
            .Returns(product);

            //Act
            var result = _controller.GetDelete(1);

            //Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("Delete", partialViewResult.ViewName);

            var model = Assert.IsType<Product>(partialViewResult.Model);
            Assert.Equal(product.ProductId, model.ProductId);
        }

        [Fact]
        public async Task Delete_WhenProductNotFound_ShouldReturnNotFoundResult()
        {
            //Arrange
            _ = _productServiceMock.Setup(static x => x.GetAsync(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()))
            .ReturnsAsync(null as Product);

            //Act
            var result = await _controller.Delete(1);

            //Assert
            _ = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WhenSucceed_ShouldReturnViewResult()
        {
            //Arrange
            var product = ProductFixtures.GetTestProducts().First();
            _ = _productServiceMock.Setup(static x => x.GetAsync(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()))
            .ReturnsAsync(product);

            //Act
            var result = await _controller.Delete(1);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var productResult = Assert.IsAssignableFrom<Product>(viewResult.ViewData.Model);
            Assert.NotNull(productResult);
            Assert.Equal(productResult.ProductId, product.ProductId);
        }

        [Fact]
        public async Task DeleteImage_WithValidId_DeletesFilesAndMappings()
        {
            // Arrange
            int id = 1;
            var files = FilesFixtures.GetTestFiles();
            var image = ProductImageFixtures.GetProductImageFixtures().First();

            _ = _productImageServiceMock.Setup(static x => x.GetAsync(
                It.IsAny<Expression<Func<ProductImage, bool>>>(),
                It.IsAny<Expression<Func<ProductImage, object>>[]>()
            )).ReturnsAsync(image);

            _ = _filesServiceMock.Setup(static x => x.RemoveAzureBlobAsync(It.IsAny<string>(), It.IsAny<FileType>())).ReturnsAsync(true);
            _ = _productImageServiceMock.Setup(static x => x.RemoveAsync(It.IsAny<int>())).ReturnsAsync(1);
            _ = _filesServiceMock.Setup(static x => x.RemoveAsync(It.IsAny<int>())).ReturnsAsync(1);


            // Act
            var result = await _controller.DeleteImage(1);

            // Assert
            _ = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", ((RedirectToActionResult)result).ActionName);
            Assert.NotNull(result);
            Assert.NotNull(((RedirectToActionResult)result).RouteValues);
            Assert.Equal(id, ((RedirectToActionResult)result).RouteValues["id"]);
        }


        #region Unit Tests for GetSkuUnitOfMeasure

        [Fact]
        public async Task GetSkuUnitOfMeasure_ReturnsListOfActiveItems()
        {
            var activeItems = SkuUnitOfMeasureFixtures.GetTestUnitOfMeasures();

            _ = _skuUnitOfMeasureService
                .Setup(static x => x.GetListAsync(It.IsAny<Expression<Func<SkuUnitOfMeasure, bool>>>(), null, null))
                .ReturnsAsync(activeItems);

            var result = await _controller.GetSkuUnitOfMeasure();

            _ = Assert.IsAssignableFrom<IEnumerable<SkuUnitOfMeasure>>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<SkuUnitOfMeasure>>(result);

            Assert.Equal(activeItems, model);
        }

        [Fact]
        public async Task GetSkuUnitOfMeasure_ReturnsEmptyListWhenNoActiveItems()
        {
            _ = _skuUnitOfMeasureService
                .Setup(static x => x.GetListAsync(It.IsAny<Expression<Func<SkuUnitOfMeasure, bool>>>(), null, null))
                .ReturnsAsync([]);

            var result = await _controller.GetSkuUnitOfMeasure();

            _ = Assert.IsAssignableFrom<IEnumerable<SkuUnitOfMeasure>>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<SkuUnitOfMeasure>>(result);

            Assert.Empty(model);
        }

        #endregion


        #region Unit Tests for UpdateCostAndLeadTime

        [Fact]
        public async Task UpdateCostAndLeadTime_ValidId_ReturnsOk()
        {
            var existingProductId = 1;

            //setup ProductVendorMapping
            var existingProductVendorMapping = ProductVendorMappingFixtures.GetTestList().First(v => v.ProductId == existingProductId && v.isPrimaryVendor && v.IsActive);
            _ = _productVendorMappingServiceMock.Setup(x => x.Get(It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(), null))
                            .Returns(existingProductVendorMapping);

            //setup product
            var existingProduct = ProductFixtures.GetTestProducts().First(p => p.ProductId == existingProductId);
            _ = _productServiceMock.Setup(x => x.Get(It.IsAny<Expression<Func<Product, bool>>>(), null))
                            .Returns(existingProduct);

            var result = await _controller.UpdateCostAndLeadTime(existingProductId);

            _ = Assert.IsType<OkResult>(result);

            _productServiceMock.Verify(x => x.UpdateAsync(It.IsAny<Product>()), Times.Once);
            Assert.Equal(existingProductVendorMapping.Cost, existingProduct.Cost);
            Assert.Equal(existingProductVendorMapping.LeadTime, existingProduct.LeadTime);
        }

        [Fact]
        public async Task UpdateCostAndLeadTime_InvalidId_ReturnsNotFound()
        {
            var nonExistingProductId = 999;

            //setup ProductVendorMapping
            var nonExistingProductVendorMapping = ProductVendorMappingFixtures.GetTestList().FirstOrDefault(v => v.ProductId == nonExistingProductId && v.isPrimaryVendor && v.IsActive);
            _ = _productVendorMappingServiceMock.Setup(x => x.Get(It.IsAny<Expression<Func<ProductVendorMapping, bool>>>(), null))
                            .Returns((ProductVendorMapping)null!);

            //setup product
            var nonExistingProduct = ProductFixtures.GetTestProducts().FirstOrDefault(p => p.ProductId == nonExistingProductId);
            _ = _productServiceMock.Setup(x => x.Get(It.IsAny<Expression<Func<Product, bool>>>(), null))
                            .Returns(nonExistingProduct ?? new Product());

            var result = await _controller.UpdateCostAndLeadTime(nonExistingProductId);

            _ = Assert.IsType<NotFoundResult>(result);
            _productServiceMock.Verify(x => x.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        #endregion


        #region Unit Tests for UnAssignProductTag

        [Fact]
        public async Task UnAssignProductTag_ValidInput_ReturnsOkResult()
        {
            int testProductId = 1;

            //setup ProductVendorMapping
            var productTagRegistry = ProductTagsRegistryFixtures.GetTestProductTags().First();
            _ = _productTagServiceMock.Setup(x => x.AssignProductTagAsync(productTagRegistry, testProductId))
                            .Returns(Task.CompletedTask);

            var result = await _controller.UnAssignProductTag(testProductId, productTagRegistry.TagId);

            _ = Assert.IsType<OkResult>(result);
        }

        #endregion


        #region Unit Tests for GetAllProducts

        [Fact]
        public async Task GetAllProducts_ValidQueryString_ReturnsMatchingProducts()
        {
            var products = ProductFixtures.GetTestProducts();

            _ = _productServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()))
                    .ReturnsAsync(products);

            var result = await _controller.GetAllProducts("Product_02");

            _ = Assert.IsType<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var resultList = okResult.Value as List<Product>;

            Assert.NotNull(resultList);
            Assert.Equal(2, resultList.Count);
        }

        [Fact]
        public async Task GetAllProducts_NullProductService_ReturnsNull()
        {
            _ = _productServiceMock.Setup(static x => x.GetListAsync(It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>()))
                .ReturnsAsync((List<Product>?)null);


            var result = await _controller.GetAllProducts("nullResult");

            Assert.Null(result);
        }

        #endregion
    }
}
