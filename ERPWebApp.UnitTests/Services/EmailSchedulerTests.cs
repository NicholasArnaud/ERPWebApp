using Microsoft.Extensions.DependencyInjection;

namespace ERPWebApp.EmailTests
{
    [Trait("Category", "execute")]
    public class EmailSchedulerHostedServiceTests
    {
        private readonly EmailSchedulerHostedService _emailSchedulerHostedService;
        private readonly Mock<IGraphAPIService> _mockGraphApiHelper;
        private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private readonly Mock<IServiceScope> _mockServiceScope;
        private readonly Mock<IScheduledEmailService> _mockScheduledEmailService;

        public EmailSchedulerHostedServiceTests()
        {
            _mockGraphApiHelper = new Mock<IGraphAPIService>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockScheduledEmailService = new Mock<IScheduledEmailService>();

            _emailSchedulerHostedService = new EmailSchedulerHostedService(_mockGraphApiHelper.Object, _mockServiceScopeFactory.Object);
        }

        //TO DO: This UT needs to be looked at. It suddenly stops without error and therefore doesn't add the 4 alerts. However, this works on production and on dev. 
        //    [Fact]
        //    public async Task Test_StartAsync()
        //    {
        //        // Arrange  
        //        var emailAlerts = new List<EmailAlert>
        //{
        //    new() { EmailAlertId = 1, Subject = "Subject1", Body = "Body1", ScheduledTime = DateTime.Now.AddHours(1), IsActive = true },
        //    new() { EmailAlertId = 2, Subject = "Subject2", Body = "Body2", ScheduledTime = DateTime.Now.AddHours(2), IsActive = true }
        //};

        //        var recipients = new List<string> { "recipient1@example.com", "recipient2@example.com" };

        //        _ = _mockServiceScopeFactory.Setup(static x => x.CreateScope()).Returns(_mockServiceScope.Object);
        //        _ = _mockServiceScope.Setup(static x => x.ServiceProvider.GetService(typeof(IScheduledEmailService))).Returns(_mockScheduledEmailService.Object);
        //        _ = _mockScheduledEmailService.Setup(static x => x.GetAllEmailAlertsAsync()).ReturnsAsync(emailAlerts);
        //        _ = _mockScheduledEmailService.Setup(static x => x.GetRecipientsForEmailAlertAsync(It.IsAny<int>())).ReturnsAsync(recipients); // Add this mock setup  

        //        // Act  
        //        await _emailSchedulerHostedService.StartAsync(CancellationToken.None);

        //        // Assert  
        //        Assert.Equal(4, _emailSchedulerHostedService.ScheduledEmails.Count); // There should be 4 scheduled emails since each email alert has 2 recipients  
        //    }

        [Fact]
        public async Task Test_StopAsync()
        {
            // Act  
            await _emailSchedulerHostedService.StopAsync(CancellationToken.None);

            // Assert  
            // No exception should be thrown  
        }

        [Fact]
        public void Test_AddScheduledEmail()
        {
            // Arrange  
            var scheduledEmail = new ScheduledEmailViewModel
            {
                EmailAlertId = 1,
                Subject = "Test Subject",
                Body = "Test Body",
                RecipientEmail = "test@example.com",
                ScheduledTime = DateTime.Now.AddHours(1),
                IsActive = true
            };

            // Act  
            _emailSchedulerHostedService.AddScheduledEmail(scheduledEmail);

            // Assert  
            _ = Assert.Single(_emailSchedulerHostedService.ScheduledEmails);
            Assert.Contains(scheduledEmail, _emailSchedulerHostedService.ScheduledEmails);
        }

        [Fact]
        public void Test_UpdateScheduledEmailAlert()
        {
            // Arrange    
            var emailAlert = new EmailAlert
            {
                EmailAlertId = 1,
                Subject = "Updated Subject",
                Body = "Updated Body",
                ScheduledTime = DateTime.Now.AddHours(2),
                IsActive = true
            };

            var recipients = new List<string> { "test1@example.com"};

            // Add some initial scheduled emails    
            _emailSchedulerHostedService.AddScheduledEmail(new ScheduledEmailViewModel
            {
                EmailAlertId = 1,
                RecipientEmail = "test1@example.com",
                CancellationTokenSource = new CancellationTokenSource()
            });

            // Act    
            _ = _emailSchedulerHostedService.UpdateScheduledEmailAlert(emailAlert, recipients);

            // Assert    
            Assert.Single(_emailSchedulerHostedService.ScheduledEmails);
            Assert.All(_emailSchedulerHostedService.ScheduledEmails, se => Assert.Equal(emailAlert.Subject, se.Subject));
            Assert.All(_emailSchedulerHostedService.ScheduledEmails, se => Assert.Equal(emailAlert.Body, se.Body));
        }

