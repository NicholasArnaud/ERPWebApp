using System.Net.Http.Headers;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using ERPWebApp.Services.IServices;

namespace ERPWebApp.Services
{
    public class GraphAPIService : IGraphAPIService
    {
        private readonly IConfiguration _configuration;
        private readonly string _managementEmail;

        public GraphAPIService(IConfiguration configuration)
        {
            _configuration = configuration;
            _managementEmail = _configuration["ManagementEmail"];
        }

        private async Task<HttpClient> GetAuthenticatedHttpClient()
        {
            try
            {
                var clientId = _configuration["AzureAd:ClientId"];
                var tenantId = _configuration["AzureAd:TenantId"];
                var clientSecret = _configuration["AzureAd:ClientSecret"];

                var authority = $"https://login.microsoftonline.com/{tenantId}";
                var scopes = new[] { "https://graph.microsoft.com/.default" };

                var confidentialClient = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithAuthority(authority)
                    .WithClientSecret(clientSecret)
                    .Build();

                var authResult = await confidentialClient.AcquireTokenForClient(scopes).ExecuteAsync();

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(authResult.TokenType, authResult.AccessToken);
                return httpClient;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAuthenticatedHttpClient: " + ex.Message);
                throw;
            }
        }

        public async Task SendEmailAlert(string subject, string body, string recipientEmail, string userId, byte[] attachment = null)
        {
            try
            {
                var httpClient = await GetAuthenticatedHttpClient();
                var graphServiceClient = new GraphServiceClient(httpClient);

                var message = new Message
                {
                    Subject = subject,
                    Body = new ItemBody { ContentType = BodyType.Html, Content = body },
                    ToRecipients = new List<Recipient> { new Recipient { EmailAddress = new EmailAddress { Address = recipientEmail } } },
                    From = new Recipient { EmailAddress = new EmailAddress { Address = _managementEmail } },
                    Attachments = new List<Attachment>()
                };

                if (attachment != null)
                {
                    message.Attachments.Add(new FileAttachment
                    {
                        OdataType = "#microsoft.graph.fileAttachment",
                        Name = "low_stock_report.csv",
                        ContentType = "text/csv",
                        ContentBytes = attachment
                    });
                }

                var sendMailRequest = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true,
                };
                string managementUserId = await GetUserIdByEmail(_managementEmail);

                await graphServiceClient.Users[managementUserId].SendMail.PostAsync(sendMailRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SendEmailAlert: " + ex.Message);
                throw;
            }
        }

        public async Task<string> GetUserIdByEmail(string userEmail)
        {
            try
            {
                var httpClient = await GetAuthenticatedHttpClient();
                var graphServiceClient = new GraphServiceClient(httpClient);

                var user = await graphServiceClient.Users[userEmail].GetAsync();
                return user.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetUserIdByEmail: " + ex.Message);
                throw;
            }
        }
    }
}