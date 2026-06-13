using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class ShipStationAwaitingOrderServices : Service<ShipStationAwaitingOrder>, IShipStationAwaitingOrderServices
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShipStationAwaitingOrderServices(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
