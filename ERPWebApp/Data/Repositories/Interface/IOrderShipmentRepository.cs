using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Orders;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IOrderShipmentRepository : IRepository<OrderShipment>
    {
        IQueryable<DepartmentShippedTotalDTO> GetAllDepartmentShippedTotals();
        Task<List<DepartmentShippedTotalByDateDTO>> GetAllDepartmentShippedTotalsByDateAsync();
        Task<List<DepartmentShippedTotalByDateDTO>> GetAllDepartmentShippedTotalsInRangeAsync(DateTime startDate,DateTime endDate);
        Task<List<OrderShipmentsByServiceDTO>> GetDaysShipmentsByServiceCode();
    }
}