using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories
{
    public class ShipStationAwaitingOrderRepository : Repository<ShipStationAwaitingOrder>, IShipStationAwaitingOrderRepository
    {
        public ShipStationAwaitingOrderRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
