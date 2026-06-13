using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Company;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class UserPreferencesService : Service<UserPreferences>, IUserPreferencesService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserPreferencesService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserPreferences> UpdateDashboardConfigAsync(string userId, DashboardConfig config)
        {
            try
            {
                UserPreferences userPreference = null;

                var preferences = await _unitOfWork.UserPreferences.FindAsync(e => e.UserId == userId);
                if (preferences is null || preferences.FirstOrDefault() is null)
                {
                    userPreference = new UserPreferences
                    {
                        UserId = userId,
                        DashboardConfigList = new List<DashboardConfig> { config }
                    };

                    await _unitOfWork.UserPreferences.AddAsync(userPreference);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    userPreference = preferences.FirstOrDefault();

                    var configList = userPreference.DashboardConfigList;
                    if (configList is null)
                    {
                        configList = new List<DashboardConfig>();
                    }
                    else if (configList.Where(c => c.Name == config.Name).Any())//if the dashboard object exists, remove it before add the new layout
                    {
                        configList.Remove(configList.Where(c => c.Name == config.Name).FirstOrDefault());
                    }

                    //add new layout
                    configList.Add(config);
                    userPreference.DashboardConfigList = configList;

                    _unitOfWork.UserPreferences.Update(userPreference);
                    await _unitOfWork.SaveChangesAsync();
                }

                return userPreference;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<UserPreferences> GetPreferencesByUserIdAsync(string userId)
        {
            var preferences = await _unitOfWork.UserPreferences.FindAsync(p =>  p.UserId == userId);

            return preferences is not null && preferences.Any() ? preferences.FirstOrDefault() : null;
        }

        public async Task<List<DashboardLayout>> GetDashboardLayoutByDashboardAsync(string userId, string dasboardName)
        {
            var preference = await GetPreferencesByUserIdAsync(userId);
            if (preference is null || preference.DashboardConfigList is null || !preference.DashboardConfigList.Any()
                || !preference.DashboardConfigList.Where(d => d.Name == dasboardName).Any())
            {
                return null;
            }

            return preference.DashboardConfigList.Where(d => d.Name == dasboardName).FirstOrDefault().Layouts;
        }
    }
}