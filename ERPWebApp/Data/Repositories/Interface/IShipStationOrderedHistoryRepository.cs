using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IShipStationOrderedHistoryRepository : IRepository<ShipStationOrderedHistory>
    {
        Task UpdateShipStationOrderedHistory();
        List<ShipStationOrderedHistory> GetShipStationOrderedHistory(int DepartmentId);
    }
}
