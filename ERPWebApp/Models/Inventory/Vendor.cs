using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models.Inventory
{
    [Index(nameof(VendorName), IsUnique = true)]
    public class Vendor
    {
        [Key]
        public int VendorId { get; set; }

        [Required]
        [Display(Name = "Vendor Number")]
        public required string VendorNumber { get; set; }

        [Required]
        [Display(Name = "Vendor Name")]
        public required string VendorName { get; set; }
        public string Notes { get; set; }

        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Business Email")]
        public string BusinessEmail { get; set; }
        public string Fax { get; set; }
        public string Website { get; set; }

        [Display(Name = "Address 1")]
        public string Address1 { get; set; }

        [Display(Name = "Address 2")]
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
        public string Country { get; set; }

        [Display(Name = "Last Modified")]
        public DateTime LastModified { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "External Vendor")]
        [DefaultValue(false)]
        public bool IsExternal { get; set; }
    }
}
