using ERPWebApp.Models.Company;
using System.Linq.Expressions;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.UnitTests.Services
{
    [Trait("Category", "execute")]
    public class OrderBatchServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IStockRepository> _mockStockRepository;
        private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
        private readonly Mock<IOrderBatchRepository> _mockOrderBatchRepository;
        private readonly Mock<IOrderBatchItemRepository> _mockOrderBatchItemRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly IOrderBatchService _orderBatchService;
        private readonly Mock<IUserSiteMappingService> _mockUserSiteMappingService;
        private readonly Mock<IOrderBatchService> _mockOrderBatchService;
        private readonly Mock<IMoveStockHistoryRepository> _mockMoveStockHistoryRepository;

        public OrderBatchServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockDepartmentRepository = new Mock<IDepartmentRepository>();
            _mockOrderBatchRepository = new Mock<IOrderBatchRepository>();
            _mockOrderBatchItemRepository = new Mock<IOrderBatchItemRepository>();
            _mockStockRepository = new Mock<IStockRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUserSiteMappingService = new Mock<IUserSiteMappingService>();
            _mockOrderBatchService = new Mock<IOrderBatchService>();
            _mockMoveStockHistoryRepository = new Mock<IMoveStockHistoryRepository>();

            _ = _mockUnitOfWork.Setup(uow => uow.Orders).Returns(_mockOrderRepository.Object);
            _ = _mockUnitOfWork.Setup(uow => uow.Departments).Returns(_mockDepartmentRepository.Object);
            _ = _mockUnitOfWork.Setup(uow => uow.OrderBatch).Returns(_mockOrderBatchRepository.Object);
            _ = _mockUnitOfWork.Setup(uow => uow.Products).Returns(_mockProductRepository.Object);
            _ = _mockUnitOfWork.Setup(uow => uow.Stocks).Returns(_mockStockRepository.Object);
            _ = _mockUnitOfWork.Setup(uow => uow.MoveStockHistories).Returns(_mockMoveStockHistoryRepository.Object);

            // Mock for FilterOneAsync in ProductRepository  
            _ = _mockProductRepository.Setup(repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync((Expression<Func<Product, bool>> predicate, Expression<Func<Product, object>>[] includes) =>
                {
                    var products = new List<Product>
                    {
                        new() {
                            ProductId = 1,
                            Sku = "SKU001",
                            IsActive = true,
                            Departments =
                            [
                                new() { DepartmentId = 1, DepartmentName = "Department1" }
                            ]
                        },
                        new() {
                            ProductId = 2,
                            Sku = "SKU002",
                            IsActive = true,
                            Departments =
                            [
                                new() { DepartmentId = 2, DepartmentName = "Department2" }
                            ]
                        },
                        new() {
                            ProductId = 3,
                            Sku = "Discount",
                            IsActive = true,
                            Departments =
                            [
                                new() { DepartmentId = 3, DepartmentName = "Department3" }
                            ]
                        },
                        new() {
                            ProductId = 4,
                            Sku = "InactiveSKU",
                            IsActive = false
                        }
                    };
                    return products.AsQueryable().FirstOrDefault(predicate);
                });

            _ = _mockOrderBatchRepository.Setup(repo => repo.FilterOneAsync(It.IsAny<Expression<Func<OrderBatch, bool>>>(), It.IsAny<Expression<Func<OrderBatch, object>>[]>()))
                .ReturnsAsync((Expression<Func<OrderBatch, bool>> predicate, Expression<Func<OrderBatch, object>>[] includes) =>
                {
                    var orderBatches = new List<OrderBatch>
                    {
                        new() {
                            OrderBatchId = 1,
                            Status = OrderBatchStatus.InProgress
                        }
                    };
                    return orderBatches.AsQueryable().FirstOrDefault(predicate);
                });

            // Mock the CreateOrderBatch method  
            _ = _mockOrderBatchRepository.Setup(repo => repo.CreateOrderBatch(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BatchType>(), It.IsAny<bool>()))
                .ReturnsAsync((string batchNumber, string user, BatchType? batchType, bool isDeductible) =>
                {
                    var orderBatch = new OrderBatch
                    {
                        OrderBatchId = 1,
                        BatchNumber = batchNumber + " 1",
                        CreateDate = DateTime.UtcNow,
                        CreateBy = user,
                        Status = OrderBatchStatus.Open,
                        Type = batchType,
                        IsDeductible = isDeductible
                    };
                    return orderBatch;
                });

            // Mock the CreateOrderBatchItems method  
            _ = _mockOrderBatchRepository.Setup(repo => repo.CreateOrderBatchItems(It.IsAny<int>(), It.IsAny<List<InventoryPickList>>(), It.IsAny<bool>()))
                .ReturnsAsync((int orderBatchId, List<InventoryPickList> inventoryPickList, bool isDeductible) =>
                {
                    return inventoryPickList.Select(item => new OrderBatchItem
                    {
                        OrderBatchId = orderBatchId,
                        ProductId = item.ERPProductId,
                        ERPOrderItemId = item.ERPOrderItemId,
                        ERPOrderId = item.ERPOrderId,
                        OrderNumber = item.OrderNumber,
                        Quantity = item.Quantity,
                        BatchItemStatusId = 1,
                        IsPicked = !isDeductible
                    }).ToList();
                });

            // Mock the ExecuteTransactionAsync method  
            _ = _mockOrderBatchRepository.Setup(repo => repo.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(async action =>
                {
                    await action();
                    return true;
                });

            // Mock the Update method
            _ = _mockOrderBatchRepository.Setup(repo => repo.Update(It.IsAny<OrderBatch>()))
                .Returns((OrderBatch orderBatch) => orderBatch);

            // Mock the GetRepository method to return the mocked OrderBatchRepository
            _ = _mockUnitOfWork.Setup(uow => uow.GetRepository<OrderBatch>()).Returns(_mockOrderBatchRepository.Object);

            _orderBatchService = new OrderBatchService(_mockUnitOfWork.Object, _mockUserSiteMappingService.Object);
        }

        #region CreateBatchAsync
        [Fact]
        public async Task CreateBatchAsync_ValidInput_ReturnsSuccess()
        {
            // Arrange  
            var ERPOrderIds = new List<int> { 1, 2, 3 };
            var BatchType = 1;
            var BatchName = "Test Batch";
            var User = "TestUser";
            var assignedDepartments = new List<AssignedDepartment>
            {
                new() { OrderItemId = 1, AssignedDepartmentId = 1 },
                new() { OrderItemId = 2, AssignedDepartmentId = 2 }
            };
            var replacementSkus = new List<ReplacementSku>
            {
                new() { OriginalSku = "SKU001", NewSku = "SKU002", NewPID = 2 }
            };
            var orders = new List<Order>
            {
                new() {
                    orderId = 1,
                    items =
                    [
                        new OrderItem
                        {
                            sku = "SKU001",
                            quantity = 1,
                            unitPrice = 10m,
                            Product = new Product { ProductId = 1, Sku = "SKU001", Departments = [] },
                            ERPProductId = 1
                        }
                    ]
                }
            };
            _ = _mockOrderBatchRepository.Setup(repo => repo.GetOrdersWithProductsByERPOrderIdsAsync(ERPOrderIds))
                .ReturnsAsync(orders);

            // Act  
            var result = await _orderBatchService.CreateBatchAsync(ERPOrderIds, BatchType, BatchName, assignedDepartments, replacementSkus, User);

            // Assert  
            Assert.True(result.Success);
            Assert.Equal("Batch created successfully", result.Message);
            Assert.Equal("Test Batch 1", result.CompleteBatchNumber);
        }

        [Fact]
        public async Task CreateBatchAsync_DiscountAdjustment_DoesNotUpdateProductId()
        {
            // Arrange  
            var ERPOrderIds = new List<int> { 1, 2, 3 };
            var BatchType = 1;
            var BatchName = "Test Batch";
            var User = "TestUser";
            var assignedDepartments = new List<AssignedDepartment>
            {
                new() { OrderItemId = 1, AssignedDepartmentId = 1 },
                new() { OrderItemId = 2, AssignedDepartmentId = 2 }
            };
            var replacementSkus = new List<ReplacementSku>
            {
                new() { OriginalSku = "SKU001", NewSku = "SKU002", NewPID = 2 }
            };
            var orders = new List<Order>
            {
                new() {
                    orderId = 1,
                    items =
                    [
                        new OrderItem
                        {
                            sku = "SKU001",
                            quantity = 1,
                            unitPrice = 10m,
                            Product = new Product { ProductId = 1, Sku = "SKU001", Departments = [] },
                            ERPProductId = 1
                        },
                        new OrderItem
                        {
                            sku = "Discount",
                            quantity = 1,
                            unitPrice = -5m,
                            name = "Discount",
                            adjustment = true,
                            Product = null,
                            ERPProductId = null
                        },
                        new OrderItem
                        {
                            sku = null,
                            quantity = 1,
                            unitPrice = -5m,
                            name = "no sku item",
                            adjustment = true,
                            Product = null,
                            ERPProductId = null
                        }
                    ]
                }
            };
            _ = _mockOrderBatchRepository.Setup(repo => repo.GetOrdersWithProductsByERPOrderIdsAsync(ERPOrderIds))
                .ReturnsAsync(orders);

            // Act  
            var result = await _orderBatchService.CreateBatchAsync(ERPOrderIds, BatchType, BatchName, assignedDepartments, replacementSkus, User);

            // Assert  
            Assert.True(result.Success);
            Assert.Equal("Batch created successfully", result.Message);
            Assert.Equal("Test Batch 1", result.CompleteBatchNumber);

            var discountItem = orders.SelectMany(o => o.items).FirstOrDefault(i => i.sku == "Discount");
            Assert.NotNull(discountItem);
            Assert.Null(discountItem.ERPProductId); // Making sure the ERPProductId remains null, to ensure that the method isn't updating it.  

            var nullSkuItem = orders.SelectMany(o => o.items).FirstOrDefault(i => string.IsNullOrEmpty(i.sku));
            Assert.NotNull(nullSkuItem);
            Assert.Null(nullSkuItem.ERPProductId); // Same as above.  
        }
        #endregion
        #region GetMissingSkus
        [Fact]
        public async Task GetMissingSkusListAsync_AllSkusExist_ReturnsEmptyList()
        {
            // Arrange  
            var inventoryPickList = new List<InventoryPickList>
            {
                new() { ERPProductId = 1, Sku = "SKU001" },
                new() { ERPProductId = 2, Sku = "SKU002" }
            };

            // Act  
            var result = await _orderBatchService.GetMissingSkusListAsync(inventoryPickList);

            // Assert  
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMissingSkusListAsync_SomeSkusMissing_ReturnsMissingSkus()
        {
            // Arrange  
            var inventoryPickList = new List<InventoryPickList>
            {
                new() { ERPProductId = 1, Sku = "SKU001" },
                new() { ERPProductId = 99, Sku = "MissingSKU" },
                new() { ERPProductId = 4, Sku = "InactiveSKU" }
            };

            // Act  
            var result = await _orderBatchService.GetMissingSkusListAsync(inventoryPickList);
            var resultSkus = result.Select(static m => m.Sku).ToList();

            // Assert  
            Assert.Contains("MissingSKU", resultSkus);
            Assert.Contains("InactiveSKU", resultSkus);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetMissingSkusListAsync_ProductRepositoryThrowsException_ReturnsMissingSkus()
        {
            // Arrange  
            var inventoryPickList = new List<InventoryPickList>
            {
                new() { ERPProductId = 1, Sku = "SKU001" },
                new() { ERPProductId = 99, Sku = "MissingSKU" }
            };

            _ = _mockProductRepository.SetupSequence(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(new Product { ProductId = 1, Sku = "SKU001", IsActive = true })
                .ThrowsAsync(new Exception("Database error"));

            // Act  
            var result = await _orderBatchService.GetMissingSkusListAsync(inventoryPickList);
            var resultSkus = result.Select(static m => m.Sku).ToList();

            // Assert  
            Assert.Contains("MissingSKU", resultSkus);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetMissingSkusListAsync_InvalidSkuIsSet_ReturnsMissingSkus()
        {
            // Arrange  
            var inventoryPickList = new List<InventoryPickList>
            {
                new() { ERPProductId = 99, Sku = "MissingSKU", InvalidSku = null }
            };

            // Act  
            var result = await _orderBatchService.GetMissingSkusListAsync(inventoryPickList);
            var resultSkus = result.Select(static m => m.Sku).ToList();

            // Assert  
            Assert.Contains("MissingSKU", resultSkus);
            Assert.Single(result);
            Assert.Equal("MissingSKU", inventoryPickList.First().InvalidSku);
        }
        #endregion
        #region GetUnassignedDepartments
        [Fact]
        public async Task GetUnassignableDepartments_AllDepartmentsAssignable_ReturnsEmptyList()
        {
            // Arrange  
            var inventoryPickList = new List<InventoryPickList>
            {
                new() { ERPProductId = 1, ERPOrderItemId = 101, Department = 1 },
                new() { ERPProductId = 2, ERPOrderItemId = 102, Department = 2 }
            };

            _ = _mockOrderBatchService.Setup(static service => service.GetDepartmentIdsByProductIdAsync(It.IsAny<int>()))
                .ReturnsAsync([1]);

            // Act  
            var result = await _orderBatchService.GetUnassignableDepartments(inventoryPickList);

            // Assert  
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUnassignableDepartments_NoProductId_ReturnsUnassignedDepartments()
        {
            // Arrange  
            var inventoryPickList = new List<InventoryPickList>
            {
                new() { ERPProductId = -1, ERPOrderItemId = 101, Department = 0 }
            };

            // Act  
            var result = await _orderBatchService.GetUnassignableDepartments(inventoryPickList);

            // Assert  
            Assert.Contains(101, result);
            _ = Assert.Single(result);
        }

        [Fact]
        public async Task GetUnassignableDepartments_MultipleDepartments_ReturnsUnassignedDepartments()
        {
            // Arrange  
            var inventoryPickList = new List<InventoryPickList>
            {
                new() { ERPProductId = 1, ERPOrderItemId = 101, Department = 0 }
            };

            _ = _mockProductRepository.Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(new Product
                {
                    ProductId = 1,
                    Departments =
                    [
                        new() { DepartmentId = 1 },
                        new() { DepartmentId = 2 }
                    ]
                });

            // Act  
            var result = await _orderBatchService.GetUnassignableDepartments(inventoryPickList);

            // Assert  
            Assert.Contains(101, result);
            _ = Assert.Single(result);
        }

        [Fact]
        public async Task GetUnassignableDepartments_ZeroDepartments_ReturnsUnassignedDepartments()
        {
            // Arrange  
            var inventoryPickList = new List<InventoryPickList>
            {
                new() { ERPProductId = 1, ERPOrderItemId = 101, Department = 0 }
            };

            _ = _mockProductRepository.Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(new Product
                {
                    ProductId = 1,
                    Departments = []
                });

            // Act  
            var result = await _orderBatchService.GetUnassignableDepartments(inventoryPickList);

            // Assert  
            Assert.Contains(101, result);
            _ = Assert.Single(result);
        }

        [Fact]
        public async Task GetUnassignableDepartments_SingleDepartmentZero_ReturnsUnassignedDepartments()
        {
            // Arrange  
            var inventoryPickList = new List<InventoryPickList>
            {
                new() { ERPProductId = 1, ERPOrderItemId = 101, Department = 0 }
            };

            _ = _mockProductRepository.Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(new Product
                {
                    ProductId = 1,
                    Departments =
                    [
                        new() { DepartmentId = 0 }
                    ]
                });

            // Act  
            var result = await _orderBatchService.GetUnassignableDepartments(inventoryPickList);

            // Assert  
            Assert.Contains(101, result);
            _ = Assert.Single(result);
        }

        [Fact]
        public async Task GetUnassignableDepartments_SingleValidDepartment_ReturnsEmptyList()
        {
            // Arrange  
            var inventoryPickList = new List<InventoryPickList>
            {
                new() { ERPProductId = 1, ERPOrderItemId = 101, Department = 0 }
            };

            _ = _mockOrderBatchService.Setup(static service => service.GetDepartmentIdsByProductIdAsync(It.IsAny<int>()))
                .ReturnsAsync([1]);

            // Act  
            var result = await _orderBatchService.GetUnassignableDepartments(inventoryPickList);

            // Assert  
            Assert.Empty(result);
        }
        #endregion
        #region CheckDuplicateBatches
        [Fact]
        public async Task CheckDuplicateBatchesByERPOrderIdsAsync_EmptyInput_ThrowsException()
        {
            // Arrange  
            var ERPOrderIds = new List<int>();

            // Act & Assert  
            var exception = await Assert.ThrowsAsync<Exception>(() => _orderBatchService.CheckDuplicateBatchesByERPOrderIdsAsync(ERPOrderIds));
            Assert.Equal("No orders could be found.", exception.Message);
        }

        [Fact]
        public async Task CheckDuplicateBatchesByERPOrderIdsAsync_ValidInput_NoDuplicates_ReturnsEmptyList()
        {
            // Arrange  
            var ERPOrderIds = new List<int> { 4, 5, 6 };
            var orderBatchItems = new List<OrderBatchItem>();

            _ = _mockOrderBatchRepository.Setup(static repo => repo.GetOrderBatchItemsByERPOrderIds(It.IsAny<List<int>>()))
                .ReturnsAsync(orderBatchItems);

            // Act  
            var result = await _orderBatchService.CheckDuplicateBatchesByERPOrderIdsAsync(ERPOrderIds);

            // Assert  
            Assert.Empty(result);
        }
        #endregion
        #region CreatePickListitem
        [Fact]
        public async Task CreatePickListItem_WithReplacementSku_ReturnsCorrectPickList()
        {
            // Arrange  
            string sku = "SKU001";
            string description = "Test Description";
            int amountRequired = 10;
            int ERPProductId = 1;
            int ERPOrderItemId = 101;
            var order = new Order { ERPOrderId = 1, orderNumber = "Order001" };
            var orderItem = new OrderItem { sku = "SKU001", quantity = 10, ERPProductId = 1 };
            var replacement = new ReplacementSku { NewSku = "SKU002" };
            AssignedDepartment? assignedDepartment = null;

            _ = _mockProductRepository.Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(new Product
                {
                    ProductId = 2,
                    Departments =
                    [
                        new() { DepartmentId = 1 }
                    ]
                });

            // Act  
            var result = await _orderBatchService.CreatePickListItem(sku, description, amountRequired, ERPProductId, ERPOrderItemId, order, replacement, assignedDepartment, orderItem);

            // Assert  
            Assert.Equal("SKU002", result.Sku);
            Assert.Equal("SKU001", result.InvalidSku);
            Assert.Equal(1, result.Department);
            Assert.Single(result.OrderQuantities);
        }

        [Fact]
        public async Task CreatePickListItem_WithoutReplacementSku_ReturnsCorrectPickList()
        {
            // Arrange  
            string sku = "SKU001";
            string description = "Test Description";
            int amountRequired = 10;
            int ERPProductId = 1;
            int ERPOrderItemId = 101;
            var order = new Order { ERPOrderId = 1, orderNumber = "Order001" };
            var orderItem = new OrderItem { sku = "SKU001", quantity = 10, ERPProductId = 1 };
            ReplacementSku? replacement = null;
            AssignedDepartment? assignedDepartment = null;

            _ = _mockProductRepository.Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(new Product
                {
                    ProductId = 1,
                    Departments =
                    [
                        new() { DepartmentId = 1 }
                    ]
                });

            // Act  
            var result = await _orderBatchService.CreatePickListItem(sku, description, amountRequired, ERPProductId, ERPOrderItemId, order, replacement, assignedDepartment, orderItem);

            // Assert  
            Assert.Equal("SKU001", result.Sku);
            Assert.Null(result.InvalidSku);
            Assert.Equal(1, result.Department);
            Assert.Single(result.OrderQuantities);
        }

        [Fact]
        public async Task CreatePickListItem_WithAssignedDepartment_ReturnsCorrectPickList()
        {
            // Arrange  
            string sku = "SKU001";
            string description = "Test Description";
            int amountRequired = 10;
            int ERPProductId = 1;
            int ERPOrderItemId = 101;
            var order = new Order { ERPOrderId = 1, orderNumber = "Order001" };
            var orderItem = new OrderItem { sku = "SKU001", quantity = 10, ERPProductId = 1 };
            ReplacementSku? replacement = null;
            var assignedDepartment = new AssignedDepartment { OrderItemId = 101, AssignedDepartmentId = 2 };

            _ = _mockProductRepository.Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync(new Product
                {
                    ProductId = 1,
                    Departments =
                    [
                        new() { DepartmentId = 1 }
                    ]
                });

            // Act  
            var result = await _orderBatchService.CreatePickListItem(sku, description, amountRequired, ERPProductId, ERPOrderItemId, order, replacement, assignedDepartment, orderItem);

            // Assert  
            Assert.Equal(2, result.Department);
            Assert.Single(result.OrderQuantities);
        }
        #endregion
        #region GetFilteredProductsForBatchItems
        [Fact]
        public async Task GetFilteredProductsForBatchItems_SkuContainsDigits_MatchesFilteredProducts()
        {
            // Arrange  
            var batchViewModels = new List<BatchItemViewModel>
            {
                new() { Sku = "SKU123" }
            };

            _ = _mockOrderBatchRepository.Setup(static repo => repo.IsAltItemCheck(It.IsAny<string>())).Returns(true);
            _ = _mockOrderBatchRepository.Setup(static repo => repo.GetFilteredProducts(It.IsAny<string>()))
                .ReturnsAsync(
                [
                    new() { Sku = "SKU123A" },
                    new() { Sku = "SKU123B" }
                ]);

            // Act  
            var result = await _orderBatchService.GetFilteredProductsForBatchItems(batchViewModels);

            // Assert  
            _ = Assert.Single(result);
            Assert.Equal(2, result[0].FilteredProductSkus.Count);
            Assert.Contains("SKU123A", result[0].FilteredProductSkus);
            Assert.Contains("SKU123B", result[0].FilteredProductSkus);
        }

        [Fact]
        public async Task GetFilteredProductsForBatchItems_SkuContainsDigits_NoMatchingFilteredProducts()
        {
            // Arrange  
            var batchViewModels = new List<BatchItemViewModel>
            {
                new() { Sku = "SKU123" }
            };

            _ = _mockOrderBatchRepository.Setup(static repo => repo.IsAltItemCheck(It.IsAny<string>())).Returns(true);
            _ = _mockOrderBatchRepository.Setup(static repo => repo.GetFilteredProducts(It.IsAny<string>()))
                .ReturnsAsync([]);

            // Act  
            var result = await _orderBatchService.GetFilteredProductsForBatchItems(batchViewModels);

            // Assert  
            _ = Assert.Single(result);
            Assert.Empty(result[0].FilteredProductSkus);
        }

        [Fact]
        public async Task GetFilteredProductsForBatchItems_SkuDoesNotContainDigits()
        {
            // Arrange  
            var batchViewModels = new List<BatchItemViewModel>
            {
                new() { Sku = "SKUNODIGITS" }
            };

            _ = _mockOrderBatchRepository.Setup(static repo => repo.IsAltItemCheck(It.IsAny<string>())).Returns(true);

            // Act  
            var result = await _orderBatchService.GetFilteredProductsForBatchItems(batchViewModels);

            // Assert  
            _ = Assert.Single(result);
            Assert.Empty(result[0].FilteredProductSkus);
        }

        [Fact]
        public async Task GetFilteredProductsForBatchItems_SkuIsNotAltItem()
        {
            // Arrange  
            var batchViewModels = new List<BatchItemViewModel>
            {
                new() { Sku = "SKU123" }
            };

            _ = _mockOrderBatchRepository.Setup(static repo => repo.IsAltItemCheck(It.IsAny<string>())).Returns(false);

            // Act  
            var result = await _orderBatchService.GetFilteredProductsForBatchItems(batchViewModels);

            // Assert  
            _ = Assert.Single(result);
            Assert.Empty(result[0].FilteredProductSkus);
        }
        #endregion
        #region GetLocationInfo
        [Fact]
        public async Task GetLocationInfo_ValidInput_ReturnsLocationInfo()
        {
            // Arrange  
            string sku = "SKU001";
            string userId = "TestUser";

            var userSiteMappings = new List<UserSiteMapping>
            {
                new() { UserId = userId, SiteId = 1 },
                new() { UserId = userId, SiteId = 2 }
            };

            var stocks = new List<Stock>
            {
                new() { StockId = 1, LocationId = 1, TotalAvailable = 10 },
                new() { StockId = 2, LocationId = 2, TotalAvailable = 20 }
            };

            var locations = new List<Location>
            {
                new() { LocationId = 1, LocationName = "Location1", SiteId = 1, IsActive = true, Type = LocationType.Normal },
                new() { LocationId = 2, LocationName = "Location2", SiteId = 2, IsActive = true, Type = LocationType.Normal }
            };

            var receiveOnlyLocations = new List<Location>
            {
                new() { LocationId = 3, LocationName = "Location3", SiteId = 1, IsActive = true, Type = LocationType.ReceiveOnly }
            };

            _ = _mockUserSiteMappingService.Setup(static x => x.GetList(
                It.IsAny<Expression<Func<UserSiteMapping, bool>>>(),
                It.IsAny<Expression<Func<UserSiteMapping, string>>[]>(),
                It.IsAny<Expression<Func<UserSiteMapping, object>>[]>()
            )).Returns(userSiteMappings);

            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetStocksBySku(It.IsAny<string>()))
                .ReturnsAsync(stocks);
            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetLocationsByStocks(It.IsAny<List<Stock>>()))
                .ReturnsAsync(locations);
            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetReceiveOnlyLocations())
                .ReturnsAsync(receiveOnlyLocations);

            // Act  
            var result = await _orderBatchService.GetLocationInfo(sku, userId);

            // Assert  
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(2, result[0].LocationId);
            Assert.Equal(1, result[1].LocationId);
            Assert.Equal(3, result[2].LocationId);
            Assert.True(result[0].IsDefault);
        }

        [Fact]
        public async Task GetLocationInfo_NoStocks_ReturnsEmptyList()
        {
            // Arrange  
            string sku = "SKU001";
            string userId = "TestUser";

            var userSiteMappings = new List<UserSiteMapping>
            {
                new() { UserId = userId, SiteId = 1 }
            };

            var stocks = new List<Stock>();
            var locations = new List<Location>();
            var receiveOnlyLocations = new List<Location>();

            _ = _mockUserSiteMappingService.Setup(static x => x.GetList(
                It.IsAny<Expression<Func<UserSiteMapping, bool>>>(),
                It.IsAny<Expression<Func<UserSiteMapping, string>>[]>(),
                It.IsAny<Expression<Func<UserSiteMapping, object>>[]>()
            )).Returns(userSiteMappings);

            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetStocksBySku(It.IsAny<string>()))
                .ReturnsAsync(stocks);
            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetLocationsByStocks(It.IsAny<List<Stock>>()))
                .ReturnsAsync(locations);
            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetReceiveOnlyLocations())
                .ReturnsAsync(receiveOnlyLocations);

            // Act  
            var result = await _orderBatchService.GetLocationInfo(sku, userId);

            // Assert  
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLocationInfo_NoUserSiteMappings_ReturnsEmptyList()
        {
            // Arrange  
            string sku = "SKU001";
            string userId = "TestUser";

            var userSiteMappings = new List<UserSiteMapping>();
            var stocks = new List<Stock>
            {
                new() { StockId = 1, LocationId = 1, TotalAvailable = 10 }
            };
            var locations = new List<Location>
            {
                new() { LocationId = 1, LocationName = "Location1", SiteId = 1, IsActive = true, Type = LocationType.Normal }
            };
            var receiveOnlyLocations = new List<Location>();

            _ = _mockUserSiteMappingService.Setup(static x => x.GetList(
                It.IsAny<Expression<Func<UserSiteMapping, bool>>>(),
                It.IsAny<Expression<Func<UserSiteMapping, string>>[]>(),
                It.IsAny<Expression<Func<UserSiteMapping, object>>[]>()
            )).Returns(userSiteMappings);

            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetStocksBySku(It.IsAny<string>()))
                .ReturnsAsync(stocks);
            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetLocationsByStocks(It.IsAny<List<Stock>>()))
                .ReturnsAsync(locations);
            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetReceiveOnlyLocations())
                .ReturnsAsync(receiveOnlyLocations);

            // Act  
            var result = await _orderBatchService.GetLocationInfo(sku, userId);

            // Assert  
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        #endregion
        #region TransferStock
        [Fact]
        public async Task TransferStock_ShouldReturnTrue_WhenTransferIsSuccessful()
        {
            // Arrange
            var stockTransfers = new List<StockTransfer>
    {
        new() { FromLocationId = 1, ToLocationId = 1, ProductId = 1, Quantity = 1, OrderBatchId = 1, OrderBatchItemId = 1, OrderBatchItemIdList = [1, 2], OrderBatchProductMappingId = 1 }
    };
            string currentUserName = "testUser";

            var orderBatch = new OrderBatch
            {
                OrderBatchId = 1,
                BatchNumber = "123",
                Status = OrderBatchStatus.InProgress,
                Type = BatchType.Inventory,
                IsDeductible = true
            };

            // Mocking the repository methods
            _ = _mockOrderBatchRepository.Setup(static repo => repo.GetOrderBatchItemByOrderBatchItemId(It.IsAny<int>()))
                .ReturnsAsync(new OrderBatchItem { OrderBatchItemId = 1, BatchItemStatus = new BatchItemStatus { DepartmentId = 1, ExecutionSequence = 1 } });

            _ = _mockOrderBatchRepository.Setup(static repo => repo.GetNextBatchItemStatusByDepartmentAndExecutionSequence(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new BatchItemStatus { BatchItemStatusId = 2, StatusName = "Picked" });

            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetStockByLocationIdAndProductId(1, 1))
                .ReturnsAsync(new Stock { StockId = 1, LocationId = 1, ProductId = 1, TotalAvailable = 10 });

            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetStockByLocationIdAndProductId(2, 1))
                .ReturnsAsync(new Stock { StockId = 2, LocationId = 2, ProductId = 1, TotalAvailable = 5 });

            _ = _mockOrderBatchRepository.Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<OrderBatch, bool>>>(), It.IsAny<Expression<Func<OrderBatch, object>>[]>()))
                .ReturnsAsync(orderBatch);

            _ = _mockOrderBatchService.Setup(static service => service.GetAsync(It.IsAny<Expression<Func<OrderBatch, bool>>>(), It.IsAny<Expression<Func<OrderBatch, object>>[]>()))
                .ReturnsAsync(orderBatch);

            _ = _mockStockRepository.Setup(static repo => repo.Update(It.IsAny<Stock>()))
                .Returns(static (Stock stock) => stock);

            _ = _mockUnitOfWork.Setup(static uow => uow.Stocks).Returns(_mockStockRepository.Object);

            // Act
            var result = await _orderBatchService.TransferStock(stockTransfers, currentUserName);

            // Assert
            Assert.True(result.Item1);
            Assert.Equal("", result.Item2);
            Assert.Equal("Picked", result.Item3);

            // Verify that the mocked method was called with the expected parameters
            _mockOrderBatchRepository.Verify(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<OrderBatch, bool>>>(), It.IsAny<Expression<Func<OrderBatch, object>>[]>()), Times.Once);
        }

        [Fact]
        public async Task TransferStock_ShouldFail_WhenInsufficientStock()
        {
            // Arrange
            var stockTransfers = new List<StockTransfer>
    {
        new() { FromLocationId = 1, ToLocationId = 2, ProductId = 1, Quantity = 10, OrderBatchId = 1, OrderBatchItemId = 1, OrderBatchItemIdList = [1, 2], OrderBatchProductMappingId = 1 }
    };
            string currentUserName = "testUser";

            var orderBatch = new OrderBatch
            {
                OrderBatchId = 1,
                BatchNumber = "123",
                Status = OrderBatchStatus.InProgress,
                Type = BatchType.Inventory,
                IsDeductible = true
            };

            _ = _mockOrderBatchRepository.Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<OrderBatch, bool>>>(), It.IsAny<Expression<Func<OrderBatch, object>>[]>()))
                .ReturnsAsync(orderBatch);

            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetStockByLocationIdAndProductId(1, 1))
                .ReturnsAsync(new Stock { StockId = 1, LocationId = 1, ProductId = 1, TotalAvailable = 5 });

            _ = _mockStockRepository.Setup(static repo => repo.Update(It.IsAny<Stock>()))
                .Returns(static (Stock stock) => stock);

            _ = _mockUnitOfWork.Setup(static uow => uow.Stocks).Returns(_mockStockRepository.Object);

            // Act
            var result = await _orderBatchService.TransferStock(stockTransfers, currentUserName);

            // Assert
            Assert.False(result.Item1);
            Assert.Equal("Transfer Error: Insufficient stock in the 'From' location for SKU: ", result.Item2);
            Assert.Null(result.Item3);
        }

        [Fact]
        public async Task TransferStock_ShouldFail_WhenInvalidLocations()
        {
            // Arrange
            var stockTransfers = new List<StockTransfer>
    {
        new() { FromLocationId = 0, ToLocationId = 1, ProductId = 1, Quantity = 1, OrderBatchId = 1, OrderBatchItemId = 1, OrderBatchItemIdList = [1, 2], OrderBatchProductMappingId = 1 }
    };
            string currentUserName = "testUser";

            var orderBatch = new OrderBatch
            {
                OrderBatchId = 1,
                BatchNumber = "123",
                Status = OrderBatchStatus.InProgress,
                Type = BatchType.Inventory,
                IsDeductible = true
            };

            _ = _mockOrderBatchRepository.Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<OrderBatch, bool>>>(), It.IsAny<Expression<Func<OrderBatch, object>>[]>()))
                .ReturnsAsync(orderBatch);

            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetStockByLocationIdAndProductId(1, 1))
                .ReturnsAsync(new Stock { StockId = 1, LocationId = 1, ProductId = 1, TotalAvailable = 10 });

            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetStockByLocationIdAndProductId(2, 1))
                .ReturnsAsync(new Stock { StockId = 2, LocationId = 2, ProductId = 1, TotalAvailable = 5 });

            _ = _mockStockRepository.Setup(static repo => repo.Update(It.IsAny<Stock>()))
                .Returns(static (Stock stock) => stock);

            _ = _mockUnitOfWork.Setup(static uow => uow.Stocks).Returns(_mockStockRepository.Object);

            // Act
            var result = await _orderBatchService.TransferStock(stockTransfers, currentUserName);

            // Assert
            Assert.False(result.Item1);
            Assert.Equal("Transfer Error: Invalid From or To location.", result.Item2);
            Assert.Null(result.Item3);
        }

        [Fact]
        public async Task TransferStock_ShouldTransferCorrectQuantity()
        {
            // Arrange
            var stockTransfers = new List<StockTransfer>
            {
                new() { FromLocationId = 1, ToLocationId = 2, ProductId = 1, Quantity = 5, OrderBatchId = 1, OrderBatchItemId = 1, OrderBatchItemIdList = [1, 2], OrderBatchProductMappingId = 1 }
            };
            string currentUserName = "testUser";

            var orderBatch = new OrderBatch
            {
                OrderBatchId = 1,
                BatchNumber = "123",
                Status = OrderBatchStatus.InProgress,
                Type = BatchType.Inventory,
                IsDeductible = true
            };

            _ = _mockOrderBatchRepository.Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<OrderBatch, bool>>>(), It.IsAny<Expression<Func<OrderBatch, object>>[]>()))
                .ReturnsAsync(orderBatch);

            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetStockByLocationIdAndProductId(1, 1))
                .ReturnsAsync(new Stock { StockId = 1, LocationId = 1, ProductId = 1, TotalAvailable = 10 });

            _ = _mockUnitOfWork.Setup(static uow => uow.OrderBatch.GetStockByLocationIdAndProductId(2, 1))
                .ReturnsAsync(new Stock { StockId = 2, LocationId = 2, ProductId = 1, TotalAvailable = 5 });

            _ = _mockStockRepository.Setup(static repo => repo.Update(It.IsAny<Stock>()))
                .Returns(static (Stock stock) => stock);

            _ = _mockUnitOfWork.Setup(static uow => uow.Stocks).Returns(_mockStockRepository.Object);

            // Act
            var result = await _orderBatchService.TransferStock(stockTransfers, currentUserName);

            // Assert
            Assert.True(result.Item1);
            Assert.Equal("", result.Item2);
            Assert.Null(result.Item3);

            _mockStockRepository.Verify(static repo => repo.Update(It.Is<Stock>(static s => s.StockId == 1 && s.TotalAvailable == 5)), Times.Once);
            _mockStockRepository.Verify(static repo => repo.Update(It.Is<Stock>(static s => s.StockId == 2 && s.TotalAvailable == 10)), Times.Once);
        }

        [Fact]
        public async Task TransferStock_ShouldFail_WhenOrderBatchIsNull()
        {
            // Arrange
            var stockTransfers = new List<StockTransfer>
    {
        new() { FromLocationId = 1, ToLocationId = 1, ProductId = 1, Quantity = 1, OrderBatchId = 1, OrderBatchItemId = 1, OrderBatchItemIdList = [1, 2], OrderBatchProductMappingId = 1 }
    };
            string currentUserName = "testUser";

            _ = _mockOrderBatchRepository.Setup(static repo => repo.FilterOneAsync(It.IsAny<Expression<Func<OrderBatch, bool>>>(), It.IsAny<Expression<Func<OrderBatch, object>>[]>()))
                .ReturnsAsync((OrderBatch)null);

            // Act
            var result = await _orderBatchService.TransferStock(stockTransfers, currentUserName);

            // Assert
            Assert.False(result.Item1);
            Assert.Equal("Transfer Error: Insufficient stock in the 'From' location for SKU: ", result.Item2);
            Assert.Null(result.Item3);
        }
        #endregion

        [Fact]
        public async Task RemoveOrders_ShouldRemoveOrderBatchItems_WhenItemsExist()
        {
            // Arrange
            int cwaOrderId = 1;
            int orderBatchId = 1;

            var pickedItem = new OrderBatchItem
            {
                ERPOrderId = cwaOrderId,
                IsPicked = true,
                ProductId = 1,
                Quantity = 10
            };

            var unpickedItem = new OrderBatchItem
            {
                ERPOrderId = cwaOrderId,
                IsPicked = false,
                ProductId = 2,
                Quantity = 5
            };

            var orderBatchItems = new List<OrderBatchItem> { pickedItem, unpickedItem };

            // Mock GetOrderBatchItemsByOrderBatchId to return the test items
            _mockOrderBatchRepository.Setup(repo => repo.GetOrderBatchItemsByOrderBatchId(orderBatchId))
                .ReturnsAsync(orderBatchItems);

            // Mock ExecuteTransactionAsync
            _mockOrderBatchRepository.Setup(repo => repo.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(async action =>
                {
                    await action();
                    return true;
                });

            // Mock RemoveRangeAsync
            _mockUnitOfWork.Setup(uow => uow.OrderBatchItem).Returns(_mockOrderBatchItemRepository.Object);


            // Mock SaveChangesAsync
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
                .ReturnsAsync(1);

            // Mock GetByIdAsync to return a Product for the picked item's ProductId
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(new Product
                {
                    ProductId = 1,
                    Sku = "SKU001"
                });

            // Mock MoveStockHistories.GetListByFilterAsync to return a MoveStockHistory entry
            _mockMoveStockHistoryRepository
                .Setup(repo => repo.GetListByFilterAsync(It.IsAny<Expression<Func<MoveStockHistory, bool>>>(), null, null))
                .ReturnsAsync(new List<MoveStockHistory>
                {
        new MoveStockHistory
        {
            Sku = "SKU001",
            Quantity = 10,
            Type = ActionType.Transfer,
            DateTime = DateTime.UtcNow,
            FromStockId = 1,
            ToStockId = 2
        }
                });


            // Mock Stocks.GetByIdAsync to return stock objects
            _mockStockRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(new Stock
                {
                    ProductId = 1,
                    TotalAvailable = 50
                });

            _mockStockRepository.Setup(repo => repo.GetByIdAsync(2))
                .ReturnsAsync(new Stock
                {
                    ProductId = 1,
                    TotalAvailable = 30
                });

            // Act
            var result = await _orderBatchService.RemoveOrders(cwaOrderId, orderBatchId);

            // Assert
            Assert.True(result);

            // Verify that UndoTransfer was called for the picked item
            _mockStockRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
            _mockStockRepository.Verify(repo => repo.GetByIdAsync(2), Times.Once);
        }

        [Fact]
        public async Task RemoveOrders_ShouldNotRemove_WhenNoMatchingItems()
        {
            // Arrange  
            int cwaOrderId = 1;
            int orderBatchId = 1;

            var orderBatchItems = new List<OrderBatchItem>
        {
            new OrderBatchItem { ERPOrderId = 2, IsPicked = false }
        };

            _mockOrderBatchRepository.Setup(repo => repo.GetOrderBatchItemsByOrderBatchId(orderBatchId))
                .ReturnsAsync(orderBatchItems);

            _mockOrderBatchRepository.Setup(repo => repo.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(async action => { await action(); return true; });

            // Act  
            var result = await _orderBatchService.RemoveOrders(cwaOrderId, orderBatchId);

            // Assert  
            Assert.False(result);
            _mockOrderBatchItemRepository.Verify(repo => repo.RemoveRangeAsync(It.IsAny<List<OrderBatchItem>>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
        }
    }
}