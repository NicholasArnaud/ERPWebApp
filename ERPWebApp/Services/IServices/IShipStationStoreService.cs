using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models;
using ERPWebApp.Models.Mappings;
using Newtonsoft.Json.Linq;

namespace ERPWebApp.Services.IServices
{
    public interface IShipStationStoreService :IService<ShipStationStore>
    {
        JObject GetShipStationStorePieChartsData();
        Task<ShipStationStore> GetShipStationStoreByEmailAsync(string email);
        Task<List<ShipStationStore>> GetAllOrderedByNameAsync();
        Task<ShipStationStore> GetFirstOrderedByNameAsync();
        Task<ShipStationStoreFile> GetStoreFileAsync(int storeFileId);
        Task DeleteStoreFile(ShipStationStoreFile storeFile);
        Task<List<string>> VerifyShipStationStores(IEnumerable<ShipStationJson> shipStations);
        Task<IEnumerable<ShipStationJson>> GetShipStationStores();
    }
}
