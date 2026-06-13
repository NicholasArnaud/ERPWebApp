using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using ERPWebApp.Data;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Services;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using ERPWebApp.Models.Config;
using ERPWebApp.Data.Repositories;
using ERPWebApp.Providers.Interfaces;
using ERPWebApp.Providers;
using Microsoft.FeatureManagement;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuestPDF.Infrastructure;
using Polly;
using Microsoft.Extensions.Http.Resilience;
using ERPWebApp.Health;

var builder = WebApplication.CreateBuilder(args);


var connectionString = "";
if (builder.Environment.IsDevelopment())
    connectionString = builder.Configuration.GetConnectionString("LocalConnection");
else
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString)
    .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.ShadowPropertyCreated, CoreEventId.IncompatibleMatchingForeignKeyProperties)));

if (builder.Environment.IsProduction())
{
    // The following line enables Application Insights telemetry collection.
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}
else
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
// Setting max file size higher specifically for invoices, since some invoices are going into 50MB+ range.
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
});

var adClient = builder.Configuration.GetValue<string>("AzureAd:ClientId");
var adClientSecret = builder.Configuration.GetValue<string>("AzureAd:ClientSecret");
var adInstance = builder.Configuration.GetValue<string>("AzureAd:Instance");
var adTenantId = builder.Configuration.GetValue<string>("AzureAd:TenantId");
var adCallbackPath = builder.Configuration.GetValue<string>("AzureAd:CallbackPath");
var appConfig = builder.Configuration.GetValue<string>("ExternalEndpoints:AppConfig");
builder.Services.Configure<ExternalEndpoints>(builder.Configuration.GetSection("ExternalEndpoints"));
builder.Services.Configure<SeedUser>(builder.Configuration.GetSection("SeedData:User"));

bool useAzureAppConfig = false;

QuestPDF.Settings.License = LicenseType.Community;

if (!string.IsNullOrEmpty(appConfig))
{
    builder.Configuration.AddAzureAppConfiguration(opt =>
    {
        opt.Connect(new Uri(appConfig), new DefaultAzureCredential())
            .ConfigureRefresh(rfs =>
            {
                rfs.Register("SentinelKey", refreshAll: true)
                    .SetRefreshInterval(TimeSpan.FromHours(1));
            })
            .UseFeatureFlags(x => x.SetRefreshInterval(TimeSpan.FromHours(1)));
    });

    useAzureAppConfig = true;
}

if (!string.IsNullOrEmpty(adClient) &&
    !string.IsNullOrEmpty(adClientSecret) &&
    !string.IsNullOrEmpty(adInstance) &&
    !string.IsNullOrEmpty(adTenantId) &&
    !string.IsNullOrEmpty(adCallbackPath)
)
{
    // Add services to the container.
    builder.Services.AddAuthentication()
        .AddMicrosoftAccount(microsoftOptions =>
        {
            microsoftOptions.ClientId = adClient;
            microsoftOptions.ClientSecret = adClientSecret;
            microsoftOptions.AuthorizationEndpoint = $"{adInstance}{adTenantId}/oauth2/v2.0/authorize";
            microsoftOptions.TokenEndpoint = $"{adInstance}{adTenantId}/oauth2/v2.0/token";
            microsoftOptions.CallbackPath = adCallbackPath;
            microsoftOptions.SaveTokens = true;
        });
}

