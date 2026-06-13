using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.PurchaseOrders;
using ERPWebApp.Models;
using ERPWebApp.Providers.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Data;

public static class SeedData
{
    public static async Task<Task> Initialize(IServiceProvider serviceProvider)
    {
        using (
            var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>(),
              serviceProvider.GetRequiredService<IUserProvider>())
        )
        {
            await context.Database.MigrateAsync();
            SeedDB(context, serviceProvider);
        } 

        return Task.CompletedTask;
    }

    private static async Task<string> EnsureUser(
        IServiceProvider serviceProvider,
        string testUserPw,
        string UserName
    )
    {
        var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

        var user = await userManager.FindByNameAsync(UserName);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = UserName,
                Email = UserName + "@completeful.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, testUserPw);
        }

        if (user == null)
        {
            throw new Exception("The password is probably not strong enough!");
        }

        return user.Id;
    }

    private static async Task<IdentityResult> EnsureRole(
        IServiceProvider serviceProvider,
        string uid,
        string role
    )
    {
        IdentityResult IR = null;
        var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

        if (roleManager == null)
        {
            throw new Exception("roleManager null");
        }

        if (!await roleManager.RoleExistsAsync(role))
        {
            IR = await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

        var user = await userManager.FindByIdAsync(uid);

        if (user == null)
        {
            throw new Exception("The testUserPw password was probably not strong enough!");
        }

        IR = await userManager.AddToRoleAsync(user, role);

        return IR;
    }

    public static void SeedDB(ApplicationDbContext context, IServiceProvider serviceProvider)
    {
        if (!context.Roles.Any())
        {
            foreach (var pi in typeof(RoleList).GetFields())
            {
                context.Roles.Add(
                    new IdentityRole
                    {
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        Name = pi.GetValue(null).ToString(),
                        NormalizedName = pi.GetValue(null).ToString().ToUpper()
                    }
                    );
            }

            context.SaveChanges();
        }

        if (!context.Users.Any())
        {
            var users = LoadSeedData<SeedUser>(serviceProvider, "SeedData");
            foreach (var user in users)
            {
                var defaultUser = new IdentityUser
                {
                    //Username needs to match the email before the @ sign
                    UserName = user.UserName,
                    NormalizedUserName = user.UserName.ToUpper(),
                    Email = user.Email,
                    NormalizedEmail = user.Email.ToUpper(),
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    AccessFailedCount = 0
                };
                var password = new PasswordHasher<IdentityUser>();
                //keep this secret, keep this safe
                var hashed = password.HashPassword(defaultUser, user.DefaultPassword);
                defaultUser.PasswordHash = hashed;
                context.Users.Add(defaultUser);
            }
            context.SaveChanges();
        }

        if (!context.UserRoles.Any())
        {
            context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {
                    RoleId = context.Roles.First(x => x.Name == "Administrator").Id,
                    UserId = context.Users.First().Id
                }
            );
            context.SaveChanges();
        }

        if (!context.Site.Any())
        {
            context.Site.Add(
                new Site
                {
                    SiteName = "Test Site",
                    SiteDescription = "The Place Where We Test",
                    IsActive = true,
                    SiteVolume = 12345.56M
                }
            );
            context.Site.Add(
                new Site
                {
                    SiteName = "The Other Site",
                    SiteDescription = "The Site on The Other Side",
                    IsActive = true,
                    SiteVolume = 34911M
                }
            );
            context.SaveChanges();
        }

        if (!context.Location.Any())
        {
            context.Location.Add(
                new Location
                {
                    IsActive = true,
                    LocationName = "Test Location",
                    LocationDescription = "This location is seeded data",
                    SiteId = 1,
                    Type = LocationType.Normal
                }
            );
            context.Location.Add(
                new Location
                {
                    IsActive = true,
                    LocationName = "The Other Location",
                    LocationDescription = "This location is on The Other Side",
                    SiteId = 2,
                    Type = LocationType.ReceiveOnly
                }
            );
            context.SaveChanges();
        }

        if (!context.Department.Any())
        {
            context.Department.AddRange(
                [new Models.Company.Department
                {
                    DepartmentName = "Test Department",
                    IsActive = true,
                    IsProduction = true
                },
                new Models.Company.Department{
                    DepartmentName = "External",
                    IsActive = true,
                    IsProduction = false
                }]
            );
            context.SaveChanges();
        }

        if (!context.Product.Any())
        {
            context.Product.Add(
                new Product
                {
                    Description = "Test Product",
                    Sku = "Test Product",
                    AltItemNumber = "Test Numbah",
                    Cost = 8675,
                    FulfillmentCost = 309,
                    DimensionalUnit = DimensionalUnit.Feet,
                    Height = 1,
                    Length = 1,
                    Width = 1,
                    IsActive = true,
                    IsEmbroidery = true,
                    IsMetal = false,
                    IsUv = true,
                    IsEngraving = false,
                    OnOrder = 10,
                    WeightAmount = 12,
                    WeightUnit = WeightUnit.Ounce,
                    LeadTime = 3
                }
            );
            context.SaveChanges();
        }

        //if (!context.Stock.Any())
        //{
        //    context.Stock.Add(
        //        new Stock
        //        {
        //            IsPrimary = true,
        //            LocationId = 1,
        //            ProductId = 2,
        //            RecentlyReadded = true
        //        }
        //    );
        //    context.Stock.Add(
        //        new Stock
        //        {
        //            IsPrimary = true,
        //            LocationId = 1,
        //            ProductId = 2,
        //            RecentlyReadded = true
        //        }
        //    );
        //    context.SaveChanges();
        //}

        //if (!context.Vendor.Any())
        //{
        //    context.Vendor.Add(
        //        new Vendor
        //        {
        //            Address1 = "111",
        //            Address2 = "123",
        //            BusinessEmail = "a@a.com",
        //            City = "lafayette",
        //            Country = "USA",
        //            VendorName = "Test Vendor",
        //            State = "LA"
        //        }
        //    );
        //    context.SaveChanges();
        //}

        if (!context.ShippingProvider.Any())
        {
            context.ShippingProvider.Add(
                new ShippingProvider
                {
                    ShippingProviderName = "FedEx",
                    ModifyDate = DateTime.Now,
                    ModifyByUser = "Admin",
                    IsActive = true
                }
            );
            context.ShippingProvider.Add(
                new ShippingProvider
                {
                    ShippingProviderName = "UPS",
                    ModifyDate = DateTime.Now,
                    ModifyByUser = "Admin",
                    IsActive = true
                }
            );
            context.ShippingProvider.Add(
                new ShippingProvider
                {
                    ShippingProviderName = "USPS",
                    ModifyDate = DateTime.Now,
                    ModifyByUser = "Admin",
                    IsActive = true
                }
            );
            context.SaveChanges();
        }

        if (!context.ShippingMethod.Any())
        {
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "Priority Overnight", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 1 });
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "Overnight", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 1 });
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "2 Day", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 1 });
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "Express Saver - 3 Day", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 1 });
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "Ground", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 1 });
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "Ground", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 2 });
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "Next Day Air AM", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 2 });
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "Next Day Air", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 2 });
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "3 Day Select", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 2 });
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "Priority Mail Express", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 3 });
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "Priority Mail", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 3 });
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "First Class Mail", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 3 });
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "Retail Ground", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 3 });
            context.ShippingMethod.Add(new ShippingMethod { ShippingMethodName = "Media Mail", ModifyDate = DateTime.Now, ModifyByUser = "Admin", IsActive = true, ShippingProviderId = 3 });
            context.SaveChanges();
        }
        if (!context.ShipStationStore.Any())
        {
            context.ShipStationStore.Add(new Models.ShipStationStore { StoreId = 1002247, StoreName = "TestShop", Email = "Management@completeful.com" });
            context.SaveChanges();
        }
        if (!context.OrderSource.Any())
        {
            foreach (Order.OrderSourceEnum os in System.Enum.GetValues(typeof(Order.OrderSourceEnum)))
            {
                context.OrderSource.Add(new Order.OrderSource { Name = os });
            }
            context.SaveChanges();

            var orders = context.Orders.ToList();
            foreach (var order in orders)
            {
                //Add the default source  
                order.Sources.Add(context.OrderSource.Single(x => x.Name == Order.OrderSourceEnum.custom));
                //Add OrderDesk Source  
                if (order.orderKey.Contains("OD"))
                    order.Sources.Add(context.OrderSource.Single(x => x.Name == Order.OrderSourceEnum.orderdesk));
                //Add ShipStation source  
                if (order.orderKey.Any())
                    order.Sources.Add(context.OrderSource.Single(x => x.Name == Order.OrderSourceEnum.shipstation));
            }

            context.Orders.UpdateRange(orders);
            context.SaveChanges();

        }

        // Fetch the list of departments  
        var departments = context.Department.ToList();

        // Loop through each department  
        foreach (var department in departments)
        {
            // Check if there are any BatchItemStatus entries with this DepartmentId  
            var hasBatchItemStatusEntries = context.BatchItemStatus.Any(bis => bis.DepartmentId == department.DepartmentId);

            // If no BatchItemStatus entries are found for this department, create two new entries  
            if (!hasBatchItemStatusEntries)
            {
                context.BatchItemStatus.AddRange(
                    new BatchItemStatus
                    {
                        StatusName = "Open",
                        DepartmentId = department.DepartmentId,
                        ExecutionSequence = 1,
                        IsDeletable = false
                    },
                    new BatchItemStatus
                    {
                        StatusName = "Completed",
                        DepartmentId = department.DepartmentId,
                        ExecutionSequence = 2,
                        IsDeletable = false
                    }
                );
                context.SaveChanges();
            }
        }
    }

    private static void GeneratePrimitive(ApplicationDbContext context)
    {
        var availableTables = context.Model.GetEntityTypes().Select(t => t.GetTableName()).Distinct().ToList();
        foreach (string table in availableTables)
        {
            var tableType = context.Model.GetEntityTypes().First(c => c.GetTableName() == table);
            dynamic tableInst = tableType;

            foreach (var property in tableType.GetProperties())
            {
                dynamic selectedValue;
                var type = property.ClrType;
                Random rnd = new Random();
                selectedValue = Type.GetTypeCode(type) switch
                {
                    TypeCode.Int32 => rnd.Next(),
                    TypeCode.Int64 => rnd.NextInt64(),
                    TypeCode.Decimal or TypeCode.Single => rnd.NextSingle(),
                    TypeCode.Double => rnd.NextDouble(),
                    TypeCode.Boolean => rnd.Next(0, 1) switch { >= 1 => true, <= 0 => false },
                    _ => null
                };
                if (selectedValue == null && Type.GetTypeCode(type) == TypeCode.String)
                {
                    const string AllowChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz#@$^*()";
                    string randomString = "";
                    for (int c = 0; c < AllowChars.Length; c++)
                    {
                        randomString.Append(AllowChars[rnd.Next(0, AllowChars.Length)]);
                        if (rnd.Next(0, 10) == 1)
                            break;
                    }
                }
            }

        }
    }
    private static List<T> LoadSeedData<T>(IServiceProvider serviceProvider, string configKey)
    {
        var configuration = serviceProvider.GetService<IConfiguration>();
        var configData = configuration.GetSection(configKey).Get<List<T>>();
        return configData;
    }
}
public class SeedUser
{
    public string UserName { get; set; }
    public string DefaultPassword { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
}
