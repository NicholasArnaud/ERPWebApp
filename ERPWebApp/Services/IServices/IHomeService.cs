using ERPWebApp.Data.DTOModels;

namespace ERPWebApp.Services.IServices;

public interface IHomeService
{
    public Task<List<TopDepartment>> TopDepartment(DateTime startDate, DateTime endDate);
    public Task<List<TallyDto>> GetDailyOrderCompletionCount();
    public Task<List<ShipstationOrderDto>> GetDailyShipstationOrdersAll(DateTime startDate, DateTime endDate, int? departmentId = null);
    public Task<List<OrderShipmentsByServiceDTO>> GetDaysShipmentsByServiceCode();
}
