using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models
{
    public class SkulabsImport
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string Store { get; set; }
        [MaxLength(50)]
        public string Order { get; set; }
        [MaxLength(50)]
        public string TrackingNumber { get; set; }
        public long Line_ID { get; set; }
        [MaxLength(50)]
        public string OrderStatus { get; set; }
        public bool Archived { get; set; }
        [MaxLength(350)]
        public string Image { get; set; }
        [MaxLength(250)]
        public string Name { get; set; }
        [MaxLength(250)]
        public string ListingName { get; set; }
        [MaxLength(250)]
        public string VariantName { get; set; }
        [MaxLength(150)]
        public string SKU { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? Cost { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? Wholesale { get; set; }
        public float? Retail { get; set; }
        public int? PriceSold { get; set; }
        public int Quantity { get; set; }
        [MaxLength(50)]
        public string? DropShipped { get; set; }
        public string? Metadata { get; set; }
        public bool Cleared { get; set; }
        public int PickedQuantity { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShipmentDate { get; set; }
        public bool? ManualShipment { get; set; }
        [MaxLength(50)]
        public string Type { get; set; }
        [MaxLength(50)]
        public string AssignedWarehouse { get; set; }
        [MaxLength(250)]
        public string LineSKU { get; set; }
        [MaxLength(250)]
        public string LineName { get; set; }
        [MaxLength(50)]
        public string CustomerName { get; set; }
        [MaxLength(50)]
        public string CustomerEmail { get; set; }
        [MaxLength(50)]
        public string Company { get; set; }
        [MaxLength(50)]
        public string? CustomerNumber { get; set; }
        [MaxLength(250)]
        public string AddressLine1 { get; set; }
        [MaxLength(250)]
        public string? AddressLine2 { get; set; }
        [MaxLength(50)]
        public string City { get; set; }
        [MaxLength(50)]
        public string State { get; set; }
        [MaxLength(50)]
        public string Zip { get; set; }
        [MaxLength(50)]
        public string Country { get; set; }
        [MaxLength(50)]
        public string Postage { get; set; }
        [MaxLength(50)]
        public string Provider { get; set; }
        [MaxLength(50)]
        public string Method { get; set; }
        [MaxLength(50)]
        public string? _3PLPartnerSKU { get; set; }
        [MaxLength(300)]
        public string OrderTags { get; set; }
        [MaxLength(100)]
        public string Personalization1Key { get; set; }
        [MaxLength(300)]
        public string Personalization1Value { get; set; }
        [MaxLength(100)]
        public string Personalization2Key { get; set; }
        [MaxLength(300)]
        public string Personalization2Value { get; set; }
        [MaxLength(100)]
        public string Personalization3Key { get; set; }
        [MaxLength(300)]
        public string Personalization3Value { get; set; }
        [MaxLength(100)]
        public string Personalization4Key { get; set; }
        [MaxLength(300)]
        public string Personalization4Value { get; set; }
        [MaxLength(100)]
        public string Personalization5Key { get; set; }
        [MaxLength(300)]
        public string Personalization5Value { get; set; }
        [MaxLength(100)]
        public string Personalization6Key { get; set; }
        [MaxLength(300)]
        public string Personalization6Value { get; set; }
        [MaxLength(100)]
        public string Personalization7Key { get; set; }
        [MaxLength(300)]
        public string Personalization7Value { get; set; }
        [MaxLength(100)]
        public string Personalization8Key { get; set; }
        [MaxLength(300)]
        public string Personalization8Value { get; set; }
        [MaxLength(100)]
        public string Personalization9Key { get; set; }
        [MaxLength(300)]
        public string Personalization9Value { get; set; }

        [MaxLength(50)]
        public string fileName { get; set; }

        [Display(Name = "File Url")]
        public string FileUrl { get; set; }

        [Display(Name = "Import Date")]
        [DataType(DataType.DateTime)]
        public DateTime? ImportDate { get; set; }

        [Display(Name = "Imported By")]
        [StringLength(30)]
        public string ImportedBy { get; set; }
    }
}