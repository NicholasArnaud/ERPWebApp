namespace ERPWebApp.Data.DTOModels
{
    public class UserPreferenceDTO
    {
        public string UserId { get; set; }
        public int PreferencesId { get; set; }
        public string Username { get; set; }
        public string PreferDashboard { get; set; }
        public string Theme { get; set; }
    }
}
