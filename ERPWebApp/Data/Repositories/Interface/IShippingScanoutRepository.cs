using ERPWebApp.Data.DTOModels.ShippingScanout;
using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IShippingScanoutRepository : IRepository<ShippingScanout>
    {
       public Task<IReadOnlyCollection<ShipmentsCountByCarrier>> GetOpenShipmentsCountByCarrierAsync();
       public Task<(List<string>, int)> GetScannedUspsTrackingNumbersAsync((string name, string state, string postalCode) shipFrom);
    }
}
