using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Inventory;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class InventoryRequestFormService : Service<InventoryRequestForm>, IInventoryRequestFormService
    {
        private readonly IUnitOfWork _unitOfWork;
        public InventoryRequestFormService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CloseInventoryRequestAsync(
            InventoryRequestForm entity,
            Stock fromStock,
            Stock toStock,
            MoveStockHistory stockHistory
        )
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                _unitOfWork.InventoryRequestForms.Update(entity);
                await _unitOfWork.SaveChangesAsync();

                _unitOfWork.Stocks.Update(fromStock);
                await _unitOfWork.SaveChangesAsync();


                _unitOfWork.Stocks.Update(toStock);
                await _unitOfWork.SaveChangesAsync();

                stockHistory.ToStock = toStock;
                stockHistory.FromStock = fromStock;

                await _unitOfWork.MoveStockHistories.AddAsync(stockHistory);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}