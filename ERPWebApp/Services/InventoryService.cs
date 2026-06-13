using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class InventoryService : IInventoryService
    {
        IUnitOfWork _unitOfWork;
        public InventoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<MovedProductsDto>> MovedProducts(int days)
        {
            var inventoryInformation = await _unitOfWork.Inventory.MovedProducts(days);
            return inventoryInformation;
        }
        public async Task<List<ProductCycleCountDto>> ProductCycleCount()
        {
            var inventoryInformation = await _unitOfWork.Inventory.ProductCycleCount();
            return inventoryInformation;
        }
        public async Task<List<RequestedProductsDto>> RequestedProducts(int days)
        {
            var inventoryInformation = await _unitOfWork.Inventory.RequestedProducts(days);
            return inventoryInformation;
        }
        public async Task<List<RequestedReasonDto>> RequestedReason(int days)
        {
            var inventoryInformation = await _unitOfWork.Inventory.RequestedReason(days);
            return inventoryInformation;
        }
        public async Task<List<VolumetricsDto>> Volumetrics()
        {
            var inventoryInformation = await _unitOfWork.Inventory.Volumetrics();
            return inventoryInformation;
        }
    }
}