using ERPWebApp.Data.DTOModels.StockDto;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class MoveStockHistoryService(IUnitOfWork unitOfWork)
        : Service<MoveStockHistory>(unitOfWork), IMoveStockHistoryService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<MoveStock> MoveStock(MoveStock moveStock)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var utcNow = DateTime.UtcNow;                

                //find the original stock id .
                var fromStock = await _unitOfWork.Stocks.FilterOneAsync(
                     x => x.LocationId == moveStock.FromStock.LocationId
                        && x.ProductId == moveStock.FromStock.ProductId,
                    includes: [x => x.Location,x => x.Location.Sites]
                );

                // check to make sure the move wont put the stock into the negatives.
                if (fromStock.TotalAvailable - moveStock.quantity < 0)
                {
                    var message = "An ERROR has occurred. Total Quantity would be less than 0 for ProductId: ";
                    message += $"{fromStock.ProductId}, From LocationId: {fromStock.LocationId}, To LocationId: {moveStock.ToStock.LocationId}";
                    throw new Exception(message);
                }

                // find the new stock id.
                var toStock = await _unitOfWork.Stocks.FilterOneAsync(
                    x => x.LocationId == moveStock.ToStock.LocationId
                        && x.ProductId == fromStock.ProductId,
                    includes: [x => x.Location,x => x.Location.Sites]
                );

                if (toStock == null)
                {
                    var location = await _unitOfWork.Locations.FilterOneAsync(b => b.LocationId == moveStock.ToStock.LocationId);
                    toStock = new Stock
                    {
                        ProductId = fromStock.ProductId,
                        LocationId = moveStock.ToStock.LocationId,
                        Location = location,
                        ModifyByUser = moveStock.ModifiedBy,
                        ModifyDate = utcNow
                    };
                }

                if (!toStock.Location.IsActive) throw new Exception("Unable to move stock to inactive locations!");
                if (toStock.Location.Sites != null)
                    if (!toStock.Location.Sites.IsActive) throw new Exception("Unable to move stock to inactive sites!");


                var checkForCounting = await _unitOfWork.Stocks.IsExistsAsync(
                    x => x.BeingCounted && x.Location.SiteId == fromStock.Location.SiteId
                        || x.BeingCounted && x.Location.SiteId == toStock.Location.SiteId
                );

                if (checkForCounting) throw new Exception("There is currently a count ongoing in this location!");

                var product = await _unitOfWork.Products.FilterOneAsync(x => x.ProductId == fromStock.ProductId);

                moveStock.DateTime = utcNow;

                // remove the quantity from the original stock.
                fromStock.TotalAvailable -= moveStock.quantity;
                fromStock.ModifyByUser = moveStock.ModifiedBy;
                fromStock.ModifyDate = utcNow;


                toStock.TotalAvailable += moveStock.quantity;
                toStock.ModifyByUser = moveStock.ModifiedBy;
                toStock.ModifyDate = utcNow;

                _unitOfWork.Stocks.Update(toStock);

                _unitOfWork.Stocks.Update(fromStock);
                //add to the history
                var history = new MoveStockHistory
                {
                    FromStock = fromStock,
                    ToStock = toStock,
                    Sku = product.Sku,
                    DateTime = utcNow,
                    Quantity = moveStock.quantity,
                    Type = ActionType.Transfer,
                    EmployeeName = moveStock.ModifiedBy
                };

                await _unitOfWork.MoveStockHistories.AddAsync(history);
                await _unitOfWork.SaveChangesAsync();

                fromStock.Products = product;
                moveStock.FromStock = fromStock;
                moveStock.ToStock = toStock;

                await _unitOfWork.CommitAsync();

                return moveStock;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<MoveStock> AddStock(MoveStock addStock)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var utcNow = DateTime.UtcNow;

                // find the new stock id.
                var toStock = await _unitOfWork.Stocks.FilterOneAsync(
                    x => x.LocationId == addStock.ToStock.LocationId
                        && x.ProductId == addStock.FromStock.ProductId,
                    [
                        x => x.Products,
                        x => x.Location,
                        x => x.Location.Sites
                    ]
                );

                if (toStock == null)
                {
                    //if theres not an existing stock in that location then create a new one.
                    toStock = new Stock
                    {
                        ProductId = addStock.FromStock.ProductId,
                        LocationId = addStock.ToStock.LocationId,
                        Location = await _unitOfWork.Locations.FilterOneAsync(
                            x => x.LocationId == addStock.ToStock.LocationId,
                            [x => x.Sites]
                        ),
                        Products = await _unitOfWork.Products.FilterOneAsync(x => x.ProductId == addStock.FromStock.ProductId),
                    };
                }

                if (!toStock.Location.IsActive) throw new Exception("Unable to add stock to inactive locations!");

                if (!toStock.Location.Sites.IsActive) throw new Exception("Unable to add stock to inactive sites!");

                var checkForCounting = await _unitOfWork.Stocks.IsExistsAsync(x => x.BeingCounted && x.Location.SiteId == toStock.Location.SiteId);

                if (checkForCounting) throw new Exception("There is currently a count ongoing in this location!");

                toStock.TotalAvailable += addStock.quantity;
                toStock.ModifyByUser = addStock.ModifiedBy;
                toStock.ModifyDate = utcNow;

                _unitOfWork.Stocks.Update(toStock);

                //add to the history
                var history = new MoveStockHistory
                {
                    ToStock = toStock,
                    Sku = toStock.Products.Sku,
                    DateTime = utcNow,
                    Quantity = addStock.quantity,
                    Type = ActionType.Add,
                    EmployeeName = addStock.ModifiedBy
                };

                await _unitOfWork.MoveStockHistories.AddAsync(history);
                await _unitOfWork.SaveChangesAsync();

                addStock.ToStock = toStock;

                await _unitOfWork.CommitAsync();

                return addStock;

            }
            catch (Exception)
            {
               await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<MoveStock> RemoveStock(MoveStock moveStock)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var utcNow = DateTime.UtcNow;

                //find the original stock id .
                var fromStock = await _unitOfWork.Stocks.FilterOneAsync(
                    x => x.LocationId == moveStock.FromStock.LocationId
                        && x.ProductId == moveStock.FromStock.ProductId,
                    [
                        x => x.Products,
                        x => x.Location,
                        x => x.Location.Sites
                    ]
                );

                if (fromStock.TotalAvailable - moveStock.quantity < 0)
                {
                    var message = "An ERROR has occurred. Total Quantity would be less than 0 for ProductId: ";
                    message += $"{moveStock.FromStock.ProductId}, From LocationId: {moveStock.FromStock.LocationId}";
                    throw new Exception(message);
                }

                var checkForCounting = await _unitOfWork.Stocks.IsExistsAsync(x => x.BeingCounted && x.Location.SiteId == fromStock.Location.SiteId);

                if (checkForCounting) throw new Exception("There is currently a count ongoing in this location!");

                // remove the quantity from the stock.
                fromStock.TotalAvailable -= moveStock.quantity;
                fromStock.ModifyByUser = moveStock.ModifiedBy;
                fromStock.ModifyDate = utcNow;

                _unitOfWork.Stocks.Update(fromStock);

                //add to the history
                var history = new MoveStockHistory
                {
                    FromStock = fromStock,
                    Sku = fromStock.Products.Sku,
                    DateTime = utcNow,
                    Quantity = moveStock.quantity,
                    Type = ActionType.Remove,
                    EmployeeName = moveStock.ModifiedBy
                };

                await _unitOfWork.MoveStockHistories.AddAsync(history);
                await _unitOfWork.SaveChangesAsync();

                moveStock.FromStock = fromStock;

                await _unitOfWork.CommitAsync();

                return moveStock;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public Task<(IEnumerable<StockMovementHistory>, int)> GetStockMovementHistoryAsync(SearchParameters search, bool? isExternal, string sku)
            => _unitOfWork.MoveStockHistories.GetStockMovementHistoryAsync(search, isExternal, sku);
    }
}