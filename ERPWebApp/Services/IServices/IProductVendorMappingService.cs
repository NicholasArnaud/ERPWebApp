using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;

namespace ERPWebApp.Services.IServices
{
    public interface IProductVendorMappingService : IService<ProductVendorMapping>
    {
        Task<IEnumerable<Vendor>> GetVendorsAsync();
    }
}