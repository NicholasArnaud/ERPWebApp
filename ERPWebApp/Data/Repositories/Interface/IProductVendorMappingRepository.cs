using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Mappings;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IProductVendorMappingRepository : IRepository<ProductVendorMapping>
    {
        Task<IEnumerable<Vendor>> GetVendorsAsync();
    }
}