namespace ERPWebApp.Data.DTOModels
{
    public class DepartmentShippedTotalDTO
    {
        public DateTime Date { get; set; }
        public int DepartmentId { get; set; }
        public int ShipCount { get; set; }
    }
    public class DepartmentShippedTotalByDateDTO
    {
        public DateTime Date { get; set; }
        public Dictionary<string, int> DepartmentTotals { get; set; } = new Dictionary<string, int>();
    }

}
