namespace ERPWebApp.Models
{
    public class AssignedDepartmentViewModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool isProduction { get; set; }
        public bool Assigned { get; set; }
    }
}
