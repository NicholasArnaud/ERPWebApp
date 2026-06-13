using ERPWebApp.Models;

namespace ERPWebApp.Services.IServices
{
    public interface IInventoryBalanceService : IService<InventoryBalance>
    {
        List<Report> GetReport(int siteId);
    }
}
