using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace ERPWebApp.Services
{
    public class CycleCountService(IUnitOfWork unitOfWork) : Service<CycleCount>(unitOfWork), ICycleCountService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public List<Report> GetCycleCountReport(DateTime startDate, DateTime endDate, int locationId)
        {
            return _unitOfWork.CycleCountes.GetCycleCountReport(startDate, endDate, locationId);
        }

        public async Task<Stock> EditCycleCount(CycleCount cycleCount, string verifiedBy)
        {
            Stock stock = null;

            try
            {

                var utcNow = DateTime.UtcNow;

                await _unitOfWork.BeginTransactionAsync();

                //mark the stock as counted by today
                stock = await _unitOfWork.Stocks.FilterOneAsync(x => x.StockId == cycleCount.StockId, includes:
                [
                    x => x.Products,
                    x => x.Location,
                    x => x.Location.Sites
                ]);

                //make quantity value to pun in move stock history
                int quantity = (cycleCount.EnteredQuantity ?? 0) - stock.TotalAvailable;

                stock.BeingCounted = false;
                stock.LastCounted = utcNow;
                stock.ModifyDate = utcNow;
                stock.ModifyByUser = verifiedBy;

                //set the counts
                cycleCount.ExpectedQuantity = stock.TotalAvailable;
                stock.TotalAvailable = (int)cycleCount.EnteredQuantity;

                //add entry to cycle count table
                cycleCount.EnteredSku = stock.Products.Sku;
                cycleCount.EnteredOn = utcNow;
                cycleCount.VerifiedBy = verifiedBy;
                cycleCount.VerifiedOn = utcNow;
                cycleCount.Finished = true;

                _unitOfWork.CycleCountes.Update(cycleCount);
                await _unitOfWork.SaveChangesAsync();

                _unitOfWork.Stocks.Update(stock);
                await _unitOfWork.SaveChangesAsync();

                //add to the history
                var history = new MoveStockHistory
                {
                    ToStock = stock,
                    Sku = stock.Products.Sku,
                    DateTime = utcNow,
                    Quantity = quantity,
                    Type = ActionType.CycleCount,
                    EmployeeName = verifiedBy
                };

                await _unitOfWork.MoveStockHistories.AddAsync(history);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return stock;
        }

        public async Task<(List<Stock>, int)> GetStockToCountAsync(int siteId, SearchParameters search, bool? isStarted)
        {
            var frequency = await _unitOfWork.CycleCountFrequencies.GetLatestFrequencyAsync(siteId);
            return await _unitOfWork.Stocks.GetStockToCountAsync(siteId, frequency, search, isStarted);
        }
        public Task<List<Stock>> GetStockToCountPagedAsync(int siteId, CycleCountFrequency frequency, int pageNumber, int pageSize)
        {
            var today = DateTime.Now;
            var count = 0;
            var query = (IQueryable<Stock> sk) =>
            {
                return sk.Where(
                    x => x.Location.SiteId == siteId
                        && x.Location.Type != LocationType.ReceiveOnly
                        && (
                            EF.Functions.DateDiffDay(x.LastCounted, today) > frequency.BaseDays && x.TotalAvailable > 0
                            || EF.Functions.DateDiffDay(x.LastCounted, today) > frequency.Over1000 && x.TotalAvailable > 1000
                            || x.Products.Cost > 10 && EF.Functions.DateDiffDay(x.LastCounted, today) > frequency.Cost10 && x.TotalAvailable > 0
                        )
                ).Include(x => x.Location)
                .Include(x => x.Products)
                .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize);

            };
            Expression<Func<Stock, bool>> queryCount = x =>
                           x.Location.SiteId == siteId
                        && x.Location.Type != LocationType.ReceiveOnly
                        && (EF.Functions.DateDiffDay(x.LastCounted, today) > frequency.BaseDays && x.TotalAvailable > 0
                           || EF.Functions.DateDiffDay(x.LastCounted, today) > frequency.Over1000 && x.TotalAvailable > 1000
                           || x.Products.Cost > 10 && EF.Functions.DateDiffDay(x.LastCounted, today) > frequency.Cost10 && x.TotalAvailable > 0
                         );

            count = _unitOfWork.Stocks.GetCount(queryCount);

            return _unitOfWork.Stocks.GetListByQueryAsync<Stock>(query);
        }

        public async Task StartCycleCountAsync(List<int> stockIds)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var cycleCounts = await _unitOfWork.CycleCountes.PrepareCycleCountAsync(stockIds);

                await _unitOfWork.CycleCountes.AddRangeAsync(cycleCounts);
                await _unitOfWork.SaveChangesAsync();

                var stocks = await _unitOfWork.Stocks.GetListByFilterAsync(x => stockIds.Contains(x.StockId));
                stocks.ForEach(x => x.BeingCounted = true);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task StartCycleCountAsync(int stockId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var cycleCount = await _unitOfWork.CycleCountes.PrepareCycleCountAsync(stockId);

                await _unitOfWork.CycleCountes.AddAsync(cycleCount);
                await _unitOfWork.SaveChangesAsync();

                var stock = await _unitOfWork.Stocks.GetByIdAsync(stockId);
                stock.BeingCounted = true;
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task StartCycleCountForSiteAsync(int siteId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var frequency = await _unitOfWork.CycleCountFrequencies.GetLatestFrequencyAsync(siteId);
                var cycleCounts = await _unitOfWork.CycleCountes.PrepareCycleCountForSiteAsync(siteId, frequency);

                await _unitOfWork.CycleCountes.AddRangeAsync(cycleCounts);
                await _unitOfWork.SaveChangesAsync();

                var stockIds = cycleCounts.Select(x => x.StockId).ToList();
                var stocks = await _unitOfWork.Stocks.GetListByFilterAsync(x => stockIds.Contains(x.StockId));
                stocks.ForEach(x => x.BeingCounted = true);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}