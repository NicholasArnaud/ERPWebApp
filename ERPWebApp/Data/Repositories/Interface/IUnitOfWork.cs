using ERPWebApp.Data.Repositories.Azure.Interface;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }
        IOrderTagRepository OrderTags { get; }
        IShipStationStoreRepository ShipStationStores { get; }
        IOrderShipmentRepository OrderShipments { get; }
        IOrderFulfillmentRepository OrderFulfillments { get; }
        IProductRepository Products { get; }
        ISubCategoryRepository SubCategories { get; }
        ISpeedOMeterGoalRepository SpeedOMeterGoals { get; }
        IEmployeeRepository Employees { get; }
        IProductionVsLaborCostPriceRepository ProductionVsLaborCostPrices { get; }
        IStockRepository Stocks { get; }
        ISiteRepository Sites { get; }
        IDepartmentRepository Departments { get; }
        IMoveStockHistoryRepository MoveStockHistories { get; }
        ILocationRepository Locations { get; }
        IProductFilesMappingsRepository ProductFilesMappings { get; }
        IFilesRepository Files { get; }
        IProductVendorMappingRepository ProductVendorMappings { get; }
        ISkuCategoryRepository SkuCategories { get; }
        ISkuColorRepository SkuColors { get; }
        ISkuUnitOfMeasureRepository SkuUnitOfMeasure { get; }
        IProductPurchaseOrderRepository ProductPurchaseOrders { get; }
        IPurchaseOrderRepository PurchaseOrders { get; }
        IPurchaseOrderFilesMappingRepository PurchaseOrderFilesMappings { get; }
        IFinancialsRepository Financials { get; }
        IInventoryRepository Inventory { get; }
        IShippingMethodRepository ShippingMethods { get; }
        IShippingProviderRepository ShippingProviders { get; }
        IVendorRepository Vendors { get; }
        IProductPurchaseOrderStockMappingRepository ProductPurchaseOrderStockMapping { get; }
        INirfProductMappingRepository NirfProductMapping { get; }
        IUserRepository Users { get; }
        IFontRepository Fonts { get; }
        INirfForecastingRepository NirfForecasting { get; }
        INirfFormRepository NirfForms { get; }
        INirfImageMappingRepository NirfImageMapping { get; }
        INirfInventoryRepository NirfInventories { get; }
        INirfPackagingRepository NirfPackaging { get; }
        INirfParametersRepository NirfParameters { get; }
        INirfShippingRepository NirfShipping { get; }
        INirfVendorMappingRepository NirfVendorMapping { get; }
        IProductContainerRepository ProductContainers { get; }
        IInventoryRequestFormRepository InventoryRequestForms { get; }
        ICycleCountFrequencyRepository CycleCountFrequencies { get; }
        ICycleCountRepository CycleCountes { get; }
        IProductImageRepository ProductImages { get; }
        IOrderItemRepository OrderItems { get; }
        IShipStationAwaitingOrderRepository ShipStationAwaitingOrders { get; }
        IProductTagRepository ProductTags { get; }
        IBarcodeScanRepository BarcodeScans { get; }
        IShipStationOrderedHistoryRepository ShipStationOrderedHistories { get; }
        ISellerMarginRepository SellerMargins {get;}
        IShippingScanoutRepository ShippingScanouts { get; }
        IOrderBatchRepository OrderBatch { get; }
        IUserPreferencesRepository UserPreferences { get; }
        IInventoryBalanceRepository InventoryBalance { get; }
        IBatchViewRepository BatchView { get; }
        IAuditLogRepository AuditLogs { get; }
        IEmailAlertsRepository EmailAlerts { get; }
        IUserEmailAlertMappingRepository UserEmailAlertMapping { get; }

        IOrderAdvancedOptionsRepository OrderAdvancedOptions { get; }
        IOrderSourceRepository OrderSource { get; }
        IOrderBatchItemRepository OrderBatchItem { get; }
        IBatchItemStatusRepository BatchItemStatus { get; }
        IBundleRepository Bundles { get; }
        IBundleItemRepository BundleItems { get; }
        IIntegrationRepository Integrations { get; }
        IInvoicedOrdersRepository InvoicedOrders { get; }
        IDHLInvoiceRepository DHLInvoices { get; }
        IUPSInvoiceRepository UPSInvoices { get; }
        IStampsUSPSInvoiceRepository StampsUSPSInvoices { get; }
        IEasyPostInvoiceRepository EasyPostInvoices { get; }
        ISkulabsImportRepository SkulabsImports { get; }
        IMiscProductRepository MiscProducts { get; }
        IShippingManifestRepository ShippingManifest { get; }

        #region Azure
        IAzureBlobStorageRepository AzureBlobStorage { get; }
        #endregion

        int SaveChanges();
        Task<int> SaveChangesAsync();
        ApplicationDbContext GetApplicationDbContext();
        IRepository<T> GetRepository<T>() where T : class;
        EntityEntry<T> Attach<T>(T entity) where T : class;
        void BeginTransaction();
        Task BeginTransactionAsync();
        void Commit();
        Task CommitAsync();
        void Rollback();
        Task RollbackAsync();
    }
}
