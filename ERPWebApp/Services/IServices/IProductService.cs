using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Services.IServices
{
    public interface IProductService : IService<Product>
    {
        Task<List<Product>> GetProductsByMinInventoryAsync(int minInventory);
        Task<List<Product>> GetNonProductionProducts();
    }
}