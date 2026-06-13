using ERPWebApp.Data;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class SpeedOMeterGoalService : Service<SpeedOMeterGoal>, ISpeedOMeterGoalService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SpeedOMeterGoalService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SpeedOMeterGoal> GetLastSpeedOMeterGoalAsync()
        {
            return await _unitOfWork.SpeedOMeterGoals.GetLastSpeedOMeterGoalAsync();
        }
    }
}