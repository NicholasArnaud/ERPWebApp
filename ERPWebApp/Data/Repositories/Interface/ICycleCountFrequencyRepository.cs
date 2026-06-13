using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface ICycleCountFrequencyRepository : IRepository<CycleCountFrequency>
    {
        Task GenerateFrequenciesAsync();
        Task<CycleCountFrequency> GetLatestFrequencyAsync(int siteId);
    }
}