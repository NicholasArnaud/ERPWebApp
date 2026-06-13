using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.NirfForms;
namespace ERPWebApp.Services.IServices
{
    public interface INirfProductMappingService : IService<NirfProductMapping>
    {
        Task<List<Product>> GetVariantProducts(int nirfFormId);
    }
}