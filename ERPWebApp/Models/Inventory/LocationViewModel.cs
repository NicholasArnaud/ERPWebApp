namespace ERPWebApp.Models.Inventory
{
    public class LocationViewModel
    {
        public Location location { get; set; }
        public IEnumerable<Stock> stocks { get; set; }
        public string permission { get; set; }
    }

}
