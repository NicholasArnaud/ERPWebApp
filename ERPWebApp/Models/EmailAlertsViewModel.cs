using ERPWebApp.Models.Mappings;

namespace ERPWebApp.Models
{
    public class EmailAlertsViewModel
    {
        public List<ScheduledEmailViewModel> ScheduledEmails { get; set; }
        public List<string> UserEmails { get; set; }
        public List<AlertTriggerTemplateMappings> AlertTriggerTemplateMappings { get; set; }
    }

    public class ScheduledEmailViewModel
    {
        public int EmailAlertId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<string> Recipients { get; set; }
        public string RecipientEmail { get; set; }
        public DateTime ScheduledTime { get; set; }
        public TimeOnly ScheduledTimeOnly { get; set; }
        // So whenever the object is constructed, it's going to take the ScheduledTime, and it's going to set the ScheduledTimeOnly to the ScheduledTimes Time value.
        public bool IsActive { get; set; }
        public Timer Timer { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public AlertType AlertType { get; set; }
        public Frequency? Frequency { get; set; }

        public int? AlertTemplateId { get; set; }
    }
    public class CreateEmailAlertViewModel
    {
        public EmailAlert EmailAlert { get; set; }
        public List<string> Recipients { get; set; }
        public int? AlertTemplateId { get; set; }
    }

}