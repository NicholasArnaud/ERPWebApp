namespace ERPWebApp.Services.IServices
{
    public interface IGraphAPIService
    {
        Task<string> GetUserIdByEmail(string userEmail);
        Task SendEmailAlert(string subject, string body, string recipientEmail, string userId, byte[] attachment = null);
    }
}
