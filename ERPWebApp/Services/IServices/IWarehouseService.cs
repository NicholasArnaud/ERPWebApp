using ERPWebApp.Data.DTOModels.ShipEngineDtos;
using ERPWebApp.Models;

namespace ERPWebApp.Services.IServices;

public interface IWarehouseService : IService<Warehouse>
{
    Task<IEnumerable<ShipEngineWarehouse>> FetchWarehouses();
}
