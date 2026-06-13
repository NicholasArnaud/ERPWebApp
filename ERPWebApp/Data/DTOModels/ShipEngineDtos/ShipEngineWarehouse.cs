using System.Text.Json.Serialization;

namespace ERPWebApp.Data.DTOModels.ShipEngineDtos;

public record ShipEngineWarehouse(
    [property: JsonPropertyName("warehouse_id")] string WarehouseId,
    [property: JsonPropertyName("is_default")] bool IsDefault,
    [property: JsonPropertyName("name")] string WarehouseName,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("origin_address")] ShipEngineWarehouseAddress OriginAddress,
    [property: JsonPropertyName("return_address")] ShipEngineWarehouseAddress ReturnAddress
);

public record ShipEngineWarehouseAddress(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("phone")] string Phone,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("company_name")] string CompanyName,
    [property: JsonPropertyName("address_line1")] string AddressLine1,
    [property: JsonPropertyName("address_line2")] string AddressLine2,
    [property: JsonPropertyName("address_line3")] string AddressLine3,
    [property: JsonPropertyName("city_locality")] string City,
    [property: JsonPropertyName("state_province")] string State,
    [property: JsonPropertyName("postal_code")] string PostalCode,
    [property: JsonPropertyName("country_code")] string CountryCode,
    [property: JsonPropertyName("address_residential_indicator")] string AddressResidentialIndicator
);

public class WarehousesRoot
{
    public List<ShipEngineWarehouse> Warehouses { get; set; }
}