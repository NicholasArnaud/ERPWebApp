using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<List<Product>> GetNonProductionProducts();
    }
}