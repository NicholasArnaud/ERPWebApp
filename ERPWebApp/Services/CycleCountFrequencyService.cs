using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class CycleCountFrequencyService : Service<CycleCountFrequency>, ICycleCountFrequencyService
    {
        IUnitOfWork _unitOfWork;
        public CycleCountFrequencyService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task GenerateFrequenciesAsync()
        {
            await _unitOfWork.CycleCountFrequencies.GenerateFrequenciesAsync();
            await _unitOfWork.SaveChangesAsync();
        }

        public Task<CycleCountFrequency> GetLatestFrequencyAsync(int siteId)
        {
            return _unitOfWork.CycleCountFrequencies.GetLatestFrequencyAsync(siteId);
        }
    }
}