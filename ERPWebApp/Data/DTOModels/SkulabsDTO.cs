using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ERPWebApp.Models.Orders;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Data.DTOModels
{
    public class SkulabsDTO
    {
        public string StoreId { get; set; }
        public string OrderNumber { get; set; }
        public string Address { get; set; }
        public string Carrier { get; set; }
        public string TrackingNumber { get; set; }
        public string Service { get; set; }
        public List<SkulabsItemDTO> OrderItems { get; set; }
        public string WarehouseId { get; set; }
        public string Dropship { get; set; }
        public bool NoRefresh { get; set; }
        public string Notes { get; set; }
        public string ForceDeduction { get; set; }
        public decimal Cost { get; set; }
        public JsonObject Options { get; set; }
        public string Booking { get; set; }
        public string Seal { get; set; }
        public string OriginAddress { get; set; }
        public string BolProNumber { get; set; }
        public string TotalBundles { get; set; }
        public string TrackingUrl { get; set; }
        public string Parcel { get; set; }
        
        //Adding more options to capture necessary order information. Everything under this is added.
        public decimal Weight { get; set; }
        public string WeightUnits { get; set; }
        public decimal Height { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public string DimUnits { get; set; }
        public string UserId { get; set; }
        public string OrderSource { get; set; }
        public decimal EstimatedShipmentCost { get; set; }
        public OrderInsuranceOptions InsuranceOptions { get; set; }
        public string CarrierCode { get; set; }
        public string ServiceCode { get; set; }
        public string PackageCode { get; set; }
        public string Confirmation { get; set; }
        public OrderShippingInfo ShipFrom { get; set; }
        public OrderShippingInfo ShipTo { get; set; }
        public OrderAdvancedOptions AdvancedOptions { get; set; }
        public bool IsExpedited { get; set; }
    }

    public class SkulabsItemDTO
    {
        public int Quantity { get; set; }
        public string LocationId { get; set; }
        public string ItemId { get; set; }
        public string LineId { get; set; }
        public List<string> SerialNumbers { get; set; }
        public string Id { get; set; }
    }

    public class SkulabsResponseDTO
    {
        public List<Shipment> Shipments { get; set; }

        public class Shipment
        {
            [JsonPropertyName("_id")]
            public string Id { get; set; }

            [JsonPropertyName("user_id")]
            public string UserId { get; set; }

            public Request Request { get; set; }
            public Response Response { get; set; }
            public bool Voided { get; set; }

            [JsonPropertyName("manual_shipment")]
            public bool ManualShipment { get; set; }

            public string Deducted { get; set; }

            [JsonPropertyName("return_label")]
            public bool ReturnLabel { get; set; }

            public DateTime Time { get; set; }

            [JsonPropertyName("tracking_status")]
            public string TrackingStatus { get; set; }

            [JsonPropertyName("tracking_type")]
            public object TrackingType { get; set; }

            [JsonPropertyName("last_tracking_update")]
            public object LastTrackingUpdate { get; set; }

            [JsonPropertyName("carrier_account_id")]
            public object CarrierAccountId { get; set; }
        }

        public class Request
        {
            public string Address { get; set; }

            [JsonPropertyName("order_items")]
            public List<OrderItem> OrderItems { get; set; }

            public string Origin { get; set; }

            [JsonPropertyName("origin_address")]
            public object OriginAddress { get; set; }

            public object Parcel { get; set; }
        }

        public class OrderItem
        {
            public int Quantity { get; set; }

            [JsonPropertyName("location_id")]
            public string LocationId { get; set; }

            [JsonPropertyName("item_id")]
            public string ItemId { get; set; }

            [JsonPropertyName("line_id")]
            public string LineId { get; set; }

            [JsonPropertyName("serial_numbers")]
            public List<object> SerialNumbers { get; set; }

            [JsonPropertyName("_id")]
            public string Id { get; set; }
        }

        public class Response
        {
            public string Provider { get; set; }

            [JsonPropertyName("tracking_number")]
            public string TrackingNumber { get; set; }

            [JsonPropertyName("tracking_url")]
            public object TrackingUrl { get; set; }

            public object Seal { get; set; }

            [JsonPropertyName("bol_pro_number")]
            public object BolProNumber { get; set; }

            [JsonPropertyName("total_bundles")]
            public object TotalBundles { get; set; }

            public object Booking { get; set; }

            public string Service { get; set; }

            public decimal Amount { get; set; }
        }
    }
}
