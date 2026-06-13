namespace ERPWebApp.Data.DTOModels.ShipEngineDtos;


using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Cost
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("amount")]
    public double Amount { get; set; }
}

public class RateDetail
{
    [JsonPropertyName("rate_detail_type")]
    public string RateDetailType { get; set; }

    [JsonPropertyName("carrier_description")]
    public string CarrierDescription { get; set; }

    [JsonPropertyName("carrier_billing_code")]
    public string CarrierBillingCode { get; set; }

    [JsonPropertyName("carrier_memo")]
    public string CarrierMemo { get; set; }

    [JsonPropertyName("amount")]
    public Cost Amount { get; set; }

    [JsonPropertyName("billing_source")]
    public string BillingSource { get; set; }
}

public class LabelDownload
{
    [JsonPropertyName("pdf")]
    public string Pdf { get; set; }

    [JsonPropertyName("png")]
    public string Png { get; set; }

    [JsonPropertyName("zpl")]
    public string Zpl { get; set; }

    [JsonPropertyName("href")]
    public string Href { get; set; }
}

public class ShipEngineLabel
{
    [JsonPropertyName("label_id")]
    public string LabelId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("shipment_id")]
    public string ShipmentId { get; set; }

    [JsonPropertyName("ship_date")]
    public DateTime ShipDate { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("shipment_cost")]
    public Cost ShipmentCost { get; set; }

    [JsonPropertyName("insurance_cost")]
    public Cost InsuranceCost { get; set; }

    [JsonPropertyName("rate_details")]
    public List<RateDetail> RateDetails { get; set; }

    [JsonPropertyName("tracking_number")]
    public string TrackingNumber { get; set; }

    [JsonPropertyName("is_return_label")]
    public bool IsReturnLabel { get; set; }

    [JsonPropertyName("is_international")]
    public bool IsInternational { get; set; }

    [JsonPropertyName("batch_id")]
    public string BatchId { get; set; }

    [JsonPropertyName("carrier_id")]
    public string CarrierId { get; set; }

    [JsonPropertyName("service_code")]
    public string ServiceCode { get; set; }

    [JsonPropertyName("package_code")]
    public string PackageCode { get; set; }

    [JsonPropertyName("voided")]
    public bool Voided { get; set; }

    [JsonPropertyName("voided_at")]
    public DateTime? VoidedAt { get; set; }

    [JsonPropertyName("label_format")]
    public string LabelFormat { get; set; }

    [JsonPropertyName("label_layout")]
    public string LabelLayout { get; set; }

    [JsonPropertyName("trackable")]
    public bool Trackable { get; set; }

    [JsonPropertyName("carrier_code")]
    public string CarrierCode { get; set; }

    [JsonPropertyName("tracking_status")]
    public string TrackingStatus { get; set; }

    [JsonPropertyName("label_download")]
    public LabelDownload LabelDownload { get; set; }

    [JsonPropertyName("form_download")]
    public object FormDownload { get; set; }

    [JsonPropertyName("insurance_claim")]
    public object InsuranceClaim { get; set; }
}

public class Link
{
    [JsonPropertyName("href")]
    public string Href { get; set; }
}

public class Links
{
    [JsonPropertyName("first")]
    public Link First { get; set; }

    [JsonPropertyName("last")]
    public Link Last { get; set; }

    [JsonPropertyName("prev")]
    public object Prev { get; set; }

    [JsonPropertyName("next")]
    public Link Next { get; set; }
}

public class ShipEngineLabelRoot
{
    [JsonPropertyName("labels")]
    public List<ShipEngineLabel> ShipEngineLabels { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    [JsonPropertyName("links")]
    public Links Links { get; set; }
}