using ERPWebApp.Models.Inventory;

namespace ERPWebApp.Data.DTOModels
{
    public class ProductGalleryDTO
    {
        public IEnumerable<Product> Products { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int TotalEntries { get; set; }
        public int EntriesPerPage { get; set; }
        public int pageIndex { get; set; }
        public string type { get; set; }
        public List<int> SelectedDepartments { get; set; }
        public List<int> SelectedProducts { get; set; }
    }
}
