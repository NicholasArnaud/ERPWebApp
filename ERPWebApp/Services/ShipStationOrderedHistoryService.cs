using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class ShipStationOrderedHistoryService : Service<ShipStationOrderedHistory>, IShipStationOrderedHistoryService
    {
        IUnitOfWork _unitOfWork;
        public ShipStationOrderedHistoryService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task UpdateShipStationOrderedHistory()
        {
            return _unitOfWork.ShipStationOrderedHistories.UpdateShipStationOrderedHistory();
        }
        public List<ShipStationOrderedHistory> GetShipStationOrderedHistory(int DepartmentId)
        {
            return  _unitOfWork.ShipStationOrderedHistories.GetShipStationOrderedHistory(DepartmentId);
        }
    }
}
