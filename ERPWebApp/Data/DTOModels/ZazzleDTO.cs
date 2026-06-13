using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace ERPWebApp.Data.DTOModels
{
    public class ZazzleDTO
    {
        public string VendorId { get; set; }
        public ZazzleRequest zazzleRequest { get; set; }
        public class ZazzleRequest()
        {
            [XmlElement("Response")]
            public Response response { get; set; }
            public class Response
            {
                [XmlElement("Status")]
                public ResponseStatus responseStatus { get; set; }
                [XmlElement("Result")]
                public Result result { get; set; }
                public class ResponseStatus
                {
                    [XmlElement("Code")]
                    public ResponseCode code { get; set; }
                    public enum ResponseCode
                    {
                        [XmlEnum("SUCCESS")]
                        success,
                        [XmlEnum("ERROR")]
                        error
                    }


                    public string Info { get; set; }
                }
                public class Result
                {
                    public List<ZazzleOrder> Orders { get; set; }
                    public ZazzleShippingInfo ShippingInfo { get; set; }
                    [XmlType("Order")]
                    public class ZazzleOrder
                    {
                        public long OrderId { get; set; }
                        public DateTime OrderDate { get; set; }
                        public DateTime ShipByDate { get; set; }
                        public string OrderingEntity { get; set; }
                        public string OrderType { get; set; }
                        public string DeliveryMethod { get; set; }
                        [XmlElement("Priority")]
                        public Priority priority { get; set; }
                        public enum Priority
                        {
                            [XmlEnum("Low")]
                            low,
                            [XmlEnum("Normal")]
                            normal,
                            [XmlEnum("High")]
                            high,
                            [XmlEnum("Zip")]
                            zip,
                            [XmlEnum("ZipPlus")]
                            zipplus
                        }
                        [MaxLength(3)]
                        public string Currency { get; set; }

                        [XmlElement("Status")]

                        public OrderStatus orderStatus { get; set; }

                        public enum OrderStatus
                        {
                            [XmlEnum("ASSIGNED")]
                            assigned,
                            [XmlEnum("ACCEPTED")]
                            accepted,
                            [XmlEnum("SHIPPED")]
                            shipped,
                            [XmlEnum("CANCELLED")]
                            cancelled,
                            [XmlEnum("CANCELINPROGRESS")]
                            cancelinprogress
                        }
                        [XmlAttribute("Attributes")]
                        public string OrderAttributes { get; set; }

                        //public Order.OrderShippingInfo ShippingAddress { get; set; }
                        public List<ZazzleLineItem> LineItems { get; set; }
                        [XmlType("LineItem")]
                        public class ZazzleLineItem
                        {
                            public long LineItemId { get; set; }
                            public long OrderId { get; set; }
                            public string LineItemType { get; set; }
                            public int Quantity { get; set; }
                            public int NumPerPack { get; set; }
                            public string Description { get; set; }
                            [XmlElement("Attributes")]
                            public string LineItemAttributes { get; set; }
                            public string ReprintInstructions { get; set; }
                            public string VendorAttributes { get; set; }
                            public long ProductId { get; set; }
                            public List<PrintFile> PrintFiles { get; set; }
                            public class PrintFile
                            {
                                [XmlElement("Type")]
                                public FilesType PrintFilesType { get; set; }

                                [XmlElement("Url")]
                                public string PrintFilesUrl { get; set; }
                                [XmlElement("Description")]
                                public string PrintFilesDescription { get; set; }
                            }
                            public List<PreviewFile> Previews { get; set; }
                            public class PreviewFile
                            {
                                [XmlElement("Type")]
                                public FilesType PreviewType { get; set; }
                                [XmlElement("Url")]
                                public string PreviewUrl { get; set; }
                                [XmlElement("Description")]
                                public string PreviewDescription { get; set; }
                            }
                            public List<ZazzleProduct> Products { get; set; }
                            [XmlType("Product")]
                            public class ZazzleProduct
                            {
                                public int ProductId { get; set; }
                                public string ProductInfo { get; set; }
                            }

                        }
                        [XmlElement("Reprint")]
                        public Reprint reprint { get; set; }
                        public class Reprint
                        {
                            public string ReprintReason { get; set; }
                            public long OriginalOrderId { get; set; }
                        }
                        [XmlElement("PackingSheet")]
                        public PackingSheet packingSheet { get; set; }
                        public class PackingSheet
                        {
                            [XmlElement("Page")]
                            public Page page { get; set; }
                            public class Page()
                            {
                                public int PageNumber { get; set; }
                                [XmlElement("Front")]
                                public Front front { get; set; }
                                public class Front()
                                {
                                    [XmlElement("Type")]
                                    public FilesType FrontType { get; set; }
                                    public string Description { get; set; }
                                    public string Url { get; set; }
                                }

                            }
                        }

                        [XmlElement("Update")]
                        public Update update { get; set; }
                        public class Update
                        {
                            [XmlElement("UpdateType")]
                            public UpdateType updateType { get; set; }
                            public string UpdateDate { get; set; }
                            public enum UpdateType
                            {
                                ShippingInfo,
                                Priority,
                                ProductInfo,
                                Remove
                            }
                        }
                        public enum FilesType
                        {
                            [XmlEnum("image/jpg")]
                            jpg,
                            [XmlEnum("image/jpeg")]
                            jpeg,
                            [XmlEnum("image/png")]
                            png,
                            [XmlEnum("application/pdf")]
                            pdf
                        }
                    }
                    public class ZazzleShippingInfo
                    {
                        public string Carrier { get; set; }
                        public string Method { get; set; }
                        public string TrackingNumber { get; set; }
                        public string Weight { get; set; }
                        public List<ZazzleShippingDocument> ShippingDocuments { get; set; }
                        [XmlType("ShippingDocument")]
                        public class ZazzleShippingDocument
                        {
                            [XmlElement("Type")]
                            public ShippingDocumentType type { get; set; }
                            public string Format { get; set; }
                            public string Url { get; set; }
                            public enum ShippingDocumentType
                            {
                                Label,
                                CommercialInvoice
                            }
                        }
                    }
                }

            }
        }
    }
}
