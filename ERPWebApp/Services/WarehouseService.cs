using System.Text.Json;
using System.Text.Json.Nodes;

using ERPWebApp.Data.DTOModels.ShipEngineDtos;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services;

public class WarehouseService : Service<Warehouse> , IWarehouseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory;

    public WarehouseService(IHttpClientFactory httpClientFactory,IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _httpClientFactory = httpClientFactory;
        _unitOfWork = unitOfWork;
    }
    public async Task<IEnumerable<ShipEngineWarehouse>> FetchWarehouses()
    {
        using HttpClient client = _httpClientFactory.CreateClient("ShipEngineV1");
        var response = await client.GetFromJsonAsync<WarehousesRoot>("warehouses");
        return response.Warehouses;
    }
}
