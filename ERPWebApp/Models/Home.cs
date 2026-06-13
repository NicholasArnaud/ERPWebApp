using ERPWebApp.Models.Company;

namespace ERPWebApp.Models;

public class Home
{

    public SpeedOMeterGoal SpeedOMeterGoal { get; set; }
    public ProductionVsLaborCostPrice ProductionVsLaborCostPrice { get; set; }
    public List<Department> Department { get; set; }
    public List<DashboardLayout> DashboardLayouts { get; set; }
    public bool SpeedOMeter { get; set; }
    public bool OrderShipments { get; set; }
    public bool DepartmentOrderHistory { get; set; }
    public bool TopDepartment { get; set; }

    public Home()
    {
        SpeedOMeterGoal = new SpeedOMeterGoal();
        ProductionVsLaborCostPrice = new ProductionVsLaborCostPrice();
        Department = new List<Department>();
        DashboardLayouts = new List<DashboardLayout>();
    }
}
