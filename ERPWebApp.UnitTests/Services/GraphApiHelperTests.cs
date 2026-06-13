using Microsoft.Extensions.Configuration;

namespace ERPWebApp.Tests
{
    public class GraphApiHelperTests
    {
        private Mock<IConfiguration> MockConfig()
        {
            //var clientIdMock = _configuration.SetupGet(x => x.GetSection("ClientId"));
            var clientIdMock = new Mock<IConfigurationSection>();
            _ = clientIdMock.SetupGet(static x => x.Value).Returns("39cbd0d1-22f6-4972-8a57-df6efac50b01");

            var configurationMock = new Mock<IConfiguration>();

            _ = configurationMock.Setup(static x => x.GetSection("ClientId")).Returns(clientIdMock.Object);

            return configurationMock;
        }

        [Fact]
        public void Test_ConfigMock()
        {
            // Arrange
            var expectedClientId = "39cbd0d1-22f6-4972-8a57-df6efac50b01";

            // Act
            var configMock = MockConfig();
            var mockClientId = configMock.Object.GetValue<string>("ClientId");
            configMock.Verify(static x => x.GetSection("ClientId"), Times.Once);

            // Assert
            Assert.Equal(expectedClientId, mockClientId);
        }
    }
}
