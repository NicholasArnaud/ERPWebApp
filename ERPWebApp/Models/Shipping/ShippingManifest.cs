using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace ERPWebApp.Models.Shipping;

[Index(nameof(ManifestId), IsUnique = true)]
public class ShippingManifest
{
    [Key]
    public Guid Id { get; set; }
    [property: JsonPropertyName("manifest_id")]
    [Required]
    [MaxLength(50)]
    public string ManifestId { get; set; }
    
    [property: JsonPropertyName("warehouse_id")]
    [MaxLength(50)]
    public string WarehouseId { get; set; }
    
    [MaxLength(150)]
    public string Warehouse { get; set; }
    
    [property: JsonPropertyName("carrier_id")]
    [MaxLength(50)]
    public string CarrierId { get; set; }
    
    [MaxLength(150)]
    public string Carrier { get; set; }
    
    [property: JsonPropertyName("created_at")]
    public DateTime CreatedDate { get; set; }
    
    [property: JsonPropertyName("ship_date")]
    public DateTime ShipDate { get; set; }
    
    [property: JsonPropertyName("shipments")]
    public int ShipmentCount { get; set; }
    
    [property: JsonPropertyName("manifestFile")]
    public string ManifestFile { get; set; }
}