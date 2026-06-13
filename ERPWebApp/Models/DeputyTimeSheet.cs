using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    public class DeputyTimeSheet
    {
        [Key]
        public int DeputyTimeSheetId { get; set; }
        public int DeputyId { get; set; }
        public int DeputyEmployeeId { get; set; }
        public int EmployeeHistory { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Department { get; set; }
        public int StartTime { get; set; }
        public DateTime StartTimeLocalized { get; set; }
        public int EndTime { get; set; }
        public DateTime EndTimeLocalized { get; set; }
        public bool IsInProgress { get; set; }
        public bool IsDiscarded { get; set; }
        public DateTime Date { get; set; }
        public DateTime MealBreak { get; set; }
        public float TotalTime { get; set; }
        public float TotalTimeInv { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}
