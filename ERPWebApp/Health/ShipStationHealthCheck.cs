using ERPWebApp.Data;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Services.IServices;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ERPWebApp.Health;

public class ShipStationHealthCheck(IShipStationStoreService shipstationStoreService) : IHealthCheck
{
    private readonly IShipStationStoreService _shipStationStoreService = shipstationStoreService;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var shipstationStores = await _shipStationStoreService.GetShipStationStores();
            return HealthCheckResult.Healthy("ShipStation is healthy");
        }
        catch(Exception ex)
        {
            return HealthCheckResult.Unhealthy("ShipStation or Connection Issue", ex);
        }
    }
}
