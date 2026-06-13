using System.Text.Json.Serialization;

namespace ERPWebApp.Data.DTOModels.ShippingScanout.USPS;

public record UspsAddressValidationResponse(
    [property: JsonPropertyName("firm")] string Firm,
    [property: JsonPropertyName("address")] UspsAddress Address,
    [property: JsonPropertyName("additionalInfo")] AdditionalInfo AdditionalInfo
);

public record AdditionalInfo(
    [property: JsonPropertyName("DPVConfirmation")] string DpvConfirmation,
    [property: JsonPropertyName("DPVCMRA")] string Dpvcmra
);