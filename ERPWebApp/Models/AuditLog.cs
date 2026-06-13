using System.ComponentModel.DataAnnotations;

namespace ERPWebApp.Models
{
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string BusinessEntity { get; set; }
        public string PropertyName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
