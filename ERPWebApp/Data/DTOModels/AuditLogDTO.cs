using System.ComponentModel;

namespace ERPWebApp.Data.DTOModels
{
    public class AuditLogDTO
    {
        public Guid Id { get; set; }

        [DisplayName("Modified at")]
        public DateTime Timestamp { get; set; }

        [DisplayName("Modified by")]
        public string User { get; set; }

        [DisplayName("Business Entity")]
        public string BusinessEntity { get; set; }

        [DisplayName("Property")]
        public string PropertyName { get; set; }

        [DisplayName("Old Value")]
        public string OldValue { get; set; }

        [DisplayName("New Value")]
        public string NewValue { get; set; }
    }
}
