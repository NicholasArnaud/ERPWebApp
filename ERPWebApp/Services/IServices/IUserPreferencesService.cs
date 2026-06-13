using ERPWebApp.Models.Company;

namespace ERPWebApp.Services.IServices
{
    public interface IUserPreferencesService : IService<UserPreferences>
    {
        public Task<UserPreferences> UpdateDashboardConfigAsync(string userId, DashboardConfig config);

        public Task<UserPreferences> GetPreferencesByUserIdAsync(string userId);

        public Task<List<DashboardLayout>> GetDashboardLayoutByDashboardAsync(string userId, string dasboardName);
    }
}