builder.Services
    .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultUI()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build()
);
builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement();
builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("Azure Db")
    .AddCheck<ShipStationHealthCheck>("ShipSation API");
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>()
    .AddTransient<IWebhooks, Webhooks>()
    .AddTransient(typeof(IService<>), typeof(Service<>))
    .AddScoped<IOrderShippingService, OrderShippingService>()
    .AddScoped<IOrderTagService, OrderTagService>()
    .AddScoped<IOrderFulfillmentService, OrderFulfillmentService>()
    .AddTransient<IDepartmentService, DepartmentService>()
    .AddTransient<IEmployeeService, EmployeeService>()
    .AddTransient<ISpeedOMeterGoalService, SpeedOMeterGoalService>()
    .AddTransient<IStocksService, StocksService>()
    .AddTransient<IShipStationStoreService, ShipStationStoreService>()
    .AddTransient<IProductFilesMappingsService, ProductFilesMappingsService>()
    .AddTransient<IFilesService, FilesService>()
    .AddTransient<ILocationService, LocationService>()
    .AddTransient<IMoveStockHistoryService, MoveStockHistoryService>()
    .AddTransient<IProductPurchaseOrderService, ProductPurchaseOrderService>()
    .AddTransient<IProductPurchaseOrderStockMappingService, ProductPurchaseOrderStockMappingService>()
    .AddTransient<IProductService, ProductService>()
    .AddTransient<IProductVendorMappingService, ProductVendorMappingService>()
    .AddTransient<IPurchaseOrderFilesMappingService, PurchaseOrderFilesMappingService>()
    .AddTransient<IPurchaseOrderService, PurchaseOrderService>()
    .AddTransient<IShippingMethodService, ShippingMethodService>()
    .AddTransient<IShippingProviderService, ShippingProviderService>()
    .AddTransient<ISkuCategoryService, SkuCategoryService>()
    .AddTransient<ISkuColorService, SkuColorService>()
    .AddTransient<ISkuUnitOfMeasureService, SkuUnitOfMeasureService>()
    .AddTransient<ISubCategoryService, SubCategoryService>()
    .AddTransient<IVendorService, VendorService>()
    .AddTransient<INirfProductMappingService, NirfProductMappingService>()
    .AddTransient<IUserService, UserService>()
    .AddTransient<IFontService, FontService>()
    .AddTransient<INirfForecastingService, NirfForecastingService>()
    .AddTransient<INirfFormService, NirfFormService>()
    .AddTransient<INirfImageMappingService, NirfImageMappingService>()
    .AddTransient<INirfInventoryService, NirfInventoryService>()
    .AddTransient<INirfPackagingService, NirfPackagingService>()
    .AddTransient<INirfParametersService, NirfParametersService>()
    .AddTransient<INirfShippingService, NirfShippingService>()
    .AddTransient<INirfVendorMappingService, NirfVendorMappingService>()
    .AddTransient<ISiteService, SiteService>()
    .AddTransient<IFinancialsService, FinancialsService>()
    .AddTransient<IInventoryService, InventoryService>()
    .AddTransient<IProductContainerService, ProductContainerService>()
    .AddTransient<IInventoryRequestFormService, InventoryRequestFormService>()
    .AddTransient<ICycleCountFrequencyService, CycleCountFrequencyService>()
    .AddTransient<ICycleCountService, CycleCountService>()
    .AddTransient<IProductionVsLaborCostPriceService, ProductionVsLaborCostPriceService>()
    .AddScoped<IOrderService, OrderService>()
    .AddTransient<IProductImageService, ProductImageService>()
    .AddTransient<IWebhookBatchService, WebhookBatchService>()
    .AddTransient<IUserImageService, UserImageService>()
    .AddTransient<IUserPreferencesService, UserPreferencesService>()
    .AddTransient<ISellerMarginService, SellerMarginService>()
    .AddTransient<IHomeService, HomeService>()
    .AddTransient<IProductTagService, ProductTagService>()
    .AddTransient<IShipStationAwaitingOrderServices, ShipStationAwaitingOrderServices>()
    .AddTransient<IShipStationOrderedHistoryService, ShipStationOrderedHistoryService>()
    .AddTransient<IBarcodeScanService, BarcodeScanService>()
    .AddTransient<IShippingScanoutService, ShippingScanoutService>()
    .AddScoped<IOrderBatchService, OrderBatchService>()
    .AddTransient<IInventoryBalanceService, InventoryBalanceService>()
    .AddScoped<IOrderItemService, OrderItemService>()
    .AddTransient<IBatchViewService, BatchViewService>()
    .AddTransient<IEmailAlertsService, EmailAlertsService>()
    .AddTransient<IBatchItemStatusService, BatchItemStatusService>()
    .AddTransient<IAuditLogService, AuditLogService>()
    .AddTransient<IBundleService, BundleService>()
    .AddTransient<IBundleItemService, BundleItemService>()
    .AddTransient<IUserSiteMappingService, UserSiteMappingService>()
    .AddTransient<IMyDashService, MyDashService>()
    .AddTransient<IWarehouseService, WarehouseService>()
    .AddTransient<IDepartmentRoleMappingService, DepartmentRoleMappingService>()
    .AddTransient<ITriggerEmailAlertService, TriggerEmailAlertService>()
    .AddSingleton<IGraphAPIService, GraphAPIService>()
    .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
    .AddSingleton<IEmailScheduler, EmailSchedulerHostedService>()
    .AddSingleton<ITriggerEmailAlertManager, TriggerEmailAlertManager>()
    .AddScoped<IUserProvider, UserProvider>()
    .AddScoped<IInvoiceService, InvoiceService>()
    .AddDistributedMemoryCache();

