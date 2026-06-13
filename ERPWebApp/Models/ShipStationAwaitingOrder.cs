using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    public class ShipStationAwaitingOrder
    {
        public int ShipStationAwaitingOrderId { get; set; }
        public string OrderNumber { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? OrderDate { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? LastModifiedDate { get; set; }
        public string OrderStatus { get; set; }
        public string CustomerNotes { get; set; }
        public string InternalNotes { get; set; }
        public string ItemSku { get; set; }
        public string ItemName { get; set; }
        public int ItemQuantity { get; set; }
        public int StoreId { get; set; }
        public long OrderItemId { get; set; }
        public string SSOrderId { get; set; }
        public string ItemOptions { get; set; }
        public string TagIds { get; set; }
    }
}
