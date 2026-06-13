using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Invoices
{
    public class EasyPostInvoices
    {
        [Key]
        public int EasyPostInvoiceId { get; set; }
        [MaxLength(100)]
        public string fileName { get; set; }
        [Display(Name = "File Url")]
        public string FileUrl { get; set; }
        [Display(Name = "Import Date")]
        [DataType(DataType.DateTime)]
        public DateTime? ImportDate { get; set; }
        [Display(Name = "Imported By")]
        [StringLength(30)]
        public string ImportedBy { get; set; }
        public DateTime? CreatedAt { get; set; }

        [MaxLength(50)]
        public string TrackingCode { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        [MaxLength(50)]
        public string Service { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? Rate { get; set; }

        [MaxLength(50)]
        public string Reference { get; set; }
        public virtual InvoicedOrders GeneralInvoice { get; set; }

        [MaxLength(50)]
        public string Carrier { get; set; }

        [Column(TypeName = "decimal(16,4)")]
        public decimal? InsuredValue { get; set; }

        public bool IsReturn { get; set; }

        [MaxLength(50)]
        public string RefundStatus { get; set; }

        [Column(TypeName = "decimal(16,4)")]
        public decimal? LabelFee { get; set; }

        [Column(TypeName = "decimal(16,4)")]
        public decimal? PostageFee { get; set; }

        [Column(TypeName = "decimal(16,4)")]
        public decimal? InsuranceFee { get; set; }

        public string Options { get; set; }

        public DateTime? PostageLabelCreatedAt { get; set; }

        [MaxLength(50)]
        public string RateId { get; set; }

        [MaxLength(50)]
        public string ParcelId { get; set; }

        [MaxLength(50)]
        public string FromAddressId { get; set; }

        [MaxLength(100)]
        public string FromName { get; set; }

        [MaxLength(100)]
        public string FromCompany { get; set; }

        [MaxLength(100)]
        public string FromStreet1 { get; set; }

        [MaxLength(100)]
        public string FromStreet2 { get; set; }

        [MaxLength(50)]
        public string FromCity { get; set; }

        [MaxLength(10)]
        public string FromState { get; set; }

        [MaxLength(10)]
        public string FromZip { get; set; }

        [MaxLength(2)]
        public string FromCountry { get; set; }

        public bool FromResidential { get; set; }

        [MaxLength(50)]
        public string ToAddressId { get; set; }

        [MaxLength(100)]
        public string ToName { get; set; }

        [MaxLength(100)]
        public string ToCompany { get; set; }

        [MaxLength(100)]
        public string ToStreet1 { get; set; }

        [MaxLength(100)]
        public string ToStreet2 { get; set; }

        [MaxLength(50)]
        public string ToCity { get; set; }

        [MaxLength(10)]
        public string ToState { get; set; }

        [MaxLength(15)]
        public string ToZip { get; set; }

        [MaxLength(2)]
        public string ToCountry { get; set; }

        public bool ToResidential { get; set; }

        [Column(TypeName = "decimal(16,4)")]
        public decimal? Length { get; set; }

        [Column(TypeName = "decimal(16,4)")]
        public decimal? Width { get; set; }

        [Column(TypeName = "decimal(16,4)")]
        public decimal? Height { get; set; }

        [Column(TypeName = "decimal(16,4)")]
        public decimal? Weight { get; set; }

        [MaxLength(50)]
        public string PredefinedPackage { get; set; }
    }
}