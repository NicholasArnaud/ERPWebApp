namespace ERPWebApp.Data.DTOModels
{
    public class TallyDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentColor { get; set; }
        public int Tally { get; set; }
        public float OriginalTarget { get; set; }
        public float Target { get; set; }
        public ICollection<DepartmentProductInfo> DepartmentGoals { get; set; }
    }
    public class DepartmentProductInfo
    {
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; }
        public string ProductSku { get; set; }
        public int Quantity { get; set; }
    }

    public class ShipstationOrderDto
    {
        public DateTime OrderDate { get; set; }
        public int OrdersIn { get; set; }
        public DateTime ShipDate { get; set; }
        public int OrdersOut { get; set; }
    }

    public sealed record TopDepartment
    (
        DateTime ShipDate, 
        string DepartmentName, 
        string DepartmentColor, 
        int TotalItemsShipped
    );
}
