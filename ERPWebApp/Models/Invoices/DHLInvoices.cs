using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Invoices
{
    public class DHLInvoices
    {
        [Key]
        public int DHLInvoiceId { get; set; }
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
        [MaxLength(100)]
        public string RecType { get; set; }
        public int? SoldTo { get; set; }
        public int? InvPosnr { get; set; }
        public int? BOL { get; set; }
        [MaxLength(50)]
        public string BillRef { get; set; }
        [MaxLength(50)]
        public string BillRef2 { get; set; }
        [MaxLength(50)]
        public string ProcessingFacility { get; set; }
        public int? PickUpFrom { get; set; }
        public DateTime? PUDATE { get; set; }
        public int? PUTIME { get; set; }
        public long? InternalTrackingNum { get; set; }

        [MaxLength(50)]
        public string CustomerConfirm { get; set; }
        [MaxLength(50)]
        public string DeliveryConfirm { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string Address1 { get; set; }
        [MaxLength(50)]
        public string Address2 { get; set; }
        [MaxLength(50)]
        public string City { get; set; }
        [MaxLength(50)]
        public string State { get; set; }
        public int? Zip { get; set; }
        [MaxLength(50)]
        public string Country { get; set; }
        public int? MaterialOrVASNum { get; set; }
        [MaxLength(50)]
        public string MaterialOrVASDesc { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? ACTWeight { get; set; }
        [MaxLength(50)]
        public string UOMACTWeight { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? BILLWEIGHT { get; set; }
        [MaxLength(50)]
        public string UOMBillWgt { get; set; }
        public int? Quantity { get; set; }
        public int? UOMQuantity { get; set; }
        [MaxLength(50)]
        public string PricingZone { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? Charge { get; set; }
        [MaxLength(50)]
        public string CustRef { get; set; }
        [MaxLength(50)]
        public string CustRef2 { get; set; }
        [MaxLength(50)]
        public string WorkshareDropoff { get; set; }
        [MaxLength(50)]
        public string WorkshareSort { get; set; }
        [MaxLength(50)]
        public string WorkshareStamp { get; set; }
        [MaxLength(50)]
        public string WorkshareMachine { get; set; }
        [MaxLength(50)]
        public string WorkshareManifest { get; set; }
        [MaxLength(50)]
        public string WorkshareBPM { get; set; }
        [MaxLength(50)]
        public string WorkshareFutureUse1 { get; set; }
        [MaxLength(50)]
        public string WorkshareFutureUse2 { get; set; }
        [MaxLength(50)]
        public string WorkshareFutureUse3 { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? SurchargeContentEndors { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? SurchargeUnassignableAdd { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? SurchargeSpecialHandling { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? SurchargeLateArrival { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? SurchargeUSPSQualif { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? SurchargeClientSRD { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? SurchargeIrregular { get; set; }
        [MaxLength(50)]
        public string ReturnedMailUnassignable { get; set; }
        [MaxLength(50)]
        public string ReturnedMailUnprocessable { get; set; }
        [MaxLength(50)]
        public string ReturnedMailRecall { get; set; }
        [MaxLength(50)]
        public string ReturnedMailDuplicate { get; set; }
        [MaxLength(50)]
        public string ReturnedMailContAssur { get; set; }
        [MaxLength(50)]
        public string ReturnedMailMoveUpdate { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? GST_Tax { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? HST_Tax { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? PST_Tax { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? VAT_Tax { get; set; }
        [MaxLength(50)]
        public string Duties { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? Tax { get; set; }
        [MaxLength(50)]
        public string ReturnedMailPaperInvoice { get; set; }
        [MaxLength(50)]
        public string ReturnedMailScreening { get; set; }
        [MaxLength(50)]
        public string ReturnedMailNonAutoFlats { get; set; }
        [MaxLength(50)]
        public string ReturnedMailFutureUse { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? SurchargeFuel { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? MinPickupCharge { get; set; }
        [Column(TypeName = "decimal(30, 2)")]
        public decimal? OverlabeledValue { get; set; }
        [MaxLength(50)]
        public string DimWeight { get; set; }
        [MaxLength(50)]
        public string UOMDimWeight { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? DimLength { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? DimWidth { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? DimHeight { get; set; }
        [MaxLength(50)]
        public string UOMDims { get; set; }
        [Column(TypeName = "decimal(16,2)")]
        public decimal? PeakSurcharge { get; set; }
        [MaxLength(50)]
        public string ReservedFutureUse1 { get; set; }
        [MaxLength(50)]
        public string ReservedFutureUse2 { get; set; }
        [MaxLength(50)]
        public string ReservedFutureUse3 { get; set; }
        [MaxLength(50)]
        public string ReservedFutureUse4 { get; set; }
        [MaxLength(50)]
        public string ReservedFutureUse5 { get; set; }
        public virtual InvoicedOrders GeneralInvoice { get; set; }
    }
}
