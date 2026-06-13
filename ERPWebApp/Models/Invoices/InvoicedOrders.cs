using ERPWebApp.Models.Orders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models.Invoices
{
    public enum Carrier
    {
        StampsUSPS,
        DHL,
        UPS,
        EasyPost
    }

    public class InvoicedOrders
    {
        [Key]
        public int Id { get; set; }
        public DateTime? DateInvoiced { get; set; }
        [MaxLength(50)]
        public string OrderNumber { get; set; }
        public Carrier OrderCarrier { get; set; }
        public int? ERPOrderId { get; set; }
        [ForeignKey(nameof(ERPOrderId))]
        public virtual Order Orders { get; set; }
        [MaxLength(50)]
        public string TrackingNumber { get; set; }
        public int? EasyPostInvoiceId { get; set; }
        [ForeignKey(nameof(EasyPostInvoiceId))]
        public virtual EasyPostInvoices EasyPostInvoice { get; set; }
        public int? DHLInvoiceId { get; set; }
        [ForeignKey(nameof(DHLInvoiceId))]
        public virtual DHLInvoices DHLInvoice { get; set; }
        public int? UPSInvoiceId { get; set; }
        [ForeignKey(nameof(UPSInvoiceId))]
        public virtual UPSInvoices UPSInvoice { get; set; }
        public int? StampsUSPSInvoiceId { get; set; }
        [ForeignKey(nameof(StampsUSPSInvoiceId))]
        public virtual StampsUSPSInvoices StampsUSPSInvoice { get; set; }
    }
}
