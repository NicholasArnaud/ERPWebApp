using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Inventory
{
    public class Stock
    {
        [Key]
        public int StockId { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Products { get; set; }

        [Display(Name = "Location")]
        public int LocationId { get; set; }

        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }

        [Display(Name = "Total Available")]
        public int TotalAvailable { get; set; }

        [Display(Name = "Recently Readded")]
        public bool RecentlyReadded { get; set; }

        [Display(Name = "Primary")]
        public bool IsPrimary { get; set; }

        [Display(Name = "Last Modified Date")]
        public DateTime ModifyDate { get; set; }

        [Display(Name = "Modified By User")]
        public string ModifyByUser { get; set; }

        [Display(Name = "Last Counted")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime LastCounted { get; set; }

        [Display(Name = "Being Counted")]
        public Boolean BeingCounted { get; set; }

        [Display(Name = "External Stock")]
        [DefaultValue(false)]
        public bool IsExternal { get; set; }

        [Display(Name = "Shipstation Store")]
        public int? ShipStationStoreId { get; set; }
        [ForeignKey("ShipStationStoreId")]
        public virtual ShipStationStore? ShipStationStore { get; set; }
    }
}