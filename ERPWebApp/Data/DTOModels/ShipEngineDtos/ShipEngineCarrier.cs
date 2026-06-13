using System.Text.Json.Serialization;

namespace ERPWebApp.Data.DTOModels.ShipEngineDtos;

public record ShipEngineCarriers(
    [property: JsonPropertyName("carrier_id")] string CarrierId,
    [property: JsonPropertyName("carrier_code")] string CarrierCode,
    [property: JsonPropertyName("account_number")] string AccountNumber,
    [property: JsonPropertyName("nickname")] string Nickname,
    [property: JsonPropertyName("friendly_name")] string FriendlyName
);
public class ShipEngineCarrierRoot
{
    public List<ShipEngineCarriers> Carriers { get; set; }
}