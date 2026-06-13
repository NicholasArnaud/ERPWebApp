using ERPWebApp.Data.DTOModels;

namespace ERPWebApp.Models
{
    public class HomeViewModel
    {
        public List<TopDepartment> TopDepartmentInfo { get; set; }
        public List<TallyDto> DailyOrderCompletionCountInfo { get; set; }
        public string topDepartmentData { get; set; }
        public string dailyOrderCompletionCountData { get; set; }

        public HomeViewModel()
        {
            TopDepartmentInfo = new List<TopDepartment>();
            DailyOrderCompletionCountInfo = new List<TallyDto>();
        }
    }
}
