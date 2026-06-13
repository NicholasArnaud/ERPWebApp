using ERPWebApp.Data.DTOModels.ShipEngineDtos;
using ERPWebApp.Data.DTOModels.ShippingScanout;
using ERPWebApp.Data.DTOModels.ShippingScanout.USPS;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Shipping;

namespace ERPWebApp.Services.IServices;

public interface IShippingScanoutService : IService<ShippingScanout>
{
    Task<int> OnBulkUpdateScanouts(List<ShippingScanout> shippingScanouts);
    Task<int> SendUPSListAddOrRemoveFromUloRequest(List<ShippingScanout> shippingScanoutList, string trailerNumber);
    Task<ShippingScanout> CreateNewShippingScanout(ShippingScanout shippingScanout);
    Task<IReadOnlyCollection<ShipmentsCountByCarrier>> GetOpenShipmentsCountByCarrierAsync();
    Task<(IReadOnlyCollection<ShippingManifest>, int)> GetShippingManifestsAsync(
        string carrierId,
        string warehouseId,
        DateTime? shipDate,
        SearchParameters search
    );

    Task SaveShippingManifestsAsync(List<ShippingManifest> manifests);
    Task<(List<string>, int)> GetScannedUspsTrackingNumbersAsync((string name, string state, string postalCode) shipFrom);
    Task<List<ShipEngineCarriers>> FetchCarriers<T>();
    Task<UspsManifestResponse> GenerateUspsManifest(ShipEngineWarehouse warehouse, List<string> validTrackingNumbers = null, int validShipmentsCount = 0);
    Task<Dictionary<string, string>> ParseUSPSResponseAsync(HttpResponseMessage response);
}
