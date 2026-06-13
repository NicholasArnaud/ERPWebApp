using System.Text.Json.Serialization;

namespace ERPWebApp.Data.DTOModels.ShippingScanout.USPS;

public class ErrorDetail
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }
}

public class Error
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("errors")]
    public List<ErrorDetail> Errors { get; set; }
}

public class UspsFailingResponse
{
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; }

    [JsonPropertyName("error")]
    public Error Error { get; set; }
}
