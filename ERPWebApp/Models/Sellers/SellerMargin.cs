using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Sellers
{
    public class SellerMargin
    {
        [Key]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; }


        [Display(Name = "Tracking Number")]
        public string TrackingNumber { get; set; }


        [Display(Name = "Fulfillment Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal FulfillmentCost { get; set; }


        [Display(Name = "Store Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal StoreCost { get; set; }


        [Display(Name = "Tax Amount")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }


        [Display(Name = "Shipping Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; }


        [Display(Name = "Store Shipping Cost")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal StoreShippingCost { get; set; }


        [Display(Name = "Store Sale")]
        [Column(TypeName = "decimal(18,2)")]
        [DataType(DataType.Currency)]
        public decimal StoreSale { get; set; }


        [Display(Name = "Profit Margin")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ProfitMargin { get; set; }



        //if role == Management=> show StoreName
        [Display(Name = "Store Name")]
        public string StoreName { get; set; }



        [ForeignKey("ShipStationStoreId")]
        public int ShipStationStoreId { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Order Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime OrderDate { get; set; }

    }
}
