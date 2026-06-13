using ERPWebApp.Models;

namespace ERPWebApp.Data.Repositories.Interface
{
    public interface ISpeedOMeterGoalRepository : IRepository<SpeedOMeterGoal>
    {
        Task<SpeedOMeterGoal> GetLastSpeedOMeterGoalAsync();
    }
}