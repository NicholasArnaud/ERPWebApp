using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Orders;

public class OrderAdvancedOptions
{
    [Key]
    public int OrderAdvancedOptionsId { get; set; }
    public int ERPOrderId { get; set; }
    [ForeignKey(nameof(ERPOrderId))]
    public Order Order { get; set; }
    /// <summary>
    /// Specifies the warehouse where to the order is to ship from. If the order was fulfilled using a fill provider, no warehouse is attached to these orders and will result in a null value being returned. *Please see note below
    /// </summary>
    [Display(Name = "Warehouse Id")]
    public long? warehouseId { get; set; }
    [Display(Name = "Is NonMachinable")]
    public bool nonMachinable { get; set; } = false;
    [Display(Name = "Is Saturday Delivery")]
    public bool saturdayDelivery { get; set; } = false;
    [Display(Name = "Is Alcohol")]
    public bool containsAlcohol { get; set; } = false;
    /// <summary>
    /// 	ID of store that is associated with the order. If not specified in the CreateOrder call either to create or update an order, ShipStation will default to the first manual store on the account. Can only be specified during order creation.
    /// </summary>
    [Display(Name = "Store Id")]
    public long storeId { get; set; }

    [Display(Name = "Store Name"), MaxLength(100)]
    public string storeName { get; set; }
    /// <summary>
    /// Field that allows for custom data to be associated with an order. *Please see note below
    /// </summary>
    [Display(Name = "Custom Field 1"), MaxLength(300)]
    public string customField1 { get; set; }

    /// <summary>
    /// Field that allows for custom data to be associated with an order. *Please see note below
    /// </summary>
    [Display(Name = "Custom Field 2"), MaxLength(300)]
    public string customField2 { get; set; }
    /// <summary>
    /// Field that allows for custom data to be associated with an order. *Please see note below
    /// </summary>
    [Display(Name = "Custom Field 3"), MaxLength(300)]
    public string customField3 { get; set; }
    /// <summary>
    /// Identifies the original source/marketplace of the order. *Please see note below
    /// </summary>
    [Display(Name = "Source"), MaxLength(150)]
    public string source { get; set; }
    /// <summary>
    /// Read-Only: Returns whether or not an order has been merged or split with another order. Read Only
    /// </summary>
    [Display(Name = "Is Merged Or Split")]
    public bool mergedOrSplit { get; set; } = false;
    /// <summary>
    /// Read-Only: Array of orderIds. Each orderId identifies an order that was merged with the associated order. Read Only
    /// </summary>
    [Display(Name = "Merged Ids"), NotMapped]
    public long[] mergedIds { get; set; } = [];
    /// <summary>
    /// Read-Only: If an order has been split, it will return the Parent ID of the order with which it has been split. If the order has not been split, this field will return null. Read Only
    /// </summary>
    [Display(Name = "Parent Id")]
    public long? parentId { get; set; }
    /// <summary>
    /// Identifies which party to bill. Possible values: "my_account", "my_other_account" (see note below), "recipient", "third_party". billTo values can only be used when creating/updating orders.
    /// </summary>
    [Display(Name = "Bill To Party")]
    public BillToParty? billToParty { get; set; } = BillToParty.my_account;
    /// <summary>
    /// Account number of billToParty. billTo values can only be used when creating/updating orders.
    /// </summary>
    [Display(Name = "Bill To Account"), MaxLength(100)]
    public string billToAccount { get; set; }
    /// <summary>
    /// Postal Code of billToParty. billTo values can only be used when creating/updating orders.
    /// </summary>
    [Display(Name = "Bill To Postal Code"), MaxLength(10)]
    public string billToPostalCode { get; set; }
    /// <summary>
    /// Country Code of billToParty. billTo values can only be used when creating/updating orders.
    /// </summary>
    [Display(Name = "Bill To Country Code"), MaxLength(10)]
    public string billToCountryCode { get; set; }
    /// <summary>
    /// When using my_other_account billToParty value, the shippingProviderId value associated with the desired account. Make a List Carriers call to obtain shippingProviderId values.
    /// </summary>
    [Display(Name = "Bill To My Other Account")]
    public long? billToMyOtherAccount { get; set; }

    [MaxLength(50)]
    public string labelMessageReference1 { get; set; }
    [MaxLength(50)]
    public string labelMessageReference2 { get; set; }
    [MaxLength(50)]
    public string labelMessageReference3 { get; set; }


    public enum BillToParty
    {
        my_account,
        my_other_account,
        recipient,
        third_party
    }
    /*
     * https://www.shipstation.com/docs/api/models/advanced-options/
     * ShipStaion API NOTES:
     * *Can only be modified via the CreateOrder call. CreateLabelForOrder will default these fields to the values set prior to the call.
     * CreateLabelForOrder will not affect billTo values. Calls will default billTo fields to the values set in the order.
     * **Setting "billToParty" attribute to "my_other_account" is used when wanting to charge a second connected account for the same carrier.
     */
}
