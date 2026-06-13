using ERPWebApp.Models;
namespace ERPWebApp.Data.Repositories.Interface
{
    public interface ICycleCountRepository : IRepository<CycleCount>
    {
        List<Report> GetCycleCountReport(DateTime startDate, DateTime endDate, int locationId);
        Task<List<CycleCount>> PrepareCycleCountAsync(List<int> stockIds);
        Task<CycleCount> PrepareCycleCountAsync(int stockId);
        Task<List<CycleCount>> PrepareCycleCountForSiteAsync(int siteId, CycleCountFrequency frequency);
    }
}