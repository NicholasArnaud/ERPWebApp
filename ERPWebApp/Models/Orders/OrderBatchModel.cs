using ERPWebApp.Models.Inventory;
using ERPWebApp.Models.Company;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERPWebApp.Models.Orders.Order;
using ERPWebApp.Models.PurchaseOrders;

namespace ERPWebApp.Models.Orders
{
    public class OrderBatch
    {
        [Key]
        public int OrderBatchId { get; set; }

        [Display(Name = "Create Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreateDate { get; set; }

        [Display(Name = "Create By")]
        [StringLength(30)]
        public string CreateBy { get; set; }

        [Display(Name = "Batch Number")]
        [StringLength(50)]
        public string BatchNumber { get; set; }

        [Display(Name = "Status")]
        public OrderBatchStatus Status { get; set; }

        [Display(Name = "Type")]
        public BatchType? Type { get; set; }

        [Display(Name = "Deductible")]
        [DefaultValue(true)]
        public bool IsDeductible { get; set; }

        [Display(Name = "Purchase Order")]
        public int? PurchaseOrderId { get; set; }

        public virtual PurchaseOrder PurchaseOrder { get; set; }

        [Display(Name = "Requires Purchase Order")]
        [DefaultValue(false)]
        public bool RequiresPO { get; set; }
    }

    public enum OrderBatchStatus
    {
        [Display(Name = "Open")]
        Open = 0,
        [Display(Name = "In-Progress")]
        InProgress = 1,
        [Display(Name = "Completed")]
        Completed = 2
    }
    public enum BatchType
    {
        Inventory = 0,
        Design = 1
    }

    public class OrderBatchItem
    {
        [Key]
        public int OrderBatchItemId { get; set; }
        [ForeignKey("OrderBatch")]
        public int OrderBatchId { get; set; }
        [Required]
        public virtual OrderBatch OrderBatch { get; set; }
        [ForeignKey("Order")]
        public int? ERPOrderId { get; set; }
        [Required]
        public virtual Order Order { get; set; }

        public string OrderNumber { get; set; }

        [ForeignKey("ERPOrderItemId")]
        public virtual OrderItem OrderItem { get; set; }

        public int ERPOrderItemId { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }
        [Required]
        public virtual Product Product { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [ForeignKey("BatchItemStatus")]
        [Display(Name = "Item Status")]
        public int BatchItemStatusId { get; set; }
        
        [Required]
        public virtual BatchItemStatus BatchItemStatus { get; set; }

        [Display(Name = "Is Picked")]
        [DefaultValue(false)]
        public bool IsPicked { get; set; }

        [Display(Name = "Is Completed")]
        [DefaultValue(false)]
        public bool IsCompleted { get; set; }
    }

    public class BatchItemStatus
    {
        [Key]
        public int BatchItemStatusId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Status Name")]
        public string StatusName { get; set; }

        [ForeignKey("Department")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required]
        public virtual Department Department { get; set; }

        [Required]
        [Display(Name = "Execution Sequence")]
        public int ExecutionSequence { get; set; }

        [Required]
        [DefaultValue(true)]
        [Display(Name = "Is Deletable")]
        public bool IsDeletable { get; set; }
    }

    public enum ProductStatus
    {
        [Display(Name = "Open")]
        Open = 0,
        [Display(Name = "Picked")]
        Picked = 1,
        [Display(Name = "Designed")]
        Designed = 2,
        [Display(Name = "Completed")]
        Completed = 3
    }
}
