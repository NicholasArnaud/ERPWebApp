using ERPWebApp.Models.Company;
using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ERPWebApp.Models
{
    public class ProductIndexData
    {
        public List<Product> Products { get; set; }
        public List<Department> Departments { get; set; }
        public List<ProductTagsRegistry> ProductTags { get; set; }
        public SelectList DepartmentList { get; set; }
    }
}
