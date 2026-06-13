using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IInventoryRepository : IRepository<InventoryViewModel>
    {
        Task<List<MovedProductsDto>> MovedProducts(int days);
        Task<List<ProductCycleCountDto>> ProductCycleCount();
        Task<List<RequestedProductsDto>> RequestedProducts(int days);
        Task<List<RequestedReasonDto>> RequestedReason(int days);
        Task<List<VolumetricsDto>> Volumetrics();
    }
}
