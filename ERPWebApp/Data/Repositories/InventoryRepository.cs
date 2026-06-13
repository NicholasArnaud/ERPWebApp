using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ERPWebApp.Data.Repositories
{
    public class InventoryRepository : Repository<InventoryViewModel>, IInventoryRepository
    {
        public InventoryRepository(ApplicationDbContext context) : base(context)
        {

        }
        //So good news, is we don't need the orders table since everything we're nabbing is from the Inventory itself.
        public async Task<List<MovedProductsDto>> MovedProducts(int days)
        {
            DateTime startDate = DateTime.UtcNow.AddDays(days);

            var joinedData = await (from product in _context.Product
                                    join stock in _context.Stock on product.ProductId equals stock.ProductId
                                    join movement in _context.MoveStockHistory on product.Sku equals movement.Sku
                                    where movement.DateTime >= startDate
                                    group movement by new { product.Sku } into g
                                    orderby g.Count() descending
                                    select new MovedProductsDto
                                    {
                                        Sku = g.Key.Sku,
                                        Quantity = g.Count()  //We're counting the number of movements for each sku here. 
                                    })
                                    .Take(10)
                                    .ToListAsync();
            return joinedData;
        }

        public async Task<List<ProductCycleCountDto>> ProductCycleCount()
        {
            DateTime currentDate = DateTime.UtcNow;

            //Joining the Product, Stock, CycleCount, and Location tables.
            var query = await (from product in _context.Product
                               join stock in _context.Stock on product.ProductId equals stock.ProductId
                               join location in _context.Location on stock.LocationId equals location.LocationId
                               //Making sure that the stock is active and that the total available is not 0.
                               where stock.TotalAvailable != 0 && location.Type != LocationType.ReceiveOnly
                               select new
                               {
                                   Product = product,
                                   Stock = stock,
                                   Location = location,
                                   Site = location.Sites,
                                   ProductSku = product.Sku,
                                   SiteName = location.Sites.SiteName,
                                   //Get the difference between today's date and the LastCounted date.
                                   DaysSinceLastVerification = (currentDate - stock.LastCounted).Days
                               }).ToListAsync();

            //Filtering out entries that have been checked less than a day ago.
            var filteredQuery = query.Where(q => (currentDate - q.Stock.LastCounted).Days > 0).ToList();

            //Group the entries by SiteName as well as ProductSku. Might not need to have it grouped by ProductSku anymore though, so I might change this.
            var groupedBySiteAndProduct = filteredQuery.GroupBy(q => new { q.Site.SiteName, q.ProductSku });

            var result = new List<ProductCycleCountDto>();

            foreach (var group in groupedBySiteAndProduct)
            {
                var productCycleCountDto = new ProductCycleCountDto
                {
                    SiteName = group.Key.SiteName,
                    ProductSku = group.Key.ProductSku,
                    //Grabbing the sum of each entries TotalAvailable stock for the product.
                    StockQuantity = group.Sum(stockInfo => stockInfo.Stock.TotalAvailable),
                    DaysSinceLastVerification = group.First().DaysSinceLastVerification
                };
                result.Add(productCycleCountDto);
            }
            return result;
        }


        public async Task<List<RequestedReasonDto>> RequestedReason(int days)
        {
            DateTime startDate = DateTime.UtcNow.AddDays(days);

            var joinedData = await (from product in _context.Product
                                    from inventory in _context.InventoryRequestForm
                                    where inventory.ProductId == product.ProductId
                                    && inventory.CreatedDate >= startDate
                                    select new
                                    {
                                        PickReason = inventory.PickReason,
                                        CreateDate = inventory.CreatedDate
                                    }).ToListAsync();

            var totalCount = joinedData.Count;

            var top5PickReasons = joinedData.GroupBy(r => r.PickReason)
                                           .Select(g =>
                                           {
                                               var reasonCount = g.Count();
                                               return new RequestedReasonDto
                                               {
                                                   PickReason = g.Key,
                                                   ReasonCount = reasonCount,
                                                   Percentage = totalCount > 0 ? (reasonCount / (double)totalCount) * 100 : 0
                                               };
                                           })
                                           .OrderByDescending(x => x.ReasonCount)
                                           .Take(5)
                                           .ToList();

            return top5PickReasons;
        }


        public async Task<List<RequestedProductsDto>> RequestedProducts(int days)
        {
            DateTime startDate = DateTime.UtcNow.AddDays(days);

            var rawData = await (from product in _context.Product
                                 join inventory in _context.InventoryRequestForm
                                 on product.Sku equals inventory.Products.Sku
                                 where inventory.CreatedDate >= startDate
                                 select new { product.Sku, inventory.QuantityNeeded })
                                 .ToListAsync();

            var totalQuantityNeeded = rawData.Sum(x => x.QuantityNeeded);

            var joinedData = rawData.GroupBy(x => x.Sku)
                                    .Select(productGroup =>
                                    {
                                        var sumQuantityNeeded = productGroup.Sum(x => x.QuantityNeeded);
                                        return new RequestedProductsDto
                                        {
                                            ProductSku = productGroup.Key,
                                            QuantityNeeded = sumQuantityNeeded,
                                            Percentage = totalQuantityNeeded > 0 ? (sumQuantityNeeded / (double)totalQuantityNeeded) * 100 : 0
                                        };
                                    })
                                    .OrderByDescending(x => x.QuantityNeeded)
                                    .Take(5)
                                    .ToList();

            return joinedData;
        }

        public async Task<List<VolumetricsDto>> Volumetrics()
        {
            var volumetricsData = await (from site in _context.Site
                                         select new VolumetricsDto
                                         {
                                             SiteId = site.SiteId,
                                             SiteNames = site.SiteName,
                                             SiteVolume = site.SiteVolume
                                         }).ToListAsync();

            return volumetricsData;
        }

    }
}
