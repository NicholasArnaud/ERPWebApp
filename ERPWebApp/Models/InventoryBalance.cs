using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    public class InventoryBalance
    {
        public int InventoryBalanceId { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }

        [Display(Name = "Total Available")]
        public int TotalAvailable { get; set; }

        [Display(Name = "ShipStation Orders")]
        public int PendingShipStationOrders { get; set; }

        [Display(Name = "Order Difference")]
        public int OrderDifference { get; set; }

        [Display(Name = "External Site?")]
        [DefaultValue(false)]
        public bool IsExternalSiteInventory { get; set; }
    }
}
