using ERPWebApp.Models.Inventory;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERPWebApp.Models.Orders.Order;

namespace ERPWebApp.Models
{
    public class Warehouse
    {
        [Key]
        public int WarehouseId { get; set; } 

        [Required]
        [StringLength(250)]
        [DisplayName("Warehouse Name")]
        public string WarehouseName { get; set; }

        [Required]
        [DisplayName("Default Warehouse")]
        public bool DefaultWarehouse { get; set; }

        [StringLength(250)]  
        public string Company { get; set; }

        [Required]
        [StringLength(100)]  
        public string Country { get; set; }

        [Required]
        [StringLength(100)]  
        [DisplayName("Street Address 1")]
        public string StreetAddress1 { get; set; }

        [StringLength(100)]  
        [DisplayName("Street Address 2")]
        public string StreetAddress2 { get; set; }

        [Required]
        [StringLength(100)]  
        public string City { get; set; }

        [Required]
        [StringLength(100)] 
        public string State { get; set; }

        [Required]
        [StringLength(10)] 
        [DisplayName("Postal Code")]
        public string PostalCode { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]  
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        [StringLength(256)]  
        public string Email { get; set; }

        [Required]
        [StringLength(50)] 
        [DisplayName("Time Zone")]
        public string TimeZone { get; set; }

        [Required]
        [DisplayName("Same As Return Address")]
        public bool SameAsReturnAddress { get; set; }
        public OrderShippingInfo BillingAddress { get; set; } = new OrderShippingInfo();

    }
}
