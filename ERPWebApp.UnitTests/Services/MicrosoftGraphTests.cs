// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Test.Requests.Functional
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Kiota.Abstractions.Authentication;

    public class GraphTestBase
    {
        protected static GraphServiceClient graphClient = null;

        public GraphTestBase()
        {
            GetAuthenticatedClient();
        }

        // Get an access token and provide a GraphServiceClient.
        private void GetAuthenticatedClient()
        {
            if (graphClient == null)
            {
                // Create Microsoft Graph client.
                try
                {
                    graphClient = new GraphServiceClient(new BaseBearerTokenAuthenticationProvider(new GraphTokenProvider()));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Could not create a graph client: " + ex.Message);
                }
            }
        }

    }

    public class GraphTokenProvider : IAccessTokenProvider
    {
        private readonly string clientId;
        private readonly string userName;
        private readonly string password;
        // Don't use password grant in your apps. Only use for legacy solutions and automated testing.
        private readonly string grantType = "password";
        private readonly string tokenEndpoint = "https://login.microsoftonline.com/common/oauth2/token";
        private readonly string resourceId = "https%3A%2F%2Fgraph.microsoft.com%2F";

        private readonly string contentType = "application/x-www-form-urlencoded";
        private static string accessToken = null;
        private static string tokenForUser = null;
        private static System.DateTimeOffset expiration;

        public GraphTokenProvider()
        {
            // Setup for CI
            clientId = System.Environment.GetEnvironmentVariable("test_client_id");
            userName = System.Environment.GetEnvironmentVariable("test_user_name");
            password = System.Environment.GetEnvironmentVariable("test_password");
        }
        public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> additionalAuthenticationContext = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            JObject jResult = null;
            string urlParameters = string.Format(
                    "grant_type={0}&resource={1}&client_id={2}&username={3}&password={4}",
                    grantType,
                    resourceId,
                    clientId,
                    userName,
                    password
            );

            var client = new HttpClient();
            var createBody = new StringContent(urlParameters, System.Text.Encoding.UTF8, contentType);

            HttpResponseMessage response = await client.PostAsync(tokenEndpoint, createBody);

            if (response.IsSuccessStatusCode)
            {
                Task<string> responseTask = response.Content.ReadAsStringAsync();
                responseTask.Wait();
                string responseContent = responseTask.Result;
                jResult = JObject.Parse(responseContent);
                accessToken = (string)jResult["access_token"];
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                //Set AuthenticationHelper values so that the regular MSAL auth flow won't be triggered.
                tokenForUser = accessToken;
                expiration = DateTimeOffset.UtcNow.AddHours(5);
            }

            return accessToken;
        }

        public AllowedHostsValidator AllowedHostsValidator
        {
            get;
        }
    }
}

// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Test.Requests.Functional
{
    using Microsoft.Graph.Models;
    using Microsoft.Graph.Me.SendMail;
    using System;
    using System.Collections.Generic;
    using Xunit;
    using Microsoft.Kiota.Abstractions;
    using System.Globalization;

    public class MailTests : GraphTestBase
    {
        public async System.Threading.Tasks.Task<Message> createEmail(string emailBody)
        {
            // Get the test user.
            var me = await graphClient.Me.GetAsync();

            var subject = DateTime.Now.ToString(CultureInfo.InvariantCulture);

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    Content = emailBody
                },
                ToRecipients =
                [
                    new()
                    {
                        EmailAddress = new EmailAddress()
                        {
                            Address = me.Mail
                        }
                    }
                ]
            };

            return message;
        }

        // Tests the SendMail action.
        [Fact]
        public async System.Threading.Tasks.Task MailSendMail()
        {
            try
            {
                var message = await createEmail("Sent from the MailSendMail test.");

                var sendMailBody = new SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true
                };
                // Send email to the test user.
                await graphClient.Me.SendMail.PostAsync(sendMailBody);

                // Check the we found the sent email in the sent items folder.
                var mailFolderMessagesCollectionPage = await graphClient.Me.MailFolders["sentitems"].Messages.GetAsync(requestConfiguration => requestConfiguration.QueryParameters.Filter = "Subject eq '" + message.Subject + "'");

                Assert.NotNull(mailFolderMessagesCollectionPage);
            }
            catch (ApiException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: " + e.Message);
            }
        }

        [Fact(Skip = "No CI set up for functional tests")]
        public async System.Threading.Tasks.Task MailGetMailWithFileAttachment()
        {
            try
            {
                // Find messages with attachments.
                var messageCollection = await graphClient.Me.Messages.GetAsync(requestConfiguration => requestConfiguration.QueryParameters.Filter = "hasAttachments eq true");

                if (messageCollection.Value.Count > 0)
                {
                    // Get information about attachments on the first message that has attachments.
                    var attachments = await graphClient.Me.Messages[messageCollection.Value[0].Id]
                                                          .Attachments
                                                          .GetAsync();

                    // Get an attachment.
                    var attachmment = await graphClient.Me.Messages[messageCollection.Value[0].Id]
                                                          .Attachments[attachments.Value[0].Id]
                                                          .GetAsync();

                    if (attachmment is FileAttachment)
                    {
                        Assert.NotNull((attachmment as FileAttachment).ContentBytes);
                    }
                }
            }
            catch (ApiException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: " + e.Message);
            }
        }

        [Fact(Skip = "No CI set up for functional tests")]
        public async System.Threading.Tasks.Task MailNextPageRequest()
        {
            try
            {
                var messages = new List<Message>();

                var messagePage = await graphClient.Me.Messages.GetAsync();

                messages.AddRange(messagePage.Value);

                while (messagePage.OdataNextLink != null)
                {
                    messagePage = await new Me.Messages.MessagesRequestBuilder(messagePage.OdataNextLink, graphClient.RequestAdapter).GetAsync();
                    messages.AddRange(messagePage.Value);
                }
            }
            catch (ApiException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: " + e.Message);
            }
        }
    }
}
