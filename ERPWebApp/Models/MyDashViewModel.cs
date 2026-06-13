using ERPWebApp.Data.DTOModels;
using ERPWebApp.Models.Company;

namespace ERPWebApp.Models
{
    public class MyDashViewModel
    {
        public MyDashDTO MyDashData { get; set; }
        public OperationsDTO OperationsData { get; set; }
        public FinancialsViewModel Financials { get; set; }
        public InventoryViewModel Inventory { get; set; }
        public HomeViewModel HomeView { get; set; }
        public Home Home { get; set; }
        public List<DashboardLayout> DashboardLayouts { get; set; }

        public MyDashViewModel()
        {
            MyDashData = new MyDashDTO();
            OperationsData = new OperationsDTO();
            Financials = new FinancialsViewModel();
            Inventory = new InventoryViewModel();
            HomeView = new HomeViewModel();
            Home = new Home();
            DashboardLayouts = new List<DashboardLayout>();
        }
    
    }
}
