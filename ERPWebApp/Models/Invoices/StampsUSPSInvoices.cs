using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Invoices
{
    public enum PaymentType
    {
        Prepaid,
        Other // Can add more as required, all I see is Prepaid currently.
    }

    public enum ShipmentStatus
    {
        Printed,
        Delivered,
        Other // Can add more as required.
    }

    public enum RefundStatus
    {
        Pending,
        Approved,
        Rejected,
        Other
    }

    public class StampsUSPSInvoices
    {
        [Key]
        public int StampsUSPSInvoiceId { get; set; }
        [MaxLength(50)]
        public string orderNumber { get; set; }
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
        public DateTime? DatePrinted { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? AmountPaid { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? AdjustedAmount { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? QuotedAmount { get; set; }
        public PaymentType? PaymentType { get; set; }
        public ShipmentStatus? Shipment { get; set; }
        [MaxLength(50)]
        public string TrackingNumber { get; set; }
        public DateTime? DateDelivered { get; set; }
        [MaxLength(200)]
        public string Recipient { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string Address1 { get; set; }
        [MaxLength(50)]
        public string Address2 { get; set; }
        [MaxLength(50)]
        public string Address3 { get; set; }
        [MaxLength(50)]
        public string City { get; set; }
        [MaxLength(50)]
        public string StateOrProvince { get; set; }
        [MaxLength(50)]
        public string PostalCode { get; set; }
        [MaxLength(50)]
        public string Country { get; set; }
        [MaxLength(50)]
        public string OriginZip { get; set; }
        [MaxLength(50)]
        public string Weight { get; set; } // Keeping as string for now to accommodate formats like "10lb 0oz", will get with Nick and see if we want to do any calculations on our end.
        [MaxLength(50)]
        public string Carrier { get; set; }
        [MaxLength(50)]
        public string Service { get; set; }
        [MaxLength(50)]
        public string TrackingConfirmation { get; set; }
        [MaxLength(50)]
        public string ExtraService { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? InsuredFor { get; set; }
        public DateTime? ShipDate { get; set; }
        [MaxLength(50)]
        public string CostCode { get; set; }
        [MaxLength(50)]
        public string PrintedMessage { get; set; }
        [MaxLength(50)]
        public string User { get; set; }
        [MaxLength(50)]
        public string RefundType { get; set; }
        public DateTime? RefundRequestDate { get; set; }
        public RefundStatus? RefundStatus { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? RefundRequested { get; set; }
        [MaxLength(50)]
        public string Reference1 { get; set; }
        [MaxLength(50)]
        public string OrderID { get; set; }
        [MaxLength(50)]
        public string Store { get; set; }
        public DateTime? OrderDate { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? OrderTotal { get; set; }
        [MaxLength(50)]
        public string ItemSKUs { get; set; }
        [MaxLength(50)]
        public string Items { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? ProductTotal { get; set; }
        [MaxLength(50)]
        public string ShippingPaid { get; set; }
        [MaxLength(50)]
        public string TaxPaid { get; set; }
        [MaxLength(50)]
        public string InsuranceProvider { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? DutiesTaxesAmount { get; set; }
        public virtual InvoicedOrders GeneralInvoice { get; set; }
    }
}
