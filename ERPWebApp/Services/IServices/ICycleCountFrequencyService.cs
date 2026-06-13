using ERPWebApp.Models;
namespace ERPWebApp.Services.IServices
{
    public interface ICycleCountFrequencyService : IService<CycleCountFrequency>
    {
        Task<CycleCountFrequency> GetLatestFrequencyAsync(int siteId);
        Task GenerateFrequenciesAsync();
    }
}