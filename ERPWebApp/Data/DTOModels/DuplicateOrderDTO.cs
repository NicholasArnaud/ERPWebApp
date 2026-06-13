using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Orders;
using System.ComponentModel.DataAnnotations;
using static ERPWebApp.Models.Orders.Order;
using static ERPWebApp.Models.Orders.OrderAdvancedOptions;

namespace ERPWebApp.Data.DTOModels
{
    public class DuplicateOrderDTO
    {
        public int ERPOrderId { get; set; }
        [Required(ErrorMessage = "Order Number is Required")]
        public string OrderNumber { get; set; }

        [Required(ErrorMessage = "Store is Required")]
        public int StoreId { get; set; }
        public string StoreName { get; set; }

        public string CustomerNotes { get; set; }
        public string InternalNotes { get; set; }

        public List<DuplicateOrderItemDTO> OrderItems { get; set; }

        #region From Address

        public bool FromResidential { get; set; }
        public string FromName { get; set; }
        public string FromStreet1 { get; set; }
        public string FromStreet2 { get; set; }
        public string FromStreet3 { get; set; }
        public string FromPostalCode { get; set; }
        public string FromCity { get; set; }
        public string FromState { get; set; }
        public string FromCountry { get; set; }
        public string FromPhone { get; set; }
        public string FromCompany { get; set; }

        #endregion

        #region To Address

        public bool ToResidential { get; set; }
        public string ToName { get; set; }
        public string ToStreet1 { get; set; }
        public string ToStreet2 { get; set; }
        public string ToStreet3 { get; set; }
        public string ToPostalCode { get; set; }
        public string ToCity { get; set; }
        public string ToState { get; set; }
        public string ToCountry { get; set; }
        public string ToPhone { get; set; }
        public string ToCompany { get; set; }

        #endregion

        [Required(ErrorMessage = "Duplication Reason is Required")]
        public DuplicationReason? DuplicationReason { get; set; }
        public OrderStatus? OrderStatus { get; set; }

        public int[] TagIds { get; set; }

        public string OrderItemsJsonString { get; set; }

        public DuplicateAdvancedOptionsDTO AdvancedOptionsDTO { get; set; }


        public Order ParentOrder { get; set; }
    }

    public class DuplicateOrderItemDTO
    {
        public int ERPOrderItemId { get; set; }
        [Required(ErrorMessage = "Quantity is Required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }

        public string Sku { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }

        public decimal UnitPrice { get; set; }

        public int ProductId { get; set; }

        public virtual Product Product { get; set; }
    }

    public class DuplicateAdvancedOptionsDTO
    {
        public bool NonMachinable { get; set; }
        public bool SaturdayDelivery { get; set; } = false;
        public bool ContainsAlcohol { get; set; } = false;
        public bool MergedOrSplit { get; set; } = false;

        public string CustomField1 { get; set; }
        public string CustomField2 { get; set; }
        public string CustomField3 { get; set; }
        public string Source { get; set; }

        public BillToParty? BillToParty { get; set; }
        public string BillToAccount { get; set; }
        public string BillToPostalCode { get; set; }
        public string BillToCountryCode { get; set; }
        public string BillToMyOtherAccount { get; set; }

        public string LabelMessageReference1 { get; set; }
        public string LabelMessageReference2 { get; set; }
        public string LabelMessageReference3 { get; set; }
    }
}
