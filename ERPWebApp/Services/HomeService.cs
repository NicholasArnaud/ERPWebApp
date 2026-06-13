using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services;

public class HomeService(IUnitOfWork unitOfWork) : IHomeService
{
    public Task<List<TopDepartment>> TopDepartment(DateTime startDate, DateTime endDate)
        => unitOfWork.Orders.GetTopDepartmentsByShipment(startDate, endDate);

    public Task<List<TallyDto>> GetDailyOrderCompletionCount()
        => unitOfWork.Orders.GetDailyOrderCompletionCount();

    public Task<List<ShipstationOrderDto>> GetDailyShipstationOrdersAll(
        DateTime startDate,
        DateTime endDate,
        int? departmentId = null
    ) => unitOfWork.Orders.GetDailyShipstationOrdersAll(startDate, endDate, departmentId);
    public Task<List<OrderShipmentsByServiceDTO>> GetDaysShipmentsByServiceCode() 
        => unitOfWork.OrderShipments.GetDaysShipmentsByServiceCode();
}
