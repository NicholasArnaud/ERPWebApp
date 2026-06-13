using System.Text;
using ERPWebApp.Data.DTOModels.ShipEngineDtos;
using ERPWebApp.Models;
using ERPWebApp.Models.Shipping;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using Moq.Protected;

namespace ERPWebApp.UnitTests.Services;
[Trait("Category", "execute")]
public class ShippingScanoutServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IWebhooks> _mockWebhooks;
    private readonly Mock<ILogger<ShippingScanoutService>> _mockLogger;
    private readonly Mock<IWebhookBatchService> _mockWebhookBatchService;
    private readonly Mock<IFilesService> _mockFilesService;
    private readonly ShippingScanoutService _service;

    public ShippingScanoutServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockWebhooks = new Mock<IWebhooks>();
        _mockLogger = new Mock<ILogger<ShippingScanoutService>>();
        _mockWebhookBatchService = new Mock<IWebhookBatchService>();
        _mockFilesService = new Mock<IFilesService>();

        _service = new ShippingScanoutService(
            _mockUnitOfWork.Object,
            _mockHttpClientFactory.Object,
            _mockWebhooks.Object,
            _mockLogger.Object,
            _mockWebhookBatchService.Object,
            _mockFilesService.Object
        );
    }
    [Fact]
    public async Task GenerateUSPSManifest_WithValidDataInFile_ReturnsSuccessResponse()
    {
        // Arrange
        var warehouse = new ShipEngineWarehouse(
            WarehouseId: "test-warehouse",
            IsDefault: true,
            WarehouseName: "Test Warehouse",
            CreatedAt: DateTime.UtcNow,
            OriginAddress: new ShipEngineWarehouseAddress(
                Name: "Test Warehouse",
                Phone: "123-456-7890",
                Email: "test@example.com",
                CompanyName: "Test Company",
                AddressLine1: "123 Test St",
                AddressLine2: "",
                AddressLine3: "",
                City: "Test City",
                State: "CA",
                PostalCode: "90210",
                CountryCode: "US",
                AddressResidentialIndicator: "no"
            ),
            ReturnAddress: null
        );
        var mailingDate = "2024-03-20";
        var trackingNumbers = new List<string> { "9405511298370938473829" };

        _mockUnitOfWork.Setup(u => u.ShippingScanouts.GetScannedUspsTrackingNumbersAsync(
            It.Is<(string name, string state, string postalCode)>(
                x => x.name == warehouse.OriginAddress.Name &&
                     x.state == warehouse.OriginAddress.State &&
                     x.postalCode == warehouse.OriginAddress.PostalCode
            )
        )).ReturnsAsync((trackingNumbers, trackingNumbers.Count));
        string responseContent = await File.ReadAllTextAsync("../../../Fixtures/files/USPSScan-FormResponse.txt");
        var content = new StringContent(responseContent);
        content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data")
        {
            Parameters = { new NameValueHeaderValue("boundary", "nNQY4NpbqMONoND5wahpXAvB") }
        };

            var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = content
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var client = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api-tem.usps.com/")
        };
        _mockHttpClientFactory.Setup(x => x.CreateClient("USPS")).Returns(client);

        // Setup file service mock to return a test file path
        string expectedFilePath = "manifests/test.pdf";
        _mockFilesService.Setup(x => x.UploadToAzureAsync(
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            FileType.ShippingManifests,
            "pdf-container"
        )).ReturnsAsync(expectedFilePath);

        // Setup ShippingManifest repository mock
        var mockShippingManifestRepo = new Mock<IShippingManifestRepository>();
        mockShippingManifestRepo.Setup(x => x.AddRangeAsync(It.IsAny<List<ShippingManifest>>()))
            .Returns(Task.FromResult<List<ShippingManifest>>(new List<ShippingManifest>()));
        _mockUnitOfWork.Setup(x => x.ShippingManifest).Returns(mockShippingManifestRepo.Object);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.GenerateUspsManifest(warehouse);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Manifest created", result.Message);
        Assert.NotNull(result.Manifests);
        Assert.Contains("application/json", result.Manifests.Keys);
        Assert.Contains("application/pdf", result.Manifests.Keys);
    }
    [Fact]
    public async Task GenerateUspsManifest_WithValidData_ReturnsSuccessResponse()
    {
        // Arrange
        var warehouse = new ShipEngineWarehouse(
            WarehouseId: "test-warehouse",
            IsDefault: true,
            WarehouseName: "Test Warehouse",
            CreatedAt: DateTime.UtcNow,
            OriginAddress: new ShipEngineWarehouseAddress(
                Name: "Test Warehouse",
                Phone: "123-456-7890",
                Email: "test@example.com",
                CompanyName: "Test Company",
                AddressLine1: "123 Test St",
                AddressLine2: "",
                AddressLine3: "",
                City: "Test City",
                State: "CA",
                PostalCode: "90210",
                CountryCode: "US",
                AddressResidentialIndicator: "no"
            ),
            ReturnAddress: null
        );
        var mailingDate = "2024-03-20";
        var trackingNumbers = new List<string> { "9405511298370938473829" };

        _mockUnitOfWork.Setup(u => u.ShippingScanouts.GetScannedUspsTrackingNumbersAsync(
            It.Is<(string name, string state, string postalCode)>(
                x => x.name == warehouse.OriginAddress.Name &&
                     x.state == warehouse.OriginAddress.State &&
                     x.postalCode == warehouse.OriginAddress.PostalCode
            )
        )).ReturnsAsync((trackingNumbers, trackingNumbers.Count));

        var boundary = "mzowiqXHz1WiH76KNgYBYiyz";
        var jsonContent = "{\"form\":\"5630\",\"imageType\":\"PDF\",\"labelType\":\"8.5x11LABEL\",\"mailingDate\":\"2025-04-22\",\"overwriteMailingDate\":true,\"entryFacilityZIPCode\":\"70501\",\"destinationEntryFacilityType\":\"NONE\",\"shipment\":{\"trackingNumbers\":[\"9205590382432300050097\"]},\"fromAddress\":{\"firm\":\"CFWAREHOUSE\",\"streetAddress\":\"1229 NW EVANGELINE TRWY\",\"secondaryAddress\":\"\",\"city\":\"Lafayette\",\"state\":\"LA\",\"ZIPCode\":\"70501\",\"ZIPPlus4\":\"3551\",\"ignoreBadAddress\":true},\"manifestNumber\":\"92750903824321000000000111\",\"trackingNumbers\":[\"9205590382432300050097\"]}";
        
        // Create a simple PDF content and base64 encode it
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A, 0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A, 0x31, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x0A, 0x3C, 0x3C, 0x0A, 0x2F, 0x54, 0x79, 0x70, 0x65, 0x20, 0x2F, 0x43, 0x61, 0x74, 0x61, 0x6C, 0x6F, 0x67, 0x0A, 0x2F, 0x50, 0x61, 0x67, 0x65, 0x73, 0x20, 0x32, 0x20, 0x30, 0x20, 0x52, 0x0A, 0x3E, 0x3E, 0x0A, 0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A, 0x0A };
        var pdfContent = Convert.ToBase64String(pdfBytes);

        var multipartContent = new MultipartFormDataContent(boundary);
        
        var jsonPart = new StringContent(jsonContent, Encoding.UTF8);
        jsonPart.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        jsonPart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "SCANFormMetaData"
        };
        
        var pdfPart = new StringContent(pdfContent, Encoding.UTF8);
        pdfPart.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        pdfPart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "SCANFormImage",
            FileName = "SCANFormImage.pdf"
        };
        
        multipartContent.Add(jsonPart);
        multipartContent.Add(pdfPart);

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = multipartContent
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var client = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api-tem.usps.com/")
        };
        _mockHttpClientFactory.Setup(x => x.CreateClient("USPS")).Returns(client);

        // Setup file service mock to return a test file path
        string expectedFilePath = "manifests/test.pdf";
        _mockFilesService.Setup(x => x.UploadToAzureAsync(
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            FileType.ShippingManifests,
            "pdf-container"
        )).ReturnsAsync(expectedFilePath);

        // Setup ShippingManifest repository mock
        var mockShippingManifestRepo = new Mock<IShippingManifestRepository>();
        mockShippingManifestRepo.Setup(x => x.AddRangeAsync(It.IsAny<List<ShippingManifest>>()))
            .Returns(Task.FromResult<List<ShippingManifest>>(new List<ShippingManifest>()));
        _mockUnitOfWork.Setup(x => x.ShippingManifest).Returns(mockShippingManifestRepo.Object);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.GenerateUspsManifest(warehouse);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Manifest created", result.Message);
        Assert.NotNull(result.Manifests);
        Assert.Contains("application/json", result.Manifests.Keys);
        Assert.Contains("application/pdf", result.Manifests.Keys);
        Assert.Equal(jsonContent, result.Manifests["application/json"]);
        Assert.Equal(pdfContent, result.Manifests["application/pdf"]);

        // Verify manifest was saved
        mockShippingManifestRepo.Verify(x => x.AddRangeAsync(
            It.Is<List<ShippingManifest>>(manifests => 
                manifests.Count == 1 && 
                manifests[0].ManifestFile == expectedFilePath &&
                manifests[0].Carrier == "USPS"
            )
        ), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ParseUSPSResponseAsync_WithValidMultipartResponse_ReturnsContentDictionary()
    {
        // Arrange
        var boundary = "mzowiqXHz1WiH76KNgYBYiyz";
        var jsonContent = "{\"form\":\"5630\",\"imageType\":\"PDF\",\"labelType\":\"8.5x11LABEL\",\"mailingDate\":\"2025-04-22\",\"overwriteMailingDate\":true,\"entryFacilityZIPCode\":\"70501\",\"destinationEntryFacilityType\":\"NONE\",\"shipment\":{\"trackingNumbers\":[\"9205590382432300050097\"]},\"fromAddress\":{\"firm\":\"CFWAREHOUSE\",\"streetAddress\":\"1229 NW EVANGELINE TRWY\",\"secondaryAddress\":\"\",\"city\":\"Lafayette\",\"state\":\"LA\",\"ZIPCode\":\"70501\",\"ZIPPlus4\":\"3551\",\"ignoreBadAddress\":true},\"manifestNumber\":\"92750903824321000000000111\",\"trackingNumbers\":[\"9205590382432300050097\"]}";
        
        // Create a simple PDF content and base64 encode it
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A, 0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A, 0x31, 0x20, 0x30, 0x20, 0x6F, 0x62, 0x6A, 0x0A, 0x3C, 0x3C, 0x0A, 0x2F, 0x54, 0x79, 0x70, 0x65, 0x20, 0x2F, 0x43, 0x61, 0x74, 0x61, 0x6C, 0x6F, 0x67, 0x0A, 0x2F, 0x50, 0x61, 0x67, 0x65, 0x73, 0x20, 0x32, 0x20, 0x30, 0x20, 0x52, 0x0A, 0x3E, 0x3E, 0x0A, 0x65, 0x6E, 0x64, 0x6F, 0x62, 0x6A, 0x0A };
        var pdfContent = Convert.ToBase64String(pdfBytes);
        
        var multipartContent = new MultipartFormDataContent(boundary);
        
        var jsonPart = new StringContent(jsonContent, Encoding.UTF8);
        jsonPart.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        jsonPart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "SCANFormMetaData"
        };
        
        var pdfPart = new StringContent(pdfContent, Encoding.UTF8);
        pdfPart.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        pdfPart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "SCANFormImage",
            FileName = "SCANFormImage.pdf"
        };
        
        multipartContent.Add(jsonPart);
        multipartContent.Add(pdfPart);

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = multipartContent
        };

        // Act
        var result = await _service.ParseUSPSResponseAsync(response);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("application/json", result.Keys);
        Assert.Contains("application/pdf", result.Keys);
        Assert.Equal(jsonContent, result["application/json"]);
        Assert.Equal(pdfContent, result["application/pdf"]);
    }

    [Fact]
    public async Task ParseUSPSResponseAsync_WithInvalidContentType_ThrowsException()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("test")
        };
        response.Content.Headers.ContentType = null;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.ParseUSPSResponseAsync(response)
        );
    }

    [Fact]
    public async Task ParseUSPSResponseAsync_WithMissingBoundary_ThrowsException()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new MultipartFormDataContent()
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.ParseUSPSResponseAsync(response)
        );
    }

    [Fact]
    public async Task ParseUSPSResponseAsync_WithRealWorldHeaders_ReturnsContentDictionary()
    {
        // Arrange
        var boundary = "nNQY4NpbqMONoND5wahpXAvB";
        var multipartContent = new StringBuilder();
        
        // Read the JSON content from the file
        var jsonContent = File.ReadAllText("../../../Fixtures/files/USPSScan-FormResponse.txt");
        var jsonLines = jsonContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        var jsonBody = string.Join("\n", jsonLines.Skip(4).TakeWhile(line => !line.StartsWith("--")));
        
        // Add JSON part
        multipartContent.AppendLine($"--{boundary}");
        multipartContent.AppendLine("Content-Type: application/json");
        multipartContent.AppendLine("Content-Disposition: form-data; name=\"SCANFormMetaData\"");
        multipartContent.AppendLine();
        multipartContent.AppendLine(jsonBody);
        
        // Add PDF part
        multipartContent.AppendLine($"--{boundary}");
        multipartContent.AppendLine("Content-Type: application/pdf");
        multipartContent.AppendLine("Content-Disposition: form-data; name=\"SCANFormImage\"; filename=\"SCANFormImage.pdf\"");
        multipartContent.AppendLine();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A };  // %PDF-1.4 header
        multipartContent.AppendLine(Convert.ToBase64String(pdfBytes));
        
        // Add final boundary
        multipartContent.AppendLine($"--{boundary}--");

        var content = new StringContent(multipartContent.ToString());
        content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
        content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", boundary));

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = content
        };

        // Add real-world headers
        response.Headers.Add("x-amzn-RequestId", "c635f5c6-18b3-4afc-94ce-a0c5080ceb04");
        response.Headers.Add("x-request-id", "cd2bec51-b00f-4c5e-9525-a92032f2688e");
        response.Headers.Add("x-amz-apigw-id", "JI3EOEfUoAMEFdQ=");
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Accept, Content-Type, Authorization");
        response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
        response.Headers.Add("Access-Control-Max-Age", "3628800");
        response.Headers.Add("Access-Control-Allow-Credentials", "true");
        response.Headers.Add("Cache-Control", "max-age=0, no-cache, no-store");

        // Act
        var result = await _service.ParseUSPSResponseAsync(response);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("application/json", result.Keys);
        Assert.Contains("application/pdf", result.Keys);
        Assert.Contains("\"form\":\"5630\"", result["application/json"]);
        Assert.StartsWith("JVBERi0", result["application/pdf"]); // PDF magic number in base64
    }

    [Fact]
    public async Task GenerateUspsManifest_WithValidDataInFile_ReturnsSuccessResponseAndUploadsFile()
    {
        // Arrange
        var warehouse = new ShipEngineWarehouse(
            WarehouseId: "test-warehouse",
            IsDefault: true,
            WarehouseName: "Test Warehouse",
            CreatedAt: DateTime.UtcNow,
            OriginAddress: new ShipEngineWarehouseAddress(
                Name: "Test Warehouse",
                Phone: "123-456-7890",
                Email: "test@example.com",
                CompanyName: "Test Company",
                AddressLine1: "123 Test St",
                AddressLine2: "",
                AddressLine3: "",
                City: "Test City",
                State: "CA",
                PostalCode: "90210",
                CountryCode: "US",
                AddressResidentialIndicator: "no"
            ),
            ReturnAddress: null
        );
        var mailingDate = "2024-03-20";
        var trackingNumbers = new List<string> { "9405511298370938473829" };

        _mockUnitOfWork.Setup(u => u.ShippingScanouts.GetScannedUspsTrackingNumbersAsync(
            It.Is<(string name, string state, string postalCode)>(
                x => x.name == warehouse.OriginAddress.Name &&
                     x.state == warehouse.OriginAddress.State &&
                     x.postalCode == warehouse.OriginAddress.PostalCode
            )
        )).ReturnsAsync((trackingNumbers, trackingNumbers.Count));

        string responseContent = await File.ReadAllTextAsync("../../../Fixtures/files/USPSScan-FormResponse.txt");
        var content = new StringContent(responseContent);
        content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data")
        {
            Parameters = { new NameValueHeaderValue("boundary", "nNQY4NpbqMONoND5wahpXAvB") }
        };

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = content
        };

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var client = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api-tem.usps.com/")
        };
        _mockHttpClientFactory.Setup(x => x.CreateClient("USPS")).Returns(client);

        // Setup file service mock to return a test file path
        string expectedFilePath = "manifests/test.pdf";
        _mockFilesService.Setup(x => x.UploadToAzureAsync(
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            FileType.ShippingManifests,
            "pdf-container"
        )).ReturnsAsync(expectedFilePath);

        // Setup ShippingManifest repository mock
        var mockShippingManifestRepo = new Mock<IShippingManifestRepository>();
        mockShippingManifestRepo.Setup(x => x.AddRangeAsync(It.IsAny<List<ShippingManifest>>()))
            .Returns(Task.FromResult<List<ShippingManifest>>(new List<ShippingManifest>()));
        _mockUnitOfWork.Setup(x => x.ShippingManifest).Returns(mockShippingManifestRepo.Object);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.GenerateUspsManifest(warehouse);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Manifest created", result.Message);
        Assert.NotNull(result.Manifests);
        Assert.Contains("application/json", result.Manifests.Keys);
        Assert.Contains("application/pdf", result.Manifests.Keys);

        // Verify file upload was called
        _mockFilesService.Verify(x => x.UploadToAzureAsync(
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            FileType.ShippingManifests,
            "pdf-container"
        ), Times.Once);

        // Verify manifest was saved with correct file path
        mockShippingManifestRepo.Verify(x => x.AddRangeAsync(
            It.Is<List<ShippingManifest>>(manifests => 
                manifests.Count == 1 && 
                manifests[0].ManifestFile == expectedFilePath &&
                manifests[0].Carrier == "USPS"
            )
        ), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ParseUSPSResponseAsync_WithValidMultipartResponse_SavesFilesCorrectly()
    {
        // Arrange
        var boundary = "mzowiqXHz1WiH76KNgYBYiyz";
        var jsonContent = "{\"form\":\"5630\",\"imageType\":\"PDF\",\"labelType\":\"8.5x11LABEL\"}";
        
        // Create a simple PDF content and base64 encode it
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // Simple PDF header
        var pdfContent = Convert.ToBase64String(pdfBytes);
        
        var multipartContent = new MultipartFormDataContent(boundary);
        
        var jsonPart = new StringContent(jsonContent, Encoding.UTF8);
        jsonPart.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        jsonPart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "SCANFormMetaData"
        };
        
        var pdfPart = new StringContent(pdfContent, Encoding.UTF8);
        pdfPart.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        pdfPart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "SCANFormImage",
            FileName = "SCANFormImage.pdf"
        };
        
        multipartContent.Add(jsonPart);
        multipartContent.Add(pdfPart);

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = multipartContent
        };

        string expectedFilePath = "files/test.pdf";
        _mockFilesService.Setup(x => x.UploadToAzureAsync(
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            FileType.Pdf,
            "application/pdf"
        )).ReturnsAsync(expectedFilePath);

        // Act
        var result = await _service.ParseUSPSResponseAsync(response);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("application/json", result.Keys);
        Assert.Contains("application/pdf", result.Keys);
        Assert.Equal(jsonContent, result["application/json"]);
        Assert.Equal(pdfContent, result["application/pdf"]);

        // Verify file was saved
        _mockFilesService.Verify(x => x.UploadToAzureAsync(
            It.IsAny<byte[]>(),
            "SCANFormImage.pdf",
            FileType.Pdf,
            "application/pdf"
        ), Times.Once);
    }
}
