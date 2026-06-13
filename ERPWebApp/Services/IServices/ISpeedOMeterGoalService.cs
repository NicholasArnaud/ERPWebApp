using ERPWebApp.Models;

namespace ERPWebApp.Services.IServices
{
    public interface ISpeedOMeterGoalService : IService<SpeedOMeterGoal>
    {
        Task<SpeedOMeterGoal> GetLastSpeedOMeterGoalAsync();
    }
}