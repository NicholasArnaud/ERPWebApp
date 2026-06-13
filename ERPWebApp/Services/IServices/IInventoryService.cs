using ERPWebApp.Data.DTOModels;

namespace ERPWebApp.Services.IServices
{
    public interface IInventoryService
    {
        public Task<List<MovedProductsDto>> MovedProducts(int days);
        public Task<List<ProductCycleCountDto>> ProductCycleCount();
        public Task<List<RequestedProductsDto>> RequestedProducts(int days);
        public Task<List<RequestedReasonDto>> RequestedReason(int days);
        public Task<List<VolumetricsDto>> Volumetrics();
    }
}
