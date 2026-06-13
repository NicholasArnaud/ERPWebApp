using ERPWebApp.Data;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Company;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Services
{
    public class MyDashService : IMyDashService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserPreferencesService _userPreferencesService;
        private readonly IUnitOfWork _unitOfWork;

        public MyDashService(ApplicationDbContext context, IUserPreferencesService userPreferencesService, IUnitOfWork unitOfWork)
        {
            _context = context;
            _userPreferencesService = userPreferencesService;
            _unitOfWork = unitOfWork;
        }
        public async Task<MyDashDTO> GetUserDashboadData(string userId)
        {
            var userDashboardSettings = await _context.MyDash
                                              .Where(u => u.UserId == userId)
                                              .FirstOrDefaultAsync();

            var myDashData = new MyDashDTO();

            if (userDashboardSettings == null)
            {
                myDashData.SpeedOMeter = false;
                myDashData.DepartmentOrderHistory = false;
                myDashData.TopDepartment = false;
                myDashData.YearlyProfit = false;
                myDashData.HistoricalTrends = false;
                myDashData.TotalFulfillmentSales = false;
                myDashData.TopProductSales = false;
                myDashData.SiteVolumetrics = false;
                myDashData.ProductCyleCount = false;
                myDashData.TopRequestedProducts = false;
                myDashData.TopMovedProducts = false;
                myDashData.TopReasonRequest = false;

                return myDashData;
            }
            myDashData.SpeedOMeter = userDashboardSettings.SpeedOMeter;
            myDashData.DepartmentOrderHistory = userDashboardSettings.DepartmentOrderHistory;
            myDashData.TopDepartment = userDashboardSettings.TopDepartment;
            myDashData.YearlyProfit = userDashboardSettings.YearlyProfit;
            myDashData.HistoricalTrends = userDashboardSettings.HistoricalTrends;
            myDashData.TotalFulfillmentSales = userDashboardSettings.TotalFulfillmentSales;
            myDashData.TopProductSales = userDashboardSettings.TopProductSales;
            myDashData.SiteVolumetrics = userDashboardSettings.SiteVolumetrics;
            myDashData.ProductCyleCount = userDashboardSettings.ProductCyleCount;
            myDashData.TopRequestedProducts = userDashboardSettings.TopRequestedProducts;
            myDashData.TopMovedProducts = userDashboardSettings.TopMovedProducts;
            myDashData.TopReasonRequest = userDashboardSettings.TopReasonRequest;

            return myDashData;
        }
        public async Task<MyDashDTO> SaveUserCustomDashBoard(MyDashDTO myDashDTO, string userId)
        {

            var myDash = new MyDash
            {
                UserId = userId,
                SpeedOMeter = myDashDTO.SpeedOMeter,
                DepartmentOrderHistory = myDashDTO.DepartmentOrderHistory,
                TopDepartment = myDashDTO.TopDepartment,
                YearlyProfit = myDashDTO.YearlyProfit,
                HistoricalTrends = myDashDTO.HistoricalTrends,
                TotalFulfillmentSales = myDashDTO.TotalFulfillmentSales,
                TopProductSales = myDashDTO.TopProductSales,
                SiteVolumetrics = myDashDTO.SiteVolumetrics,
                ProductCyleCount = myDashDTO.ProductCyleCount,
                TopRequestedProducts = myDashDTO.TopRequestedProducts,
                TopMovedProducts = myDashDTO.TopMovedProducts,
                TopReasonRequest = myDashDTO.TopReasonRequest
            };

            // Check if the record already exists
            var existingRecord = await _context.MyDash.FirstOrDefaultAsync(m => m.UserId == myDash.UserId);
            if (existingRecord != null)
            {
                // Update the existing record
                existingRecord.SpeedOMeter = myDash.SpeedOMeter;
                existingRecord.DepartmentOrderHistory = myDash.DepartmentOrderHistory;
                existingRecord.TopDepartment = myDash.TopDepartment;
                existingRecord.YearlyProfit = myDash.YearlyProfit;
                existingRecord.HistoricalTrends = myDash.HistoricalTrends;
                existingRecord.TotalFulfillmentSales = myDash.TotalFulfillmentSales;
                existingRecord.TopProductSales = myDash.TopProductSales;
                existingRecord.SiteVolumetrics = myDash.SiteVolumetrics;
                existingRecord.ProductCyleCount = myDash.ProductCyleCount;
                existingRecord.TopRequestedProducts = myDash.TopRequestedProducts;
                existingRecord.TopMovedProducts = myDash.TopMovedProducts;
                existingRecord.TopReasonRequest = myDash.TopReasonRequest;

                _context.Update(existingRecord);
            }
            else
            {
                // Add the new record
                await _context.AddAsync(myDash);
            }

            await _context.SaveChangesAsync();

            // Redirect to a confirmation page or the same page to show the updated values
            return myDashDTO;
        }

        public async Task<IActionResult> UpdateFavouriteStatus(string propertyName, bool value, string userId)
        {
            var userDashboardSettings = await _context.MyDash.FirstOrDefaultAsync(u => u.UserId == userId);


            if (userDashboardSettings == null)
            {
                // If no user settings found, create a new instance with the userId
                userDashboardSettings = new MyDash
                {
                    UserId = userId,
                    // inventory dashboard values
                    SiteVolumetrics = propertyName == nameof(MyDashDTO.SiteVolumetrics) ? value : false,
                    ProductCyleCount = propertyName == nameof(MyDashDTO.ProductCyleCount) ? value : false,
                    TopRequestedProducts = propertyName == nameof(MyDashDTO.TopRequestedProducts) ? value : false,
                    TopMovedProducts = propertyName == nameof(MyDashDTO.TopMovedProducts) ? value : false,
                    TopReasonRequest = propertyName == nameof(MyDashDTO.TopReasonRequest) ? value : false,
                    // operations dashboard values
                    SpeedOMeter = propertyName == nameof(MyDashDTO.SpeedOMeter) ? value : false,
                    DepartmentOrderHistory = propertyName == nameof(MyDashDTO.DepartmentOrderHistory) ? value : false,
                    TopDepartment = propertyName == nameof(MyDashDTO.TopDepartment) ? value : false,
                    //financial dashboard values
                    YearlyProfit = propertyName == nameof(MyDashDTO.YearlyProfit) ? value : false,
                    HistoricalTrends = propertyName == nameof(MyDashDTO.HistoricalTrends) ? value : false,
                    TotalFulfillmentSales = propertyName == nameof(MyDashDTO.TotalFulfillmentSales) ? value : false,
                    TopProductSales = propertyName == nameof(MyDashDTO.TopProductSales) ? value : false
                };

                try
                {
                    await _context.AddAsync(userDashboardSettings);
                    await _context.SaveChangesAsync();

                    return new JsonResult(new { success = true });
                }
                catch (Exception ex)
                {
                    return new JsonResult(new { success = false, message = ex.Message });
                }
            }
            else
            {
                // Fetch layouts, if null or empty, set to an empty list to avoid null issues
                var layouts = (await _userPreferencesService.GetDashboardLayoutByDashboardAsync(userId, DashboardNames.DashboardMyDash.ToString())) ?? new List<DashboardLayout>();

                if(layouts.Count == 0)
                {
                    var property = typeof(MyDash).GetProperty(propertyName);
                    if (property != null)
                    {
                        property.SetValue(userDashboardSettings, value);

                        _context.Update(userDashboardSettings);
                        await _context.SaveChangesAsync();

                        return new JsonResult(new { success = true });

                    }
                }
                
                var preferences = (await _unitOfWork.UserPreferences.FindAsync(e => e.UserId == userId)) ?? new List<UserPreferences>();

                UserPreferences userPreference = preferences.FirstOrDefault();
                var configList = userPreference?.DashboardConfigList ?? new List<DashboardConfig>();
                int maxPosition = layouts.Any() ? layouts.Max(e => e.Position) : 0;
                var myDashConfig = configList.FirstOrDefault(config => config.Name == "DashboardMyDash");
                var filteredList = configList
                    .Where(config => config.Name == "DashboardMyDash");

                try
                {
                    // Update the relevant property
                    switch (propertyName)
                    {
                        case nameof(MyDashDTO.SiteVolumetrics):
                            userDashboardSettings.SiteVolumetrics = value;
                            var hasSiteVolumetrics = filteredList.Any(config => config.Layouts.Any(layout => layout.ElemId == "site-volumetrics-chart"));
                            if (hasSiteVolumetrics && !value)
                            {
                                foreach (var config in filteredList)
                                {
                                    var layoutToRemove = config.Layouts.FirstOrDefault(layout => layout.ElemId == "site-volumetrics-chart");
                                    if (layoutToRemove != null)
                                    {
                                        config.Layouts.Remove(layoutToRemove);
                                    }
                                }
                            }
                            if (!hasSiteVolumetrics && value)
                            {
                                int maxPositionInList = configList.SelectMany(config => config.Layouts).Max(layout => layout.Position);
                                foreach (var config in filteredList)
                                {
                                    var newLayout = new DashboardLayout
                                    {
                                        ElemId = "site-volumetrics-chart",
                                        Position = maxPositionInList + 1
                                    };

                                    config.Layouts.Add(newLayout);
                                }
                            }
                            break;
                        case nameof(MyDashDTO.ProductCyleCount):
                            userDashboardSettings.ProductCyleCount = value;
                            var hasProductCyleCount = filteredList.Any(config => config.Layouts.Any(layout => layout.ElemId == "product-cycle-count-chart"));
                            if (hasProductCyleCount && !value)
                            {
                                foreach (var config in filteredList)
                                {
                                    var layoutToRemove = config.Layouts.FirstOrDefault(layout => layout.ElemId == "product-cycle-count-chart");
                                    if (layoutToRemove != null)
                                    {
                                        config.Layouts.Remove(layoutToRemove);
                                    }
                                }
                            }
                            if (!hasProductCyleCount && value)
                            {
                                int maxPositionInList = configList.SelectMany(config => config.Layouts).Max(layout => layout.Position);
                                foreach (var config in filteredList)
                                {
                                    var newLayout = new DashboardLayout
                                    {
                                        ElemId = "product-cycle-count-chart",
                                        Position = maxPositionInList + 1
                                    };

                                    config.Layouts.Add(newLayout);
                                }
                            }
                            break;
                        case nameof(MyDashDTO.TopRequestedProducts):
                            userDashboardSettings.TopRequestedProducts = value;
                            var hasTopRequestedProducts = filteredList.Any(config => config.Layouts.Any(layout => layout.ElemId == "top-requested-products-chart"));
                            if (hasTopRequestedProducts && !value)
                            {
                                foreach (var config in filteredList)
                                {
                                    var layoutToRemove = config.Layouts.FirstOrDefault(layout => layout.ElemId == "top-requested-products-chart");
                                    if (layoutToRemove != null)
                                    {
                                        config.Layouts.Remove(layoutToRemove);
                                    }
                                }
                            }
                            if (!hasTopRequestedProducts && value)
                            {
                                int maxPositionInList = configList.SelectMany(config => config.Layouts).Max(layout => layout.Position);
                                foreach (var config in filteredList)
                                {
                                    var newLayout = new DashboardLayout
                                    {
                                        ElemId = "top-requested-products-chart",
                                        Position = maxPositionInList + 1
                                    };

                                    config.Layouts.Add(newLayout);
                                }
                            }
                            break;
                        case nameof(MyDashDTO.TopMovedProducts):
                            userDashboardSettings.TopMovedProducts = value;
                            var hasTopMovedProducts = filteredList.Any(config => config.Layouts.Any(layout => layout.ElemId == "top-moved-products-chart"));
                            if (hasTopMovedProducts && !value)
                            {
                                foreach (var config in filteredList)
                                {
                                    var layoutToRemove = config.Layouts.FirstOrDefault(layout => layout.ElemId == "top-moved-products-chart");
                                    if (layoutToRemove != null)
                                    {
                                        config.Layouts.Remove(layoutToRemove);
                                    }
                                }
                            }
                            if (!hasTopMovedProducts && value)
                            {
                                int maxPositionInList = configList.SelectMany(config => config.Layouts).Max(layout => layout.Position);
                                foreach (var config in filteredList)
                                {
                                    var newLayout = new DashboardLayout
                                    {
                                        ElemId = "top-moved-products-chart",
                                        Position = maxPositionInList + 1
                                    };

                                    config.Layouts.Add(newLayout);
                                }
                            }
                            break;
                        case nameof(MyDashDTO.TopReasonRequest):
                            userDashboardSettings.TopReasonRequest = value;
                            var hasTopReasonRequest = filteredList.Any(config => config.Layouts.Any(layout => layout.ElemId == "top-reason-request-chart"));
                            if (hasTopReasonRequest && !value)
                            {
                                foreach (var config in filteredList)
                                {
                                    var layoutToRemove = config.Layouts.FirstOrDefault(layout => layout.ElemId == "top-reason-request-chart");
                                    if (layoutToRemove != null)
                                    {
                                        config.Layouts.Remove(layoutToRemove);
                                    }
                                }
                            }
                            if (!hasTopReasonRequest && value)
                            {
                                int maxPositionInList = configList.SelectMany(config => config.Layouts).Max(layout => layout.Position);
                                foreach (var config in filteredList)
                                {
                                    var newLayout = new DashboardLayout
                                    {
                                        ElemId = "top-reason-request-chart",
                                        Position = maxPositionInList + 1
                                    };

                                    config.Layouts.Add(newLayout);
                                }
                            }
                            break;
                        case nameof(MyDashDTO.YearlyProfit):
                            userDashboardSettings.YearlyProfit = value;
                            var hasYearlyProfit = filteredList.Any(config => config.Layouts.Any(layout => layout.ElemId == "yearly-profit-chart"));
                            if (hasYearlyProfit && !value)
                            {
                                foreach (var config in filteredList)
                                {
                                    var layoutToRemove = config.Layouts.FirstOrDefault(layout => layout.ElemId == "yearly-profit-chart");
                                    if (layoutToRemove != null)
                                    {
                                        config.Layouts.Remove(layoutToRemove);
                                    }
                                }
                            }
                            if (!hasYearlyProfit && value)
                            {
                                int maxPositionInList = configList.SelectMany(config => config.Layouts).Max(layout => layout.Position);
                                foreach (var config in filteredList)
                                {
                                    var newLayout = new DashboardLayout
                                    {
                                        ElemId = "yearly-profit-chart",
                                        Position = maxPositionInList + 1
                                    };

                                    config.Layouts.Add(newLayout);
                                }
                            }
                            break;
                        case nameof(MyDashDTO.HistoricalTrends):
                            userDashboardSettings.HistoricalTrends = value;
                            var hasHistoricalTrends = filteredList.Any(config => config.Layouts.Any(layout => layout.ElemId == "historical-trends-chart"));
                            if (hasHistoricalTrends && !value)
                            {
                                foreach (var config in filteredList)
                                {
                                    var layoutToRemove = config.Layouts.FirstOrDefault(layout => layout.ElemId == "historical-trends-chart");
                                    if (layoutToRemove != null)
                                    {
                                        config.Layouts.Remove(layoutToRemove);
                                    }
                                }
                            }
                            if (!hasHistoricalTrends && value)
                            {
                                int maxPositionInList = configList.SelectMany(config => config.Layouts).Max(layout => layout.Position);
                                foreach (var config in filteredList)
                                {
                                    var newLayout = new DashboardLayout
                                    {
                                        ElemId = "historical-trends-chart",
                                        Position = maxPositionInList + 1
                                    };

                                    config.Layouts.Add(newLayout);
                                }
                            }
                            break;
                        case nameof(MyDashDTO.TotalFulfillmentSales):
                            userDashboardSettings.TotalFulfillmentSales = value;
                            var hasTotalFulfillmentSales = filteredList.Any(config => config.Layouts.Any(layout => layout.ElemId == "total-fulfillment-sales-chart"));
                            if (hasTotalFulfillmentSales && !value)
                            {
                                foreach (var config in filteredList)
                                {
                                    var layoutToRemove = config.Layouts.FirstOrDefault(layout => layout.ElemId == "total-fulfillment-sales-chart");
                                    if (layoutToRemove != null)
                                    {
                                        config.Layouts.Remove(layoutToRemove);
                                    }
                                }
                            }
                            if (!hasTotalFulfillmentSales && value)
                            {
                                int maxPositionInList = configList.SelectMany(config => config.Layouts).Max(layout => layout.Position);
                                foreach (var config in filteredList)
                                {
                                    var newLayout = new DashboardLayout
                                    {
                                        ElemId = "total-fulfillment-sales-chart",
                                        Position = maxPositionInList + 1
                                    };

                                    config.Layouts.Add(newLayout);
                                }
                            }
                            break;
                        case nameof(MyDashDTO.TopProductSales):
                            userDashboardSettings.TopProductSales = value;
                            var hasTopProductSales = filteredList.Any(config => config.Layouts.Any(layout => layout.ElemId == "top-product-sales-chart"));
                            if (hasTopProductSales && !value)
                            {
                                foreach (var config in filteredList)
                                {
                                    var layoutToRemove = config.Layouts.FirstOrDefault(layout => layout.ElemId == "top-product-sales-chart");
                                    if (layoutToRemove != null)
                                    {
                                        config.Layouts.Remove(layoutToRemove);
                                    }
                                }
                            }
                            if (!hasTopProductSales && value)
                            {
                                int maxPositionInList = configList.SelectMany(config => config.Layouts).Max(layout => layout.Position);
                                foreach (var config in filteredList)
                                {
                                    var newLayout = new DashboardLayout
                                    {
                                        ElemId = "top-product-sales-chart",
                                        Position = maxPositionInList + 1
                                    };

                                    config.Layouts.Add(newLayout);
                                }
                            }
                            break;
                        case nameof(MyDashDTO.SpeedOMeter):
                            userDashboardSettings.SpeedOMeter = value;
                            var hasSpeedOMeter = filteredList.Any(config => config.Layouts.Any(layout => layout.ElemId == "speedometer-chart"));
                            if (hasSpeedOMeter && !value)
                            {
                                foreach (var config in filteredList)
                                {
                                    var layoutToRemove = config.Layouts.FirstOrDefault(layout => layout.ElemId == "speedometer-chart");
                                    if (layoutToRemove != null)
                                    {
                                        config.Layouts.Remove(layoutToRemove);
                                    }
                                }
                            }
                            if (!hasSpeedOMeter && value)
                            {
                                int maxPositionInList = configList.SelectMany(config => config.Layouts).Max(layout => layout.Position);
                                foreach (var config in filteredList)
                                {
                                    var newLayout = new DashboardLayout
                                    {
                                        ElemId = "speedometer-chart",
                                        Position = maxPositionInList + 1
                                    };

                                    config.Layouts.Add(newLayout);
                                }
                            }
                            break;
                        case nameof(MyDashDTO.DepartmentOrderHistory):
                            userDashboardSettings.DepartmentOrderHistory = value;
                            var hasDepartmentOrderHistory = filteredList.Any(config => config.Layouts.Any(layout => layout.ElemId == "department-order-history-chart"));
                            if (hasDepartmentOrderHistory && !value)
                            {
                                foreach (var config in filteredList)
                                {
                                    var layoutToRemove = config.Layouts.FirstOrDefault(layout => layout.ElemId == "department-order-history-chart");
                                    if (layoutToRemove != null)
                                    {
                                        config.Layouts.Remove(layoutToRemove);
                                    }
                                }
                            }
                            if (!hasDepartmentOrderHistory && value)
                            {
                                int maxPositionInList = configList.SelectMany(config => config.Layouts).Max(layout => layout.Position);
                                foreach (var config in filteredList)
                                {
                                    var newLayout = new DashboardLayout
                                    {
                                        ElemId = "department-order-history-chart",
                                        Position = maxPositionInList + 1
                                    };

                                    config.Layouts.Add(newLayout);
                                }
                            }
                            break;
                        case nameof(MyDashDTO.TopDepartment):
                            userDashboardSettings.TopDepartment = value;
                            var hasTopDepartment = filteredList.Any(config => config.Layouts.Any(layout => layout.ElemId == "top-department-chart"));
                            if (hasTopDepartment && !value)
                            {
                                foreach (var config in filteredList)
                                {
                                    var layoutToRemove = config.Layouts.FirstOrDefault(layout => layout.ElemId == "top-department-chart");
                                    if (layoutToRemove != null)
                                    {
                                        config.Layouts.Remove(layoutToRemove);
                                    }
                                }
                            }
                            if (!hasTopDepartment && value)
                            {
                                int maxPositionInList = configList.SelectMany(config => config.Layouts).Max(layout => layout.Position);
                                foreach (var config in filteredList)
                                {
                                    var newLayout = new DashboardLayout
                                    {
                                        ElemId = "top-department-chart",
                                        Position = maxPositionInList + 1
                                    };

                                    config.Layouts.Add(newLayout);
                                }
                            }
                            break;
                        default:
                            return new JsonResult(new { success = false, message = "Invalid property name." });
                    }
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // Update the user preferences
                            userPreference.DashboardConfigList = configList;
                            _unitOfWork.UserPreferences.Update(userPreference);
                            await _unitOfWork.SaveChangesAsync();

                            // Update the user dashboard settings
                            _context.Update(userDashboardSettings);
                            await _context.SaveChangesAsync();

                            // Commit the transaction
                            await transaction.CommitAsync();
                        }
                        catch (Exception)
                        {
                            // Rollback the transaction if something goes wrong
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                    return new JsonResult(new { success = true });

                }
                catch (Exception ex)
                {
                    return new JsonResult(new { success = false, message = ex.Message });
                }
            }
        }

    }

}

