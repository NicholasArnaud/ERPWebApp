using ERPWebApp.Models;
using ERPWebApp.Models.Mappings;
using Newtonsoft.Json.Linq;
namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IShipStationStoreRepository : IRepository<ShipStationStore>
    {
        Task<string> GetStoreNameById(long storeId);
        JObject GetShipStationStorePieChartsData(DateTime date);
        Task<ShipStationStore> GetShipStationStoreByEmailAsync(string email);
        Task<List<ShipStationStore>> GetAllOrderedByNameAsync();
        Task<ShipStationStore> GetFirstOrderedByNameAsync();
        Task AddStoreFileAsync(ShipStationStoreFile file);
        Task<ShipStationStoreFile> GetStoreFileAsync(int storeFileId);
        Task DeleteStoreFileAsync(int storeFileId);
    }
}
