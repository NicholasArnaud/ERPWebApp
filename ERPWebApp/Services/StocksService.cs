using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ERPWebApp.Services;
public class StocksService(
    IUnitOfWork unitOfWork,
    IHttpContextAccessor http
) : Service<Stock>(unitOfWork), IStocksService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public decimal SetupVolumeTally(int ChosenSite)
    {
        decimal InchToFeet = 1m / (12m * 12m * 12m);
        decimal CentiToFeet = 1m / (2.54m * 12m * 2.54m * 12m * 2.54m * 12m);
        decimal MeterToFeet = (100m * 100m * 100m) / (2.54m * 12m * 2.54m * 12m * 2.54m * 12m);

        var total = (decimal)0;

        try
        {

            var stockviewermodelt = _unitOfWork.Stocks.GetStockProductContainersBySiteId(ChosenSite);

            //calculates the site volumetrics  for any containers or products use inches and converst them to feet
            var totalInventoryIncht = (decimal)0;
            totalInventoryIncht = stockviewermodelt.Where(x => x.ProductContainer.ContainerQuantity > 1 && x.ProductContainer.ContainerDiminsions == ContainerDiminsions.Inches).Sum(y => y.ProductContainer.Length * y.ProductContainer.Width * y.ProductContainer.Height * Math.Ceiling((decimal)y.Stock.TotalAvailable / (decimal)y.ProductContainer.ContainerQuantity));//*inch3tofeet3);
            totalInventoryIncht += stockviewermodelt.Where(x => (x.ProductContainer.ContainerQuantity <= 1 || x.ProductContainer == null) && x.Stock.Products.DimensionalUnit == DimensionalUnit.Inches).Sum(y => y.Stock.Products.Length * y.Stock.Products.Width * y.Stock.Products.Height * y.Stock.TotalAvailable);
            totalInventoryIncht = totalInventoryIncht * InchToFeet;

            //calculates the site volumetrics  for any containers or products use feet
            var totalInventoryFeett = (decimal)0;
            totalInventoryFeett = stockviewermodelt.Where(x => x.ProductContainer.ContainerQuantity > 1 && x.ProductContainer.ContainerDiminsions == ContainerDiminsions.Feet).Sum(y => y.ProductContainer.Length * y.ProductContainer.Width * y.ProductContainer.Height * Math.Ceiling((decimal)y.Stock.TotalAvailable / (decimal)y.ProductContainer.ContainerQuantity));//*inch3tofeet3);
            totalInventoryFeett += stockviewermodelt.Where(x => (x.ProductContainer.ContainerQuantity <= 1 || x.ProductContainer == null) && x.Stock.Products.DimensionalUnit == DimensionalUnit.Feet).Sum(y => y.Stock.Products.Length * y.Stock.Products.Width * y.Stock.Products.Height * y.Stock.TotalAvailable);

            //calculates the site volumetrics  for any containers or products use centimeters and converst them to feet
            var totalInventoryCentit = (decimal)0;
            totalInventoryCentit = stockviewermodelt.Where(x => x.ProductContainer.ContainerQuantity > 1 && x.ProductContainer.ContainerDiminsions == ContainerDiminsions.Centimeters).Sum(y => y.ProductContainer.Length * y.ProductContainer.Width * y.ProductContainer.Height * Math.Ceiling((decimal)y.Stock.TotalAvailable / (decimal)y.ProductContainer.ContainerQuantity));//*inch3tofeet3);
            totalInventoryCentit += stockviewermodelt.Where(x => (x.ProductContainer.ContainerQuantity <= 1 || x.ProductContainer == null) && x.Stock.Products.DimensionalUnit == DimensionalUnit.Centimeters).Sum(y => y.Stock.Products.Length * y.Stock.Products.Width * y.Stock.Products.Height * y.Stock.TotalAvailable);
            totalInventoryCentit = totalInventoryCentit * CentiToFeet;

            //calculates the site volumetrics  for any containers or products use meters and converst them to feet
            var totalInventoryMetert = (decimal)0;
            totalInventoryMetert = stockviewermodelt.Where(x => x.ProductContainer.ContainerQuantity > 1 && x.ProductContainer.ContainerDiminsions == ContainerDiminsions.Meters).Sum(y => y.ProductContainer.Length * y.ProductContainer.Width * y.ProductContainer.Height * Math.Ceiling((decimal)y.Stock.TotalAvailable / (decimal)y.ProductContainer.ContainerQuantity));//*inch3tofeet3);
            totalInventoryMetert += stockviewermodelt.Where(x => (x.ProductContainer.ContainerQuantity <= 1 || x.ProductContainer == null) && x.Stock.Products.DimensionalUnit == DimensionalUnit.Meters).Sum(y => y.Stock.Products.Length * y.Stock.Products.Width * y.Stock.Products.Height * y.Stock.TotalAvailable);
            totalInventoryMetert = totalInventoryCentit * MeterToFeet;


            //adds everything together
            total = totalInventoryMetert + totalInventoryCentit + totalInventoryFeett + totalInventoryIncht;

            return total;

        }
        catch (Exception)
        {
            throw;
        }
    }

    #region GetProducts
    public IQueryable<Product> GetProducts(
    string searchValue,
    bool? zeroQtyStock,
    int? subCategoryId,
    int? siteId,
    int? departmentId,
    int? productTagId,
    int? vendorId,
    string sortColumn,
    string sortColumnDirection,
    int? storeId
)
    {
        if (http.HttpContext.User.IsInRole(RoleList.ExternalUser) && storeId == null)
        {
            return Enumerable.Empty<Product>().AsQueryable();
        }

        List<int> productIdList = new List<int>();
        string normalizedUserName = http.HttpContext?.User.Identity?.Name?.ToUpperInvariant();
        if (vendorId != null)
        {
            var filteredList = _unitOfWork.ProductVendorMappings.GetListByFilter(pv => pv.isPrimaryVendor && pv.IsActive && pv.VendorId == vendorId.Value);
            if (filteredList != null)
            {
                var filteredProductIds = filteredList.Select(pv => pv.ProductId);
                if (filteredProductIds != null) { productIdList = filteredProductIds.ToList(); }
            }
        }
        var stockQuery = (IQueryable<Stock> stock) =>
        {

            if (http.HttpContext.User.IsInRole(RoleList.ExternalUser))
            {
                //TO DO: Will need to re-evaluate this at a later date for a better long term way of handling stock for External Users.
                //stock = stock.Where(x => x.IsExternal && (storeId == null || x.ShipStationStoreId == storeId));
                stock = stock.Where(x => x.ShipStationStoreId == storeId);
            }
            if (subCategoryId != null)
                stock = stock.Where(x => x.Products.SubCategoryId == subCategoryId);
            if (departmentId != null)
                stock = stock.Where(x => x.Products.Departments.Any(x => x.DepartmentId == departmentId));
            if (productTagId != null)
                stock = stock.Where(x => x.Products.ProductTags.Any(t => t.TagId == productTagId.Value));
            if (siteId != null)
                stock = stock.Where(x => x.Location.SiteId == siteId);
            if (productIdList.Any())
                stock = stock.Where(x => productIdList.Contains(x.ProductId));

            if (!String.IsNullOrEmpty(searchValue))
            {
                stock = stock.Where(
                    x =>
                        x.Products.Sku.Contains(searchValue)
                        || x.Products.Description.Contains(searchValue)
                        || x.Location.LocationName.Contains(searchValue)
                        || x.Location.Sites.SiteName.Contains(searchValue)
                        || x.Products.Departments.Any(z => z.DepartmentName.Contains(searchValue))
                        || x.Products.ProductTags.Any(t => t.Tag.Description.ToLower().Contains(searchValue))
                );
            }

            return stock
                .Include(s => s.Location)
                .Include(s => s.Products)
                .Include(s => s.ShipStationStore)
                .Include(s => s.Products.Departments)
                .Include(s => s.Products.ProductTags)
                .ThenInclude(x => x.Tag)
                .OrderBy(s => s.Products.Sku);
        };

        var filteredStock = _unitOfWork.Stocks.QueryFilter(stockQuery);
        IQueryable<QueryDataModel> locationTotals;

        var query = (IQueryable<Stock> Query) =>
        {

            if (http.HttpContext.User.IsInRole(RoleList.ExternalUser))
            {
                //Query = Query.Where(x => x.IsExternal && (storeId == null || x.ShipStationStoreId == storeId));
                Query = Query.Where(x => x.ShipStationStoreId == storeId);
            }

            return Query.Where(i => i.Location.Type != LocationType.ReceiveOnly && i.Products.IsActive)

                .Include(i => i.Products)
                .Include(i => i.Location)
                .GroupBy(i => i.Products.Sku)
                .Select(
                    x =>
                        new QueryDataModel
                        {
                            product = x.Key,
                            Total = x.Sum(i => i.TotalAvailable),
                            Description = x.First().Products.Description
                        }
                );
        };

        locationTotals = _unitOfWork.Stocks.QueryFilter(query);

        if (zeroQtyStock != true)
        {
            locationTotals = locationTotals.Where(x => x.Total > 0);
        }

        var productsQuery = (IQueryable<Product> product) => product.Where(
                s =>
                    (filteredStock.Any(o => o.Products.Sku.Equals(s.Sku))
                    || s.Sku.Contains(searchValue)) && s.IsActive
            )
            .Include(s => s.Departments)
            .Include(x => x.ProductTags)
            .ThenInclude(x => x.Tag);

        var stockProducts = _unitOfWork.Products.QueryFilter(productsQuery);

        IQueryable<QueryDataModel> locationStock; // initialized below, but needs higher scope here  

        if (http.HttpContext.User.IsInRole(RoleList.ExternalUser))
        {
            locationStock = filteredStock
                //.Where(y => y.IsExternal && (storeId == null || y.ShipStationStoreId == storeId))
                .Where(y => y.ShipStationStoreId == storeId)
                .Include(y => y.Location)
                .GroupBy(y => y.Products.Sku)
                .Select(
                    x =>
                        new QueryDataModel
                        {
                            product = x.Key,
                            Total = x.Sum(s => s.TotalAvailable),
                            Description = x.First().Products.Description
                        }
                );
        }
        else
        {
            locationStock = filteredStock
                .Include(y => y.Location)
                .GroupBy(y => y.Products.Sku)
                .Select(
                    x =>
                        new QueryDataModel
                        {
                            product = x.Key,
                            Total = x.Sum(i => i.TotalAvailable),
                            Description = x.First().Products.Description
                        }
                );
        }

        // joins the tables together to get total counts  
        stockProducts = stockProducts.Join(
            locationTotals,
            s => s.Sku,
            t => t.product,
            (s, t) =>
                new Product
                {
                    Sku = s.Sku,
                    StockTotalAvailable = t.Total,
                    Description = s.Description,
                    OnOrder = s.OnOrder,
                    ProductTags = s.ProductTags
                }
        );

        stockProducts = stockProducts.Join(
            locationStock,
            s => s.Sku,
            t => t.product,
            (s, t) =>
                new Product
                {
                    Sku = s.Sku,
                    StockTotalAvailable = s.StockTotalAvailable,
                    Description = s.Description,
                    StockTotalAvailableFilter = t.Total,
                    OnOrder = s.OnOrder,
                    ProductTags = s.ProductTags
                }
        );

        //sort direction
        if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
        {
            if (sortColumnDirection == "asc")
            {
                switch (sortColumn)
                {
                    case "Sku":
                        stockProducts = stockProducts.OrderBy(x => x.Sku);
                        break;
                    case "Description":
                        stockProducts = stockProducts.OrderBy(x => x.Description);
                        break;
                    case "StockTotalAvailable":
                        stockProducts = stockProducts.OrderBy(x => x.StockTotalAvailable);
                        break;
                    case "StockTotalAvailableFilter":
                        stockProducts = stockProducts.OrderBy(x => x.StockTotalAvailableFilter);
                        break;
                }
            }
            else if (sortColumnDirection == "desc")
            {
                switch (sortColumn)
                {
                    case "Sku":
                        stockProducts = stockProducts.OrderByDescending(x => x.Sku);
                        break;
                    case "Description":
                        stockProducts = stockProducts.OrderByDescending(x => x.Description);
                        break;
                    case "StockTotalAvailable":
                        stockProducts = stockProducts.OrderByDescending(x => x.StockTotalAvailable);
                        break;
                    case "StockTotalAvailableFilter":
                        stockProducts = stockProducts.OrderByDescending(x => x.StockTotalAvailableFilter);
                        break;
                }
            }
        }

        return stockProducts;
    }
    #endregion

    #region GetProductsStock
    public IQueryable<Stock> GetProductsStock(
        string searchValue,
        string sku,
        string sortColumn,
        string sortColumnDirection,
        string role,
        int? storeId
    )
    {

        var stockQuery = (IQueryable<Stock> stock) =>
        {

            if (http.HttpContext.User.IsInRole(RoleList.ExternalUser))
            {
                //stock = stock.Where(x =>x.IsExternal);
                stock = stock.Where(y => y.ShipStationStoreId == storeId);
            }
            else if (!(http.HttpContext.User.IsInRole(RoleList.Administrator) || http.HttpContext.User.IsInRole(RoleList.FinancialManager)))
            {
                stock = stock.Where(x => !x.IsExternal);
            }

            stock = stock.Where(x => x.Products.Sku.Equals(sku))
                .Include(x => x.Products)
                .Include(x => x.ShipStationStore)
                .Include(x => x.Products.Departments)
                .Include(x => x.Location)
                .Include(x => x.Location.Sites);

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
            {
                if (sortColumnDirection == "asc")
                {
                    switch (sortColumn)
                    {
                        case "Location.LocationName":
                            stock = stock.OrderBy(x => x.Location.LocationName);
                            break;
                        case "Location.Sites.SiteName":
                            stock = stock.OrderBy(x => x.Location.Sites.SiteName);
                            break;
                        case "TotalAvailable":
                            stock = stock.OrderBy(x => x.TotalAvailable);
                            break;
                        default:
                            stock = stock.OrderByDescending(x => x.TotalAvailable);
                            break;
                    }
                }
                else if (sortColumnDirection == "desc")
                {
                    switch (sortColumn)
                    {
                        case "Location.LocationName":
                            stock = stock.OrderByDescending(x => x.Location.LocationName);
                            break;
                        case "Location.Sites.SiteName":
                            stock = stock.OrderByDescending(x => x.Location.Sites.SiteName);
                            break;
                        case "TotalAvailable":
                            stock = stock.OrderByDescending(x => x.TotalAvailable);
                            break;
                        default:
                            stock = stock.OrderByDescending(x => x.TotalAvailable);
                            break;
                    }
                }
            }

            return stock;
        };

        var result = _unitOfWork.Stocks.QueryFilter(stockQuery);
        string normalizedUserName = http.HttpContext?.User.Identity?.Name?.ToUpperInvariant();
        if (http.HttpContext.User.IsInRole(RoleList.SellerBasic))
            result = result.Where(x => x.ShipStationStore.Email.Contains(normalizedUserName));
        return result;
    }
    #endregion

    #region DeleteConfirmed
    public async Task<bool> DeleteConfirmed(int id)
    {
        try
        {
            var stock = await _unitOfWork.Stocks.GetByIdAsync(id);

            var checkMe = await _unitOfWork.MoveStockHistories.GetStockHistoriesCustomSelectionAsync(id);

            // if there is any history attached to this stock, mark this stock as inactive. AND NOT DELETE IT :)
            if (checkMe.Any())
            {
                _unitOfWork.Stocks.Update(stock);
            }
            else
            {
                _unitOfWork.Stocks.Delete(stock.StockId);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (SqlException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
    #endregion

    public List<Report> StockHistoryReport_Old(int locationId, DateTime selectedDate)
    {
        return _unitOfWork.Stocks.StockHistoryReport_Old(locationId, selectedDate);
    }
    public List<Report> GetOnHandBySiteFilter(int SiteId)
    {
        try
        {
            return _unitOfWork.Stocks.GetOnHandReport(SiteId);
        }
        catch
        {
            throw;
        }
    }
    public List<Report> GetStockHistoryReport(int siteId, DateTime selectedDate)
    {
        return _unitOfWork.Stocks.GetStockHistoryReport(siteId, selectedDate);
    }

    public ReportMetaData GetStockHistoryReport(DateTime StartDate, DateTime EndDate, int ProductId, int SubCategoryId, int DepartmentId,int ShipStationStoreId, int PageNo, int PageSize)
    {
        try
        {
            SqlParameter[] parameters = new SqlParameter[]{
                new SqlParameter("@StartDate", SqlDbType.DateTime2) { Value = StartDate },
                new SqlParameter("@EndDate", SqlDbType.DateTime2) { Value = EndDate },
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = ProductId },
                new SqlParameter("@SubCategoryId", SqlDbType.Int) { Value = SubCategoryId },
                new SqlParameter("@DepartmentId", SqlDbType.Int) { Value = DepartmentId },
                new SqlParameter("@ShipStationStoreId", SqlDbType.Int) { Value = ShipStationStoreId },
                new SqlParameter("@PageIndex", SqlDbType.Int) { Value = PageNo },
                new SqlParameter("@PageLength", SqlDbType.Int) { Value = PageSize }
            };

            ReportMetaData report = _unitOfWork.Stocks.GetStockHistoryReport(
                "SpExtractStockTransactionLog",
                parameters,
                120
            );

            return report;
        }
        catch
        {
            throw;
        }
    }
}