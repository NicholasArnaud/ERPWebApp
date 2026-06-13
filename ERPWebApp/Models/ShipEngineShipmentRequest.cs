#nullable enable
namespace ERPWebApp.Models;

public class ShipEngineShipmentRequest
{
    public required ShipmentObj shipment { get; set; }
    public required string label_format { get; set; }

    public class ShipmentObj
    {
        public required string carrier_id { get; set; }
        public required string service_code { get; set; }
        public DateTime ship_date { get; set; }
        public required ShipToObj ship_to { get; set; }
        public required ShipFromObj ship_from { get; set; }
        public required string confirmation { get; set; }
        public CustomsObj? customs { get; set; }
        public required string insurance_provider { get; set; }
        public required PackageObj[] packages { get; set; }
    }

    public class ShipToObj
    {
        public required string name { get; set; }
        public required string phone { get; set; }
        public required string address_line1 { get; set; }
        public required string address_line2 { get; set; }
        public required string city_locality { get; set; }
        public required string state_province { get; set; }
        public required string postal_code { get; set; }
        public required string country_code { get; set; }
        public required string address_residential_indicator { get; set; }
    }

    public class ShipFromObj
    {
        public required string name { get; set; }
        public required string company_name { get; set; }
        public required string phone { get; set; }
        public required string address_line1 { get; set; }
        public required string address_line2 { get; set; }
        public required string city_locality { get; set; }
        public required string state_province { get; set; }
        public required string postal_code { get; set; }
        public required string country_code { get; set; }
        public required string address_residential_indicator { get; set; }
    }

    public class CustomsObj
    {
        public required string contents { get; set; }
        public required string non_delivery { get; set; }
        public List<CustomsItem>? customs_items { get; set; }
    }

    public class CustomsItem
    {
        public int quantity { get; set; }
        public decimal value { get; set; }
        public required string harmonized_tariff_code { get; set; }
        public required string sku { get; set; }
        public required string sku_description { get; set; }
        public required string country_of_origin { get; set; }
    }

    public class PackageObj
    {
        public required string package_code { get; set; }
        public required WeightObj weight { get; set; }
        public DimensionsObj? dimensions { get; set; }
        public InsuredValueObj? insured_value { get; set; }
        public LabelMessageObj? label_messages { get; set; }
    }

    public class LabelMessageObj
    {
        public string? reference1 { get; set; }
        public string? reference2 { get; set; }
        public string? reference3 { get; set; }
    }

    public class WeightObj
    {
        public double value { get; set; }
        public required string unit { get; set; }
    }

    public class DimensionsObj
    {
        public required string unit { get; set; }
        public double length { get; set; }
        public double width { get; set; }
        public double height { get; set; }
    }

    public class InsuredValueObj
    {
        public required string currency { get; set; }
        public double amount { get; set; }
    }
}
