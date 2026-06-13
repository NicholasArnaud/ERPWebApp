using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using ERPWebApp.Models.Company.Security;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Inventory.SkuProperties;
using ERPWebApp.Models.Invoices;
using ERPWebApp.Models.Mappings;
using ERPWebApp.Models.NirfForms;
using ERPWebApp.Models.Orders;
using ERPWebApp.Models.PurchaseOrders;
using ERPWebApp.Models.Sellers;
using ERPWebApp.Models.Shipping;
using ERPWebApp.Providers.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Data;


public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUserProvider userProvider) : IdentityDbContext(options)
{
    public DbContextOptions<ApplicationDbContext> Options { get { return options; } }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region OrderJSON
        modelBuilder.Entity<Order>()
            .HasMany(e => e.Tags)
            .WithMany(e => e.Orders)
            .UsingEntity(
            "OrderTagMapping",
            l => l.HasOne(typeof(OrderTag)).WithMany().HasForeignKey("OrderTagId").HasPrincipalKey(nameof(OrderTag.OrderTagId)),
            r => r.HasOne(typeof(Order)).WithMany().HasForeignKey("ERPOrderId").HasPrincipalKey(nameof(Order.ERPOrderId)),
            j => j.HasKey("OrderTagId", "ERPOrderId")
            );
        modelBuilder.Entity<Order>()
            .HasMany(e => e.Sources)
            .WithMany(e => e.Orders)
            .UsingEntity(
            "OrderSourceMapping",
            l => l.HasOne(typeof(OrderSource)).WithMany().HasForeignKey("OrderSourceId").HasPrincipalKey(nameof(Order.OrderSource.OrderSourceId)),
            r => r.HasOne(typeof(Order)).WithMany().HasForeignKey("ERPOrderId").HasPrincipalKey(nameof(Order.ERPOrderId)),
            j => j.HasKey("OrderSourceId", "ERPOrderId")
            );
        modelBuilder.Entity<Order>().OwnsOne(o => o.internationalOptions, options =>
        {
            options.ToJson();
            options.OwnsMany(iO => iO.customsItems);
        });
        modelBuilder.Entity<Order>().OwnsOne(o => o.insuranceOptions, options =>
        {
            options.ToJson();
        });
        modelBuilder.Entity<Order>().OwnsOne(o => o.weight, options =>
        {
            options.ToJson();
        });
        modelBuilder.Entity<Order>().OwnsOne(o => o.dimensions, options =>
        {
            options.ToJson();
        });

        #endregion

        #region OrderItemJSON
        modelBuilder.Entity<OrderItem>().OwnsMany(i => i.options, options =>
        {
            options.ToJson();
        });
        modelBuilder.Entity<OrderItem>().OwnsOne(o => o.weight, options =>
        {
            options.ToJson();
        });
        #endregion

        #region OrderShipmentJSON
        modelBuilder.Entity<OrderShipment>().OwnsOne(o => o.weight, options =>
        {
            options.ToJson();
        });
        modelBuilder.Entity<OrderShipment>().OwnsOne(o => o.dimensions, options =>
        {
            options.ToJson();
        });
        #endregion

        #region product

        modelBuilder.Entity<ProductTagsRegistry>()
            .HasIndex(p => p.Description)
            .IsUnique();

        var productTag = modelBuilder.Entity<ProductTag>();
        productTag.HasKey(t => new { t.ProductId, t.TagId });
        productTag.HasOne(x => x.Product)
            .WithMany(x => x.ProductTags)
            .HasForeignKey(x => x.ProductId)
            .IsRequired();
        productTag.HasOne(x => x.Tag)
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .IsRequired();
        #endregion

        #region ShipStation Store
        //The names are hardcoded because they have already been manually created.
        modelBuilder.Entity<ShipStationStore>()
            .HasIndex(x => x.StoreId)
            .IsUnique()
            .HasDatabaseName("UQ_ShipStationStore_StoreId");

        modelBuilder.Entity<ShipStationStore>()
            .HasIndex(x => x.StoreName)
            .IsUnique()
            .HasDatabaseName("UQ_ShipStationStore_StoreName");
        #endregion

        #region stock
        modelBuilder.Entity<Stock>()
        .ToTable("Stock", c => c.IsTemporal());
        #endregion

        #region BatchRelated
        modelBuilder
            .Entity<OrderBatchItem>()
            .ToTable("OrderBatchItem", b => b.IsTemporal())
            .HasOne(obi => obi.BatchItemStatus)
            .WithMany()
            .HasForeignKey(obi => obi.BatchItemStatusId)
            .OnDelete(DeleteBehavior.NoAction);
        #endregion

        #region SubCategory
        modelBuilder.Entity<SubCategory>()
            .HasIndex(s => s.Description)
            .IsUnique()
            .HasDatabaseName("UQ_SubCategory_Description");
        #endregion
    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
    }
    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //=> optionsBuilder.LogTo(Console.WriteLine);
    /// <summary>
    /// Uncomment this if database connection keep giving timeout exceptions. Remember to change the query string.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ApplicationDbContext>();

        int commandTimeout = (int)TimeSpan.FromMinutes(10).TotalSeconds; // Adjust the time as needed
        optionsBuilder.UseSqlServer(
            options => options.CommandTimeout(commandTimeout)
        );
        optionsBuilder.UseLoggerFactory(loggerFactory);
        optionsBuilder.EnableSensitiveDataLogging();
    }

    public class BlankTriggerAddingConvention : IModelFinalizingConvention
    {
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var table = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);
                if (table != null
                    && entityType.GetDeclaredTriggers().All(t => t.GetDatabaseName(table.Value) == null))
                {
                    entityType.Builder.HasTrigger(table.Value.Name + "_Trigger");
                }

                foreach (var fragment in entityType.GetMappingFragments(StoreObjectType.Table))
                {
                    if (entityType.GetDeclaredTriggers().All(t => t.GetDatabaseName(fragment.StoreObject) == null))
                    {
                        entityType.Builder.HasTrigger(fragment.StoreObject.Name + "_Trigger");
                    }
                }
            }
        }
    }

    public override int SaveChanges()
    {
        if (!ChangeTracker.Entries().Any())
            return 0;
        var auditLogs = PrepareAuditLogs();
        if (auditLogs != null && auditLogs.Count > 0)
        {
            AddRange(auditLogs);
        }
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (!ChangeTracker.Entries().Any())
            return 0;
        var auditLogs = PrepareAuditLogs();
        if (auditLogs != null && auditLogs.Count > 0)
        {
            await AddRangeAsync(auditLogs, cancellationToken);
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    private List<AuditLog> PrepareAuditLogs()
    {
        var userId = userProvider.GetCurrentUserId();
        if (userId == Guid.Empty) return [];

        var user = this.Users.FirstOrDefault(x => x.Id == userId.ToString());
        var auditLogs = new List<AuditLog>();
        var ignoreProperties = new List<string>() { "ModifyByUser", "ModifyDate" };

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified)
            {
                var entityName = entry.Entity.GetType().Name;

                if (entityName != "AuditLog")
                {
                    foreach (var property in entry.OriginalValues.Properties)
                    {
                        if (ignoreProperties.Any(x => x.Equals(property.Name, StringComparison.OrdinalIgnoreCase))) continue;

                        var original = entry.OriginalValues[property];
                        var current = entry.CurrentValues[property];

                        if (!object.Equals(original, current))
                        {
                            auditLogs.Add(new AuditLog
                            {
                                Id = Guid.NewGuid(),
                                Timestamp = DateTime.Now,
                                UserId = userId.ToString(),
                                BusinessEntity = entityName,
                                PropertyName = property.Name,
                                OldValue = original?.ToString(),
                                NewValue = current?.ToString(),
                                UserName = user.UserName,
                                Email = user.Email
                            });
                        }
                    }
                }
            }
        }

        return auditLogs;
    }
    //NOTE: The property name set for all DbSet properties defines the database table name.
    //Any property name changes will require a new migration to update the database table.
    public DbSet<Site> Site { get; set; }
    public DbSet<Location> Location { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<ProductContainer> ProductContainer { get; set; }
    public DbSet<Stock> Stock { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderTag> OrderTags { get; set; }
    public DbSet<OrderItem> OrderItem { get; set; }
    public DbSet<OrderShipment> OrderShipments { get; set; }
    public DbSet<OrderFulfillment> OrderFulfillments { get; set; }
    public DbSet<InventoryBalance> InventoryBalance { get; set; }
    public DbSet<SpeedOMeterGoal> SpeedOMeterGoal { get; set; }
    public DbSet<ShipStationOrderedHistory> ShipStationOrderedHistory { get; set; }
    public DbSet<ShipStationAwaitingOrder> ShipStationAwaitingOrder { get; set; }
    public DbSet<BarcodeScan> BarcodeScan { get; set; }
    public DbSet<RedoOrder> RedoOrder { get; set; }
    public DbSet<DeputyTimeSheet> DeputyTimeSheet { get; set; }
    public DbSet<SkuCategory> SkuCategory { get; set; }
    public DbSet<SkuColor> SkuColor { get; set; }
    public DbSet<SkuUnitOfMeasure> SkuUnitOfMeasure { get; set; }
    public DbSet<SalesReport> SalesReport { get; set; }
    public DbSet<ShipStationStore> ShipStationStore { get; set; }
    public DbSet<Vendor> Vendor { get; set; }
    public DbSet<ProductPurchaseOrder> ProductPurchaseOrder { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrder { get; set; }
    public DbSet<ShippingMethod> ShippingMethod { get; set; }
    public DbSet<ShippingProvider> ShippingProvider { get; set; }
    public DbSet<PurchaseOrderFilesMapping> PurchaseOrderFilesMapping { get; set; }
    public DbSet<ProductPurchaseOrderStockMapping> ProductPurchaseOrderStockMapping { get; set; }
    public DbSet<ProductionVsLaborCostPrice> ProductionVsLaborCostPrice { get; set; }
    public DbSet<ProductionVsLaborCostHistory> ProductionVsLaborCostHistory { get; set; }
    public DbSet<ProductVendorMapping> ProductVendorMapping { get; set; }
    public DbSet<ProductFilesMappings> ProductFilesMappings { get; set; }
    public DbSet<Department> Department { get; set; }
    public DbSet<Employee> Employee { get; set; }
    public DbSet<MessageEmployee> MessageEmployee { get; set; }
    public DbSet<AccessCard> AccessCard { get; set; }
    public DbSet<AccessPoint> AccessPoint { get; set; }
    public DbSet<AccessPlan> AccessPlan { get; set; }
    public DbSet<AccessPlanDoor> AccessPlanDoor { get; set; }
    public DbSet<AccessPlanUser> AccessPlanUser { get; set; }
    public DbSet<AccessPointLog> AccessPointLog { get; set; }
    public DbSet<SellerMargin> SellerMargin { get; set; }
    public DbSet<SellerMargins> SellerMargins { get; set; }
    public DbSet<QCDiagnosis> QCDiagnosis { get; set; }
    public DbSet<QCStationLocation> QCStationLocation { get; set; }
    public DbSet<QualityControlCapture> QualityControlCapture { get; set; }
    public DbSet<InventoryRequestForm> InventoryRequestForm { get; set; }
    public DbSet<HelpRequestForm> HelpRequestForm { get; set; }
    public DbSet<Files> Files { get; set; }
    public DbSet<ProductImage> ProductImage { get; set; }
    public DbSet<WebHookBatch> WebHookBatch { get; set; }
    public DbSet<ProductCustomFulfillment> ProductCustomFulFillment { get; set; }
    public DbSet<CycleCount> CycleCount { get; set; }
    public DbSet<CycleCountFrequency> CycleCountFrequency { get; set; }
    public DbSet<SubCategory> SubCategory { get; set; }
    public DbSet<MoveStockHistory> MoveStockHistory { get; set; }
    public DbSet<NirfForm> NirfForm { get; set; }
    public DbSet<Fonts> Fonts { get; set; }
    public DbSet<NirfInventory> NirfInventory { get; set; }
    public DbSet<NirfParameters> NirfParameters { get; set; }
    public DbSet<NirfPackaging> NirfPackaging { get; set; }
    public DbSet<NirfForecasting> NirfForecasting { get; set; }
    public DbSet<NirfShipping> NirfShipping { get; set; }
    public DbSet<NirfShippingProdivder> NirfShippingProvider { get; set; }
    public DbSet<NirfProductMapping> NirfProductMapping { get; set; }
    public DbSet<NirfImageMapping> NirfImageMapping { get; set; }
    public DbSet<NirfVendorMapping> NirfVendorMapping { get; set; }
    public DbSet<UserImage> UserImage { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }
    public DbSet<ProductTagsRegistry> ProductTagsRegistry { get; set; }
    public DbSet<ProductTag> productTag { get; set; }
    public DbSet<ShipStationStoreFile> ShipStationStoreFiles { get; set; }
    public DbSet<OrderBatch> OrderBatch { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<EmailAlert> EmailAlerts { get; set; }
    public DbSet<UserEmailAlertMapping> UserEmailAlertMapping { get; set; }
    public DbSet<AlertTriggerTemplateMappings> AlertTriggerTemplateMappings { get; set; }
    public DbSet<OrderSource> OrderSource { get; set; }
    public DbSet<OrderBatchItem> OrderBatchItem { get; set; }
    public DbSet<BatchItemStatus> BatchItemStatus { get; set; }
    public DbSet<MyDash> MyDash { get; set; }
    public DbSet<Bundle> Bundle { get; set; }
    public DbSet<BundleItem> BundleItem { get; set; }
    public DbSet<ShippingScanout> ShippingScanout { get; set; }
    public DbSet<UserSiteMapping> UserSiteMapping { get; set; }
    public DbSet<DepartmentRoleMapping> DepartmentRoleMapping { get; set; }
    public DbSet<DHLInvoices> DHLInvoices { get; set; }
    public DbSet<StampsUSPSInvoices> StampsUSPSInvoices { get; set; }
    public DbSet<UPSInvoices> UPSInvoices { get; set; }
    public DbSet<EasyPostInvoices> EasyPostInvoices { get; set; }
    public DbSet<InvoicedOrders> InvoicedOrders { get; set; }
    public DbSet<SkulabsImport> SkulabsImport { get; set; }
    public DbSet<Warehouse> Warehouse { get; set; }
    public DbSet<MiscProduct> MiscProdcut { get; set; }
    public DbSet<ShippingManifest> ShippingManifests { get; set; }

}
