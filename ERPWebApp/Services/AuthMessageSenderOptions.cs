namespace ERPWebApp.Services
{
    public class AuthMessageSenderOptions
    {
        public string SendGridUser { get; set; }
        public string SendGridKey { get; set; }
        public string TwilioUser { get; set; }
        public string TwilioKey { get; set; }
    }
}
