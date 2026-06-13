using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Orders;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace ERPWebApp.UnitTests.Services;
    [Trait("Category", "execute")]
    public class OrderItemServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IBundleRepository> _mockBundleRepository;
        private readonly IOrderItemService _orderItemService;
        private readonly Mock<ILogger<OrderItemService>> _mockLogger;


        public OrderItemServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockBundleRepository = new Mock<IBundleRepository>();
            _mockLogger = new Mock<ILogger<OrderItemService>>();
        _ = _mockUnitOfWork.Setup(static uow => uow.Products).Returns(_mockProductRepository.Object);
        _ = _mockUnitOfWork.Setup(static uow => uow.Bundles).Returns(_mockBundleRepository.Object);

        _ = _mockProductRepository
            .Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(static (Expression<Func<Product, bool>> predicate, Expression<Func<Product, object>>[] _) =>
            {
                var testProduct1 = new Product { ProductId = 1, Sku = "PCHIPBAG03ZSO01", IsActive = true, DepartmentList = [1], Departments = [new Department { DepartmentId = 1, DepartmentName = "Dep 1", IsProduction = true, IsActive = true }] };
                var testProduct2 = new Product { ProductId = 2, Sku = "PCHIPBAGBBQSO01", IsActive = true, DepartmentList = [2], Departments = [new Department { DepartmentId = 2, DepartmentName = "Dep 2", IsProduction = true, IsActive = true }] };
                var testProduct3 = new Product { ProductId = 3, Sku = "SKU-001", IsActive = true, DepartmentList = [3], Departments = [new Department { DepartmentId = 3, DepartmentName = "Dep 3", IsProduction = true, IsActive = true }] };
                var testProduct4 = new Product { ProductId = 4, Sku = "CHKSKU01", IsActive = true, DepartmentList = [4], Departments = [new Department { DepartmentId = 4, DepartmentName = "Dep 4", IsProduction = true, IsActive = true }] };
                var testProduct5 = new Product { ProductId = 5, Sku = "SKU001", IsActive = true, DepartmentList = [5], Departments = [new Department { DepartmentId = 5, DepartmentName = "Dep 5", IsProduction = true, IsActive = true }] };
                var testProduct6 = new Product { ProductId = 6, Sku = "SKU002", IsActive = true, DepartmentList = [6], Departments = [new Department { DepartmentId = 6, DepartmentName = "Dep 6", IsProduction = true, IsActive = true }] };
                var testProduct7 = new Product { ProductId = 7, Sku = "GIL.G500VL.L.BLACK", IsActive = true, DepartmentList = [6], Departments = [new Department { DepartmentId = 6, DepartmentName = "Dep 6", IsProduction = true, IsActive = true }] };
                var testProduct8 = new Product { ProductId = 8, Sku = "GIL.G2400.XL.NAVY", IsActive = true, DepartmentList = [6], Departments = [new Department { DepartmentId = 6, DepartmentName = "Dep 6", IsProduction = true, IsActive = true }] };
                var testProduct9 = new Product { ProductId = 9, Sku = "BEL.3739.L.NAVYBLUE", IsActive = true, DepartmentList = [6], Departments = [new Department { DepartmentId = 6, DepartmentName = "Dep 6", IsProduction = true, IsActive = true }] };
                var testProducts = new List<Product> { testProduct1, testProduct2, testProduct3, testProduct4, testProduct5, testProduct6, testProduct7, testProduct8, testProduct9 };

                return testProducts.FirstOrDefault(predicate.Compile());
            });
        _ = _mockBundleRepository
            .Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Bundle, bool>>>(), It.IsAny<Expression<Func<Bundle, object>>[]>()))
            .ReturnsAsync(static (Expression<Func<Bundle, bool>> predicate, Expression<Func<Bundle, object>>[] _) =>
            {
                var testBundle1 = new Bundle { BundleId = 1, BundleName = "BUNDLESO05" };
                var testBundle2 = new Bundle { BundleId = 2, BundleName = "BBQBOXSET" };
                var testBundleList = new List<Bundle> { testBundle1, testBundle2 };

                return testBundleList.FirstOrDefault(predicate.Compile());
            });
                _orderItemService = new OrderItemService(_mockUnitOfWork.Object,_mockLogger.Object);
        }

        //[Fact]
        //public async Task CustomProductSkuConversion_GetsProductSetSku_ReturnsOriginalOrderItem()
        //{
        //    // Arrange
        //    OrderItem orderItem = new()
        //    {
        //        sku = "CVHD020",
        //        quantity = 1,
        //        unitPrice = 22.22m,
        //        name = "Random Product",
        //        productId = 1,
        //        ERPProductId = null,
        //        ERPOrderId = 1,
        //        ERPOrderItemId = 1
        //    };

        //    // Act
        //    List<OrderItem> listOrderItems = await _orderItemService.CustomProductSkuConversion(orderItem);
        //    // Assert
        //    Assert.True(listOrderItems.Count == 1);
        //    Assert.Equal(orderItem, listOrderItems.First());
        //}

        //[Fact]
        //public async Task CustomProductSkuConversion_GetsProductSetSku_ReturnsListOfOrderItemsWithSetAsync()
        //{
        //    // Arrange
        //    OrderItem orderItem = new()
        //    {
        //        sku = "CS_3-CB_3",
        //        quantity = 2,
        //        unitPrice = 22.22m,
        //        name = "Custom Set",
        //        productId = 1,
        //        ERPProductId = null,
        //        ERPOrderId = 1,
        //        ERPOrderItemId = 1
        //    };

        //    // Act
        //    List<OrderItem> listOrderItems = await _orderItemService.CustomProductSkuConversion(orderItem);
        //    // Assert
        //    Assert.True(listOrderItems.Count == orderItem.sku.Split('-').Length);
        //    Assert.Equal(2, listOrderItems.Count(x =>
        //    x.Product != null && x.ERPProductId != null &&
        //    x.quantity == 6));
        //}
        [Fact]
        public async Task ConvertAttributeToProductSku_GetsProductConvertedSku_ReturnsItemAsync()
        {
            // Arrange
            OrderItem orderItem = new()
            {
                sku = "CS_3-CB_3",
                quantity = 2,
                unitPrice = 22.22m,
                fulfillmentSku = "style=hanes_mens_crew_longsleeve_5586&amp;color=navyblue&amp;size=a_xl&amp;design.areas=[zazzle_shirt_10x12_front]&amp;dark=true",
                productId = 1,
                ERPProductId = null,
                ERPOrderId = 1,
                ERPOrderItemId = 1
            };

            // Act
            OrderItem OrderItem = await _orderItemService.ConvertAttributeToProductSku(orderItem);
            // Assert
            Assert.NotNull(OrderItem);
        }

        [Fact]
        public async Task AssignProductIds_SetsOrderItemProductAndBundles_ReturnsUpdatedOrderItemsWithBundlesOrProducts()
        {
            // Arrange
            List<Order> listOrders = [.. OrderFixtures.GetTestOrders().Where(x=>x.ERPOrderId <5)];

            List<OrderItem> listOrderItems = [];

            // Act
            foreach (Order order in listOrders)
            {
                var orderitems = await _orderItemService.AssignProductIds(order.items);
                listOrderItems.AddRange(orderitems);
            }

            // Assert
            Assert.True(listOrderItems.Count() == listOrderItems.Count(static x => x.Product != null || x.Bundle != null));
            Assert.DoesNotContain(listOrderItems, static x => x.Product != null && x.Bundle != null);
        }
    }