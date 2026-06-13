using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebApp.Models
{
    public class Integration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string AccessToken { get; set; }

        [StringLength(100)]
        public string StoreName { get; set; }

        [StringLength(100)]
        public string WebhookUrl { get; set; }

        [StringLength(100)]
        public string APIKey { get; set; }

        [StringLength(100)]
        public string APISecret { get; set; }

        [StringLength(500)]
        public string Scopes { get; set; }
    }
}
