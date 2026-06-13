using ERPWebApp.Models;

namespace ERPWebApp.Services.IServices
{
    public interface IShipStationOrderedHistoryService:IService<ShipStationOrderedHistory>
    {
        Task UpdateShipStationOrderedHistory();
        List<ShipStationOrderedHistory> GetShipStationOrderedHistory(int DepartmentId);
    }
}
