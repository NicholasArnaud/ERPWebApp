using System.Text.Json.Serialization;
namespace ERPWebApp.Data.DTOModels.ShipEngineDtos;

public record ShipEngineVoidMessage
{
    [JsonPropertyName("approved")]
    public bool Approved { get; set; }
    [JsonPropertyName("message")]
    public string Message{ get; set; }
}
public record ShipStationVoidMessage
{
    [JsonPropertyName("approved")]
    public bool Approved { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
}