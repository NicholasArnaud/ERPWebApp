using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models.Inventory
{
    public class StockViewModel
    {
        public Stock Stock { get; set; } = new Stock();
        public IEnumerable<Site>? Sites { get; set; }
        public Location? Location { get; set; } 
        public bool CreateNewLocation { get; set; }
        [Display(Name = "Location Name")]
        public int? LocationId { get; set; } = 0;
        public ShipStationStore? ShipStationStore {get;set;}
        [Display(Name = "ShipStation Store")]
        public int? ShipStationStoreId {get;set;}
    }


}