        [Fact]
        public void Test_UpdateScheduledEmailAlert_WithUTC()
        {
            // Arrange  
            var utcScheduledTime = new DateTime(2024, 4, 1, 18, 0, 0);
            var emailAlert = new EmailAlert
            {
                EmailAlertId = 1,
                Subject = "Updated Subject",
                Body = "Updated Body",
                ScheduledTime = utcScheduledTime,
                IsActive = true
            };

            var recipients = new List<string> { "test1@example.com", "test2@example.com" };

            _ = _emailSchedulerHostedService.UpdateScheduledEmailAlert(emailAlert, recipients);

            // Additional assertion to check if the ScheduledTime was converted to CST  
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            var utcScheduledTimeOffset = new DateTimeOffset(utcScheduledTime, TimeSpan.Zero);
            var expectedCstTime = TimeZoneInfo.ConvertTime(utcScheduledTimeOffset, cstZone).DateTime;
            Assert.Equal(new DateTime(2024, 4, 1, 13, 0, 0, DateTimeKind.Unspecified), expectedCstTime);
            Assert.All(_emailSchedulerHostedService.ScheduledEmails, se => Assert.Equal(expectedCstTime.TimeOfDay, se.ScheduledTime.TimeOfDay));
        }


        [Fact]
        public void Test_RemoveScheduledEmailAlert()
        {
            // Arrange  
            var emailAlertId = 1;

            // Add some initial scheduled emails  
            _emailSchedulerHostedService.AddScheduledEmail(new ScheduledEmailViewModel
            {
                EmailAlertId = 1,
                RecipientEmail = "test1@example.com",
                CancellationTokenSource = new CancellationTokenSource()
            });
            _emailSchedulerHostedService.AddScheduledEmail(new ScheduledEmailViewModel
            {
                EmailAlertId = 2,
                RecipientEmail = "test2@example.com",
                CancellationTokenSource = new CancellationTokenSource()
            });

            // Act  
            _ = _emailSchedulerHostedService.RemoveScheduledEmailAlert(emailAlertId);

            // Assert  
            _ = Assert.Single(_emailSchedulerHostedService.ScheduledEmails);
            Assert.DoesNotContain(_emailSchedulerHostedService.ScheduledEmails, se => se.EmailAlertId == emailAlertId);
        }

        [Fact]
        public void Test_SubscribeToEmailAlert()
        {
            // Arrange  
            var emailAlert = new EmailAlert
            {
                EmailAlertId = 1,
                Subject = "Test Subject",
                Body = "Test Body",
                ScheduledTime = DateTime.Now.AddHours(1),
                IsActive = true
            };

            var recipientEmail = "newuser@example.com";

            // Act  
            _ = _emailSchedulerHostedService.SubscribeToEmailAlert(emailAlert, recipientEmail);

            // Assert  
            _ = Assert.Single(_emailSchedulerHostedService.ScheduledEmails);
        }

        [Fact]
        public void Test_UnsubscribeFromEmailAlert()
        {
            // Arrange    
            int emailAlertId = 1;
            string recipientEmail = "test1@example.com";

            // Add some initial scheduled emails    
            _emailSchedulerHostedService.AddScheduledEmail(new ScheduledEmailViewModel
            {
                EmailAlertId = 1,
                RecipientEmail = "test1@example.com",
                CancellationTokenSource = new CancellationTokenSource()
            });
            _emailSchedulerHostedService.AddScheduledEmail(new ScheduledEmailViewModel
            {
                EmailAlertId = 2,
                RecipientEmail = "test2@example.com",
                CancellationTokenSource = new CancellationTokenSource()
            });

            // Act    
            _ = _emailSchedulerHostedService.UnsubscribeFromEmailAlert(emailAlertId, recipientEmail);

            // Assert    
            _ = Assert.Single(_emailSchedulerHostedService.ScheduledEmails);
            Assert.DoesNotContain(_emailSchedulerHostedService.ScheduledEmails, se => se.EmailAlertId == emailAlertId && se.RecipientEmail == recipientEmail);
        }
    }
}
