using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface IInventoryBalanceRepository
    {
        List<Report> GetReport(int ProductId);
    }
}
