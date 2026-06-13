using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Invoices
{
    public class UPSInvoices
    {
        [Key]
        public int UPSInvoiceId { get; set; }
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
        [MaxLength(50)]
        public string CustomerNumber { get; set; }
        [MaxLength(50)]
        public string InvoiceNumber { get; set; }
        [MaxLength(50)]
        public string LineOfBusiness { get; set; }
        [MaxLength(50)]
        public string AirbillNumber { get; set; }
        [MaxLength(50)]
        public DateTime? ShipDate { get; set; }
        [MaxLength(50)]
        public string ProNumber { get; set; }
        [MaxLength(50)]
        public string BolNumber { get; set; }
        [MaxLength(50)]
        public string Scac { get; set; }
        [MaxLength(50)]
        public string BillType { get; set; }
        [MaxLength(50)]
        public string ShippersName { get; set; }
        [MaxLength(50)]
        public string ShippersAddress1 { get; set; }
        [MaxLength(50)]
        public string ShippersAddress2 { get; set; }
        [MaxLength(50)]
        public string ShippersAddress3 { get; set; }
        [MaxLength(50)]
        public string ShippersCity { get; set; }
        [MaxLength(50)]
        public string ShippersState { get; set; }
        [MaxLength(50)]
        public string ShippersZip { get; set; }
        [MaxLength(50)]
        public string ReceiverName { get; set; }
        [MaxLength(50)]
        public string ReceiverAddress1 { get; set; }
        [MaxLength(50)]
        public string ReceiverAddress2 { get; set; }
        [MaxLength(50)]
        public string ReceiverAddress3 { get; set; }
        [MaxLength(50)]
        public string ReceiverCity { get; set; }
        [MaxLength(50)]
        public string ReceiverState { get; set; }
        [MaxLength(50)]
        public string ReceiverZip { get; set; }
        [MaxLength(50)]
        public string ConsigneeName { get; set; }
        [MaxLength(50)]
        public string ConsigneeCity { get; set; }
        [MaxLength(50)]
        public string ConsigneeState { get; set; }
        [MaxLength(50)]
        public string ConsigneeZip { get; set; }
        [MaxLength(50)]
        public string OriginatingCustomer { get; set; }
        [MaxLength(50)]
        public string CustomerName { get; set; }
        [MaxLength(50)]
        public string CustomerAddress1 { get; set; }
        [MaxLength(50)]
        public string CustomerAddress2 { get; set; }
        [MaxLength(50)]
        public string CustomerCity { get; set; }
        [MaxLength(50)]
        public string CustomerState { get; set; }
        public int? CustomerZip { get; set; }
        public int? HandlingUnit { get; set; }
        [MaxLength(50)]
        public string Pieces { get; set; }
        [MaxLength(50)]
        public string OriginalWeight { get; set; }
        [MaxLength(50)]
        public string ChargedWeight { get; set; }
        public int? Class { get; set; }
        [MaxLength(50)]
        public string ChargeType1 { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? ChargeAmount1 { get; set; }
        [MaxLength(50)]
        public string ChargeType2 { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? ChargeAmount2 { get; set; }
        [MaxLength(50)]
        public string ChargeType3 { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? ChargeAmount3 { get; set; }
        [MaxLength(50)]
        public string ChargeType4 { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? ChargeAmount4 { get; set; }
        [MaxLength(50)]
        public string ChargeType5 { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? ChargeAmount5 { get; set; }
        [MaxLength(50)]
        public string ChargeType6 { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? ChargeAmount6 { get; set; }
        [MaxLength(50)]
        public string ChargeType7 { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? ChargeAmount7 { get; set; }
        [MaxLength(50)]
        public string ChargeType8 { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? ChargeAmount8 { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? ChargeTotal { get; set; }
        public DateTime? InvoiceDate { get; set; }
        [MaxLength(50)]
        public string BillingReference1 { get; set; }
        [MaxLength(50)]
        public string BillingReference2 { get; set; }
        [MaxLength(50)]
        public string VendorReference1 { get; set; }
        [MaxLength(50)]
        public string VendorReference2 { get; set; }
        [MaxLength(50)]
        public string SentBy { get; set; }
        [MaxLength(50)]
        public string ServiceLevel { get; set; }
        public int? Zone { get; set; }
        [Column(TypeName = "decimal(16,4)")]
        public decimal? YouOweAs { get; set; }
        [MaxLength(50)]
        public string Description1 { get; set; }
        [MaxLength(50)]
        public string Description2 { get; set; }
        [MaxLength(50)]
        public string Description3 { get; set; }
        [MaxLength(50)]
        public string Description4 { get; set; }
        [MaxLength(50)]
        public string PickupLocation { get; set; }
        [MaxLength(50)]
        public string SenderNo { get; set; }
        [MaxLength(50)]
        public string ReceiverNo { get; set; }
        [MaxLength(50)]
        public string ReceiverLine1 { get; set; }
        [MaxLength(50)]
        public string ReceiverLine2 { get; set; }
        [MaxLength(50)]
        public string PackageReference1 { get; set; }
        [MaxLength(50)]
        public string PackageReference2 { get; set; }
        [MaxLength(50)]
        public string PackageReference3 { get; set; }
        [MaxLength(50)]
        public string PackageReference4 { get; set; }
        [MaxLength(50)]
        public string PackageReference5 { get; set; }
        [MaxLength(50)]
        public string PackageReference6 { get; set; }
        [MaxLength(50)]
        public string PackageReference7 { get; set; }
        [MaxLength(50)]
        public string PackageReference8 { get; set; }
        [MaxLength(50)]
        public string UpsNumber { get; set; }
        public virtual InvoicedOrders GeneralInvoice { get; set; }
    }
}
