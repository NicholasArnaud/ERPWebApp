using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Extensions;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ERPWebApp.Data.Repositories
{
    public class StockRepository : Repository<Stock>, IStockRepository
    {
        public StockRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IQueryable<StockProductContainer> GetStockProductContainersBySiteId(int siteId)
        {
            return _context.Stock.Include(l => l.Location)
                    .Include(l => l.Location.Sites)
                    .Where(l => l.Location.SiteId == siteId)
                    .GroupJoin(
                        _context.ProductContainer
                            .Include(s => s.ProductVendorMappings),
                            s => s.ProductId,
                            t => t.ProductVendorMappings.ProductId,
                            (s, t) => new { Stock = s, ProductContainer = t }
                    )
                    .SelectMany(
                        s => s.ProductContainer.DefaultIfEmpty(),
                        (s, t) => new StockProductContainer { Stock = s.Stock, ProductContainer = t }
                    );
        }

        public List<Report> StockHistoryReport_Old(int locationId, DateTime selectedDate)
        {
            var result = from s in _context.Stock.TemporalAll()
                            .Where(x => EF.Property<DateTime>(x, "PeriodStart") <= selectedDate && selectedDate <= EF.Property<DateTime>(x, "PeriodEnd"))
                         join c in _context.CycleCount on s.StockId equals c.StockId
                         join p in _context.Product on s.ProductId equals p.ProductId
                         join l in _context.Location on s.LocationId equals l.LocationId
                         where (locationId <= 0 || l.LocationId == locationId)
                         select new Report
                         {
                             Date = s.ModifyDate.ToString("MM/dd/yyyy"),
                             User = s.ModifyByUser,
                             Sku = p.Sku,
                             Description = p.Description,
                             Location = l.LocationName,
                             TotalAvailable = s.TotalAvailable
                         };

            return result.ToList();
        }

        public List<Report> GetOnHandReport(int siteId)
        {
            var result = from stock in _context.Stock
                         join product in _context.Product on stock.ProductId equals product.ProductId
                         join location in _context.Location on stock.LocationId equals location.LocationId
                         join site in _context.Site on location.SiteId equals site.SiteId
                         where site.SiteId == (siteId > 0 ? siteId : site.SiteId)
                         group new { stock, product, location, site } by new
                         {
                             product.Sku,
                             product.Description,
                             site.SiteName,
                             location.LocationName,
                             product.Cost
                         } into groupedData
                         select new Report
                         {
                             Sku = groupedData.Key.Sku,
                             Description = groupedData.Key.Description,
                             SiteName = groupedData.Key.SiteName,
                             LocationName = groupedData.Key.LocationName,
                             OnHand = groupedData.Sum(x => x.stock.TotalAvailable),
                             TotalCost = groupedData.Sum(x => x.stock.TotalAvailable) * groupedData.Key.Cost
                         };
            return result.ToList();
        }

        public List<Report> GetStockHistoryReport(int siteId, DateTime selectedDate)
        {
            var query = from s in _context.Stock.TemporalAll()
                            .Where(x => EF.Property<DateTime>(x, "PeriodStart") <= selectedDate && selectedDate <= EF.Property<DateTime>(x, "PeriodEnd"))
                        join l in _context.Location on s.LocationId equals l.LocationId
                        join p in _context.Product on s.ProductId equals p.ProductId
                        join ss in _context.Site on l.SiteId equals ss.SiteId
                        join pvm in _context.ProductVendorMapping
                            on p.ProductId equals pvm.ProductId into pvmGroup
                        from pvm in pvmGroup.Where(x => x.isPrimaryVendor).DefaultIfEmpty()
                        join v in _context.Vendor on pvm.VendorId equals v.VendorId into vGroup
                        from v in vGroup.DefaultIfEmpty()
                        where siteId <= 0 || l.SiteId == siteId
                        select new Report
                        {
                            Sku = p.Sku,
                            Description = p.Description,
                            TotalAvailable = s.TotalAvailable,
                            TotalCost = p.Cost * s.TotalAvailable,
                            LocationName = l.LocationName,
                            SiteName = ss.SiteName,
                            MaxInventoryAmount = p.MaxInventory,
                            PrimaryVendorName = v != null ? v.VendorName : "N/A"
                        };

            return query.ToList();
        }

        public ReportMetaData GetStockHistoryReport(string procedure, SqlParameter[] parameters, int timeout = 0)
        {
            var conn = _context.Database.GetDbConnection();
            try
            {
                conn.Open();

                var reader = ExecuteStoredProcedure(conn, procedure, timeout, parameters);

                int totalRecords = 0;
                string jsonResult = "";
                if (reader != null && reader.HasRows)
                {
                    while (reader.Read())
                    {
                        totalRecords = reader.GetInt32(0);
                    }

                    reader.NextResult();
                    while (reader.Read())
                    {
                        jsonResult += reader.GetString(0);
                    }

                }

                List<Report> results = new List<Report>();
                if (!string.IsNullOrEmpty(jsonResult))
                {
                    results = JsonConvert.DeserializeObject<List<Report>>(jsonResult);
                }

                reader.Close();
                return new ReportMetaData { TotalRecords = totalRecords, ReportItemsList = results };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                conn.Close();
            }
        }
        public async Task<List<Stock>> GetAllStocksWithProductAndLocationAsync()
        {
            return await _context.Stock
                .Include(s => s.Products)
                .Include(s => s.Location)
                .ToListAsync();
        }

        public async Task<(List<Stock>, int)> GetStockToCountAsync(int siteId, CycleCountFrequency frequency, SearchParameters search, bool? isStarted)
        {
            var today = DateTime.Now;

            var query = _context.Stock.Where(
                 x => x.Location.SiteId == siteId
                 && x.Location.Type != LocationType.ReceiveOnly
                 && (
                     EF.Functions.DateDiffDay(x.LastCounted, today) > frequency.BaseDays && x.TotalAvailable > 0
                     || EF.Functions.DateDiffDay(x.LastCounted, today) > frequency.Over1000 && x.TotalAvailable > 1000
                     || x.Products.Cost > 10 && EF.Functions.DateDiffDay(x.LastCounted, today) > frequency.Cost10 && x.TotalAvailable > 0
                 )
             ).WhereIf(isStarted!=null, x=> x.BeingCounted == isStarted)
             .Include(x => x.Products)
             .Include(x => x.Location)
             .SmartSearch(search.SearchColumns, search.SearchValue);

            var rersults = await query.SmartSort(search.SortBy, search.IsDescending)
            .SmartPaging(search.Start, search.PageSize)
            .ToListAsync();

            return (rersults, await query.CountAsync());
        }
    }
}