using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Data.Repositories
{
    public class SpeedOMeterGoalRepository : Repository<SpeedOMeterGoal>, ISpeedOMeterGoalRepository
    {
        public SpeedOMeterGoalRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<SpeedOMeterGoal> GetLastSpeedOMeterGoalAsync()
        {
            return await _context.SpeedOMeterGoal.OrderByDescending(x => x.SpeedOMeterGoalId).FirstOrDefaultAsync();
        }
    }
}