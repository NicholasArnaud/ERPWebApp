using System.Text.Json.Serialization;

namespace ERPWebApp.Data.DTOModels.ShippingScanout.USPS;

public record UspsAddress(
    [property: JsonPropertyName("streetAddress")] string StreetAddress,
    [property:JsonPropertyName("streetAddressAbbreviation")] string StreetAddressAbbreviation,
    [property: JsonPropertyName("secondaryAddress")] string SecondaryAddress,
    [property: JsonPropertyName("cityAbbreviation")] string CityAbbreviation,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("urbanization")] string Urbanization,
    [property: JsonPropertyName("ZIPCode")] string ZipCode,
    [property: JsonPropertyName("ZIPPlus4")] string ZipPlus4
);