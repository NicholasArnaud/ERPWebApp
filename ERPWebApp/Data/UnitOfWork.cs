using ERPWebApp.Data.Repositories;
using ERPWebApp.Data.Repositories.Azure;
using ERPWebApp.Data.Repositories.Azure.Interface;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Config;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace ERPWebApp.Data
{
    public class UnitOfWork(
        ApplicationDbContext context,
        IOptions<AzureStorageConfig> config,
        ILoggerFactory loggerFactory
    ) : IUnitOfWork
    {
        #region dependencies

        private readonly IOptions<AzureStorageConfig> _config = config;
        #endregion

        private IOrderRepository _Orders;
        private IOrderTagRepository _OrderTags;
        private IShipStationStoreRepository _shipStationStores;
        private IOrderShipmentRepository _orderShipments;
        private IOrderFulfillmentRepository _orderFulfillments;
        private IProductRepository _products;
        private IStockRepository _stocks;
        private ISiteRepository _sites;
        private IDepartmentRepository _departments;
        private IEmployeeRepository _employees;
        private ISubCategoryRepository _subCategories;
        private ISpeedOMeterGoalRepository _speedOMeterGoals;
        private IProductionVsLaborCostPriceRepository _productionVsLaborCostPrices;
        private IMoveStockHistoryRepository _moveStockHistories;
        private ILocationRepository _locations;
        private IProductFilesMappingsRepository _productFilesMappings;
        private IFilesRepository _files;
        private IProductVendorMappingRepository _productVendorMappings;
        private ISkuCategoryRepository _skuCategories;
        private ISkuColorRepository _skuColors;
        private ISkuUnitOfMeasureRepository _skuUnitOfMeasures;
        private IProductPurchaseOrderRepository _productPurchaseOrders;
        private IPurchaseOrderRepository _purchaseOrders;
        private IPurchaseOrderFilesMappingRepository _purchaseOrderFilesMappings;
        private IShippingMethodRepository _shippingMethods;
        private IShippingProviderRepository _shippingProviders;
        private IVendorRepository _vendors;
        private IProductPurchaseOrderStockMappingRepository _productPurchaseOrderStockMappings;
        private INirfProductMappingRepository _nirfProductMappings;
        private IUserRepository _users;
        private IFontRepository _fonts;
        private INirfForecastingRepository _nirfForecastings;
        private INirfFormRepository _nirfForms;
        private INirfImageMappingRepository _nirfImageMappings;
        private INirfInventoryRepository _nirfInventories;
        private INirfPackagingRepository _nirfPackagings;
        private INirfParametersRepository _nirfParameters;
        private INirfShippingRepository _nirfShipping;
        private INirfVendorMappingRepository _nirfVendorMappings;
        private IProductContainerRepository _productContainers;
        private IInventoryRequestFormRepository _inventoryRequestForms;
        private ICycleCountFrequencyRepository _cycleCountFrequences;
        private ICycleCountRepository _cycleCountes;
        private IFinancialsRepository _financials;
        private IInventoryRepository _inventory;
        private IProductImageRepository _productImages;
        private IOrderItemRepository _orderItems;
        private IProductTagRepository _productTags;
        private IBarcodeScanRepository _barcodeScans;
        private IShipStationAwaitingOrderRepository _shipstationAwaitingOrders;
        private IShipStationOrderedHistoryRepository _shipStationOrderedHistories;
        private ISellerMarginRepository _sellerMargins;
        private IShippingScanoutRepository _shippingScanouts;
        private IOrderBatchRepository _orderBatch;
        private IUserPreferencesRepository _userPreferences;
        private IInventoryBalanceRepository _inventoryBalance;
        private IBatchViewRepository _batchView;
        private IOrderAdvancedOptionsRepository _orderAdvancedOptions;
        private IAuditLogRepository _auditLog;
        private IEmailAlertsRepository _emailAlerts;
        private IUserEmailAlertMappingRepository _userEmailAlertMapping;
        private IOrderSourceRepository _orderSource;
        private IOrderBatchItemRepository _orderBatchItem;
        private IBatchItemStatusRepository _batchItemStatus;
        private IBundleRepository _bundles;
        private IBundleItemRepository _bundleItems;
        private IIntegrationRepository _integration;
        private IUserSiteMappingRepository _userSiteMapping;
        private IInvoicedOrdersRepository _invoicedOrders;
        private IDHLInvoiceRepository _dhlInvoices;
        private IUPSInvoiceRepository _upsInvoices;
        private IStampsUSPSInvoiceRepository _stampsUSPSInvoices;
        private IEasyPostInvoiceRepository _easyPostInvoices;
        private ISkulabsImportRepository _skulabsImports;
        private IMiscProductRepository _miscProducts;
        private IShippingManifestRepository _shippingManifests;

        #region Azure Repositories
        private IAzureBlobStorageRepository _azureBlobStorage;

        public IAzureBlobStorageRepository AzureBlobStorage => _azureBlobStorage ??= new AzureBlobStorageRepository(_config);
        #endregion

        #region Repositories
        public IOrderRepository Orders => _Orders ??= new OrderRepository(context);
        public IOrderTagRepository OrderTags => _OrderTags ??= new OrderTagRepository(context);
        public IShipStationStoreRepository ShipStationStores => _shipStationStores ??= new ShipStationStoreRepository(context);
        public IOrderShipmentRepository OrderShipments => _orderShipments ??= new OrderShipmentRepository(context);
        public IOrderFulfillmentRepository OrderFulfillments => _orderFulfillments ??= new OrderFulfillmentRepository(context);
        public IProductRepository Products => _products ??= new ProductRepository(context);
        public IStockRepository Stocks => _stocks ??= new StockRepository(context);
        public ISiteRepository Sites => _sites ??= new SiteRepository(context);
        public IDepartmentRepository Departments => _departments ??= new DepartmentRepository(context);
        public ISubCategoryRepository SubCategories => _subCategories ??= new SubCategoryRepository(context);
        public ISpeedOMeterGoalRepository SpeedOMeterGoals => _speedOMeterGoals ??= new SpeedOMeterGoalRepository(context);
        public IEmployeeRepository Employees => _employees ??= new EmployeeRepository(context);
        public IProductionVsLaborCostPriceRepository ProductionVsLaborCostPrices => _productionVsLaborCostPrices ??= new ProductionVsLaborCostPriceRepository(context);
        public IMoveStockHistoryRepository MoveStockHistories => _moveStockHistories ??= new MoveStockHistoryRepository(context, loggerFactory.CreateLogger<MoveStockHistoryRepository>());
        public ILocationRepository Locations => _locations ??= new LocationRepository(context);
        public IProductFilesMappingsRepository ProductFilesMappings => _productFilesMappings ??= new ProductFilesMappingsRepository(context);
        public IFilesRepository Files => _files ??= new FilesRepository(context);
        public IProductVendorMappingRepository ProductVendorMappings => _productVendorMappings ??= new ProductVendorMappingRepository(context);
        public ISkuCategoryRepository SkuCategories => _skuCategories ??= new SkuCategoryRepository(context);
        public ISkuColorRepository SkuColors => _skuColors ??= new SkuColorRepository(context);
        public ISkuUnitOfMeasureRepository SkuUnitOfMeasure => _skuUnitOfMeasures ??= new SkuUnitOfMeasureRepository(context);
        public IProductPurchaseOrderRepository ProductPurchaseOrders => _productPurchaseOrders ??= new ProductPurchaseOrderRepository(context);
        public IPurchaseOrderRepository PurchaseOrders => _purchaseOrders ??= new PurchaseOrderRepository(context);
        public IPurchaseOrderFilesMappingRepository PurchaseOrderFilesMappings => _purchaseOrderFilesMappings ??= new PurchaseOrderFilesMappingRepository(context);
        public IShippingMethodRepository ShippingMethods => _shippingMethods ??= new ShippingMethodRepository(context);
        public IShippingProviderRepository ShippingProviders => _shippingProviders ??= new ShippingProviderRepository(context);
        public IVendorRepository Vendors => _vendors ??= new VendorRepository(context);
        public IProductPurchaseOrderStockMappingRepository ProductPurchaseOrderStockMapping => _productPurchaseOrderStockMappings ??= new ProductPurchaseOrderStockMappingRepository(context);
        public INirfProductMappingRepository NirfProductMapping => _nirfProductMappings ??= new NirfProductMappingRepository(context);
        public IUserRepository Users => _users ??= new UserRepository(context);
        public IFontRepository Fonts => _fonts ??= new FontRepository(context);
        public INirfForecastingRepository NirfForecasting => _nirfForecastings ??= new NirfForecastingRepository(context);
        public INirfFormRepository NirfForms => _nirfForms ??= new NirfFormRepository(context);
        public INirfImageMappingRepository NirfImageMapping => _nirfImageMappings ??= new NirfImageMappingRepository(context);
        public INirfInventoryRepository NirfInventories => _nirfInventories ??= new NirfInventoryRepository(context);
        public INirfPackagingRepository NirfPackaging => _nirfPackagings ??= new NirfPackagingRepository(context);
        public INirfParametersRepository NirfParameters => _nirfParameters ??= new NirfParametersRepository(context);
        public INirfShippingRepository NirfShipping => _nirfShipping ??= new NirfShippingRepository(context);
        public INirfVendorMappingRepository NirfVendorMapping => _nirfVendorMappings ??= new NirfVendorMappingRepository(context);
        public IProductContainerRepository ProductContainers => _productContainers ??= new ProductContainerRepository(context);
        public IInventoryRequestFormRepository InventoryRequestForms => _inventoryRequestForms ??= new InventoryRequestFormRepository(context);
        public ICycleCountFrequencyRepository CycleCountFrequencies => _cycleCountFrequences ??= new CycleCountFrequencyRepository(context);
        public ICycleCountRepository CycleCountes => _cycleCountes ??= new CycleCountRepository(context);
        public IFinancialsRepository Financials => _financials ??= new FinancialsRepository(context);
        public IInventoryRepository Inventory => _inventory ??= new InventoryRepository(context);
        public IProductImageRepository ProductImages => _productImages ??= new ProductImageRepository(context);
        public IOrderItemRepository OrderItems => _orderItems ??= new OrderItemRepository(context);
        public IBarcodeScanRepository BarcodeScans => _barcodeScans ??= new BarcodeScanRepository(context);
        public IProductTagRepository ProductTags => _productTags ??= new ProductTagRepository(context);
        public IShipStationAwaitingOrderRepository ShipStationAwaitingOrders => _shipstationAwaitingOrders ??= new ShipStationAwaitingOrderRepository(context);
        public IShipStationOrderedHistoryRepository ShipStationOrderedHistories => _shipStationOrderedHistories ??= new ShipStationOrderedHistoryRepository(context);
        public ISellerMarginRepository SellerMargins => _sellerMargins ??= new SellerMarginRepository(context);
        public IShippingScanoutRepository ShippingScanouts => _shippingScanouts ??= new ShippingScanoutRepository(context);
        public IOrderBatchRepository OrderBatch => _orderBatch ??= new OrderBatchRepository(context);
        public IUserPreferencesRepository UserPreferences => _userPreferences ??= new UserPreferencesRepository(context);
        public IInventoryBalanceRepository InventoryBalance => _inventoryBalance ??= new InventoryBalanceRepository(context);
        public IBatchViewRepository BatchView => _batchView ??= new BatchViewRepository(context);
        public IOrderAdvancedOptionsRepository OrderAdvancedOptions => _orderAdvancedOptions ??= new OrderAdvancedOptionsRepository(context);
        public IAuditLogRepository AuditLogs => _auditLog ??= new AuditLogRepository(context);
        public IEmailAlertsRepository EmailAlerts => _emailAlerts ??= new EmailAlertsRepository(context);
        public IUserEmailAlertMappingRepository UserEmailAlertMapping => _userEmailAlertMapping ??= new UserEmailAlertMappingRepository(context);
        public IOrderBatchItemRepository OrderBatchItem => _orderBatchItem ??= new OrderBatchItemRepository(context);
        public IOrderSourceRepository OrderSource => _orderSource ??= new OrderSourceRepository(context);
        public IBatchItemStatusRepository BatchItemStatus => _batchItemStatus ?? new BatchItemStatusRepository(context);
        public IBundleRepository Bundles => _bundles ?? new BundleRepository(context);
        public IBundleItemRepository BundleItems => _bundleItems ?? new BundleItemRepository(context);
        public IIntegrationRepository Integrations => _integration ?? new IntegrationRepository(context);
        public IUserSiteMappingRepository UserSiteMapping => _userSiteMapping ??= new UserSiteMappingRepository(context);
        public IInvoicedOrdersRepository InvoicedOrders => _invoicedOrders ??= new InvoicedOrdersRepository(context);
        public IDHLInvoiceRepository DHLInvoices => _dhlInvoices ??= new DHLInvoiceRepository(context);
        public IUPSInvoiceRepository UPSInvoices => _upsInvoices ??= new UPSInvoiceRepository(context);
        public IStampsUSPSInvoiceRepository StampsUSPSInvoices => _stampsUSPSInvoices ??= new StampsUSPSInvoiceRepository(context);
        public IEasyPostInvoiceRepository EasyPostInvoices => _easyPostInvoices ??= new EasyPostInvoiceRepository(context);
        public ISkulabsImportRepository SkulabsImports => _skulabsImports ??= new SkulabsImportRepository(context);
        public IMiscProductRepository MiscProducts => _miscProducts ??= new MiscProductRepository(context);
        public IShippingManifestRepository ShippingManifest => _shippingManifests ??= new ShippingManifestRepository(context);
        #endregion


        public int SaveChanges()
        {
            return context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public ApplicationDbContext GetApplicationDbContext()
        {
            return context;
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            return new Repository<T>(context);
        }

        public EntityEntry<T> Attach<T>(T entity) where T: class
        {
            return context.Attach(entity);
        }

        #region Transaction
        private IDbContextTransaction _transaction;

        public void BeginTransaction()
        {
            _transaction = context.Database.BeginTransaction();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await context.Database.BeginTransactionAsync();
        }

        public void Commit()
        {
            context.SaveChanges();
            _transaction.Commit();
        }

        public async Task CommitAsync()
        {
            await context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
        }
        #endregion
    }
}