builder.Services.AddHttpClient(
    "ShipStationV1",
    client =>
    {
        client.BaseAddress = new Uri("https://ssapi.shipstation.com/");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Authorization", $"Basic {builder.Configuration.GetValue<string>("ShipStationAuth")}");
        client.DefaultRequestHeaders.Add("x-partner", builder.Configuration.GetValue<string>("ShipStationXKey"));
    })
    .AddResilienceHandler("RetrySS", (resiliencePipelineBuilder, context) =>
    {
        resiliencePipelineBuilder
        .AddRetry(new HttpRetryStrategyOptions
        {
            ShouldHandle = args => args.Outcome switch
            {
                { Result.StatusCode: System.Net.HttpStatusCode.TooManyRequests } => PredicateResult.True(),
                { Result.IsSuccessStatusCode: false } => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            MaxRetryAttempts = 2,
            DelayGenerator = args =>
            {
                if (args.Outcome.Result is HttpResponseMessage response)
                {
                    if (response.Headers.TryGetValues("X-Rate-Limit-Reset", out IEnumerable<string>? retryAfterValues))
                    {
                        var retryAfter = retryAfterValues.FirstOrDefault();
                        if (int.TryParse(retryAfter, out int seconds))
                        {
                            TimeSpan delay = TimeSpan.FromSeconds(seconds);
                            return new ValueTask<TimeSpan?>(delay);
                        }
                    }
                }
                TimeSpan defaultDelay = TimeSpan.FromSeconds(Math.Pow(2, args.AttemptNumber));
                return new ValueTask<TimeSpan?>(defaultDelay);
            },
            OnRetry = async (outcome) =>
            {
                await Task.CompletedTask;
            }
        });
    });
builder.Services.AddHttpClient(
    "ShipEngineV1",
    client =>
    {
        client.BaseAddress = new Uri("https://api.shipengine.com/v1/");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("API-Key", builder.Configuration.GetValue<string>("ShipEngineAuth"));
    });

builder.Services.AddSingleton<ICachedTokenService, CachedTokenService>();
builder.Services.AddSingleton<IExternalTokenService, ExternalTokenService>();
builder.Services.AddTransient<TokenRetrievalHandler>();
builder.Services.AddHttpClient(
    "USPS",
    client =>
    {
        client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("Usps:ApiUrl"));
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    })
    .AddHttpMessageHandler<TokenRetrievalHandler>()
    .AddResilienceHandler("RetryUSPS", (resiliencePipelineBuilder, context) =>
    {
        resiliencePipelineBuilder
        .AddRetry(new HttpRetryStrategyOptions
        {
            ShouldHandle = args => args.Outcome switch
            {
                { Exception: HttpRequestException } => PredicateResult.True(),
                { Result.StatusCode: System.Net.HttpStatusCode.Unauthorized } => PredicateResult.True(),
                { Result.StatusCode: System.Net.HttpStatusCode.BadRequest } => PredicateResult.False(),
                { Result.IsSuccessStatusCode: false } => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(2),
            UseJitter = true,
            OnRetry = async (outcome) =>
            {
                if (outcome.Outcome.Result?.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await context.ServiceProvider.GetRequiredService<ICachedTokenService>().RefreshTokenAsync(outcome.Context);
                }
            }
        });
    });


builder.Services.AddHostedService<EmailSchedulerHostedService>();
builder.Services.AddScoped<IScheduledEmailRepository, ScheduledEmailRepository>();
builder.Services.AddScoped<IScheduledEmailService, ScheduledEmailService>();
builder.Services.AddScoped<Webhooks>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);
builder.Services.AddRazorPages();
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

builder.Services.Configure<AzureStorageConfig>(builder.Configuration.GetSection("AzureStorageConfig"));

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(6);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

//TO DO: Reimplement preferences later.
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromDays(1);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});
// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();
if (app.Environment.IsDevelopment())
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        await SeedData.Initialize(services);
    }

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
if (useAzureAppConfig)
{
    app.UseAzureAppConfiguration();
}
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
builder.Services.AddDataProtection();
//TO DO: Reimplement preferences later.
//app.UseSession();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
