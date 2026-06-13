using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ERPWebApp.Models.Company
{
    public class UserPreferences
    {
        [Key]
        public int PreferencesId { get; set; }
        [MaxLength(70)]
        public string UserId { get; set; }
        [MaxLength(50)]
        public string PreferDashboard { get; set; }
        [MaxLength(10)]
        public string Theme { get; set; }
        public int? PreferDepartment { get; set; }
        [ForeignKey("PreferDepartment")]
        public virtual Department Department { get; set; }

        [DataType(DataType.Text)]
        public string DashboardConfig { get; set; }

        [NotMapped]
        public List<DashboardConfig> DashboardConfigList
        {
            get => string.IsNullOrEmpty(DashboardConfig) ? new List<DashboardConfig>() : JsonSerializer.Deserialize<List<DashboardConfig>>(DashboardConfig);
            set => DashboardConfig = JsonSerializer.Serialize(value);
        }
    }

    public class DashboardConfig
    {
        public string Name { get; set; }
        public List<DashboardLayout> Layouts { get; set; }
    }

    public class DashboardLayout
    {
        public string ElemId { get; set; }
        public int Position { get; set; }
    }

    public enum DashboardNames
    {
        Invalid = 0,
        DashboardOperations = 1,
        DashboardFinancials = 2,
        DashboardInventory = 3,
        DashboardMyDash = 4,
    }
}
