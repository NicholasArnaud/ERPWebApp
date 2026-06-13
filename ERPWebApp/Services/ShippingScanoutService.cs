using ERPWebApp.Data;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Services.IServices;
using System.Text.Json;
using System.Text;
using System.Xml.Linq;
using ERPWebApp.Data.DTOModels.ShippingScanout;
using ERPWebApp.Models.Common;
using ERPWebApp.Models.Shipping;
using ERPWebApp.Data.DTOModels.ShipEngineDtos;
using ERPWebApp.Data.DTOModels.ShippingScanout.USPS;
using System.Text.Json.Nodes;
using ERPWebApp.Extensions;

namespace ERPWebApp.Services;

public class ShippingScanoutService(
    IUnitOfWork unitOfWork,
    IHttpClientFactory httpClientFactory,
    IWebhooks webhooks,
    ILogger<ShippingScanoutService> logger,
    IWebhookBatchService webhookBatchService,
    IFilesService filesService) : Service<ShippingScanout>(unitOfWork), IShippingScanoutService
{
    IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IWebhooks _webhooks = webhooks;
    private readonly IWebhookBatchService _webhookBatchService = webhookBatchService;
    private readonly DateTime _now = TimeZoneInfo.ConvertTime(
            DateTime.Now,
            TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
        );
    private readonly ILogger<ShippingScanoutService> _logger = logger;
    private readonly IFilesService _fileService = filesService;

    public async Task<ShippingScanout> CreateNewShippingScanout(ShippingScanout shippingScanout)
    {
        var shipmentFound = await _unitOfWork.OrderShipments.FindAsync(x => x.trackingNumber == shippingScanout.ScannedTrackingNumber ||
        (shippingScanout.ScannedTrackingNumber.EndsWith(x.trackingNumber) && !string.IsNullOrEmpty(x.trackingNumber)));
        var fulfillmentFound = await _unitOfWork.OrderFulfillments.FindAsync(x => x.trackingNumber == shippingScanout.ScannedTrackingNumber ||
        (shippingScanout.ScannedTrackingNumber.EndsWith(x.trackingNumber) && !string.IsNullOrEmpty(x.trackingNumber)));

        shippingScanout.ScannedTrackingNumber = shippingScanout.ScannedTrackingNumber.ToUpperInvariant().Trim();
        shippingScanout.CreateDate = _now;
        shippingScanout.OrderShipmentId = shipmentFound.FirstOrDefault()?.OrderShipmentId;
        shippingScanout.OrderFulfillmentId = fulfillmentFound.FirstOrDefault()?.OrderFulfillmentId;
        shippingScanout.IsValidTrackingNumber = shipmentFound.FirstOrDefault()?.carrierCode is not null || (shippingScanout.ScannedTrackingNumber.Length is >= 18 and <= 20
            && shippingScanout.ScannedTrackingNumber.Contains('Z'));

        //Add and Save to DB
        return await AddAsync(shippingScanout);
    }

    public Task<IReadOnlyCollection<ShipmentsCountByCarrier>> GetOpenShipmentsCountByCarrierAsync()
        => _unitOfWork.ShippingScanouts.GetOpenShipmentsCountByCarrierAsync();

    public Task<(IReadOnlyCollection<ShippingManifest>, int)> GetShippingManifestsAsync(
        string carrierId,
        string warehouseId,
        DateTime? shipDate,
        SearchParameters search
    ) => _unitOfWork.ShippingManifest.GetShippingManifestsAsync(carrierId, warehouseId, shipDate, search);

    public async Task<int> OnBulkUpdateScanouts(List<ShippingScanout> shippingScanouts)
    {
        _unitOfWork.ShippingScanouts.UpdateRange(shippingScanouts);
        return await _unitOfWork.SaveChangesAsync();
    }
    /// <summary>  
    /// Sends UPS Add or Remove from ULO (UPS Load Optimization) requests for the specified trailer number and list of tracking numbers in batches of 200.  
    /// </summary>
    /// <param name="shippingScanoutList">The list of all scanouts that need to be sent to UPS.</param>  
    /// <param name="trailerNumber">The trailer number for the Add or Remove from ULO request.</param>  
    /// <returns>An IActionResult indicating the success or failure of the operation.</returns> 
    public async Task<int> SendUPSListAddOrRemoveFromUloRequest(List<ShippingScanout> shippingScanoutList, string trailerNumber)
    {
        if (string.IsNullOrEmpty(trailerNumber) || !shippingScanoutList.Any())
            return 0;
        try
        {
            int batchSize = 200;
            int currentTrackingNumberIteration = 0;
            List<string> deserializedResponses = new();
            StringBuilder trackingNumberEvents = new();
            List<ShippingScanout> reachedScanouts = new();
            int scanoutsSaved = 0;
            foreach (var shippingScanout in shippingScanoutList)
            {
                //TODO:: For now we will override whatever trailer number was initialally passed when the scanout was added to the db
                //if (shippingScanout.TrailerNumber.IsNullOrEmpty())
                //{
                shippingScanout.TrailerNumber = trailerNumber;
                //}
                //else
                //{
                //    //TODO:: Add logic to handle multiple trailers
                //}
                trackingNumberEvents.Append($@"<v11:Events>  
                                            <v11:Action>A</v11:Action>  
                                            <v11:Input>K</v11:Input>  
                                            <v11:Mode>B</v11:Mode>  
                                            <v11:Item>{shippingScanout.ScannedTrackingNumber}</v11:Item>  
                                            <v11:EventTime>{_now:yyyy-MM-ddTHH:mm:ss}</v11:EventTime>  
                                        </v11:Events>");

                currentTrackingNumberIteration++;
                reachedScanouts.Add(shippingScanout);
                if (currentTrackingNumberIteration > batchSize - 1)
                {
                    var (httpResponseHeaders, responseBody) = await _webhooks.SendUPSListAddOrRemoveFromUloRequest(
                        trailerNumber,
                        trackingNumberEvents.ToString()
                    );
                    XDocument xmlResponse = XDocument.Parse(responseBody);
                    //deserializedResponses.Add(JsonSerializer.SerializeToNode(xmlResponse));
                    trackingNumberEvents.Clear();
                    currentTrackingNumberIteration = 0;

                    var webHookBatch = await _webhookBatchService.GetAsNoTrackingAsync(x =>
                    x.ResponseHeaders == httpResponseHeaders && x.ResponseBody == responseBody);
                    //Update the scanouts with the webhook batch id
                    foreach (ShippingScanout scanout in reachedScanouts)
                    {
                        scanout.WebhookBatchId = webHookBatch.WebHookBatchId;
                    }
                    scanoutsSaved += await OnBulkUpdateScanouts(reachedScanouts);
                    reachedScanouts.Clear();
                }
            }

            if (trackingNumberEvents.Length > 0)
            {

                var (httpResponseHeaders, responseBody) = await _webhooks.SendUPSListAddOrRemoveFromUloRequest(
                    trailerNumber,
                    trackingNumberEvents.ToString()
                );

                XDocument xmlResponse = XDocument.Parse(responseBody);
                //deserializedResponses.Add(JsonConvert.SerializeXNode(xmlResponse));
                trackingNumberEvents.Clear();
                var webHookBatch = await _webhookBatchService.GetAsNoTrackingAsync(x =>
                x.ResponseHeaders == httpResponseHeaders && x.ResponseBody == responseBody);
                //Update the scanouts with the webhook batch id
                foreach (ShippingScanout scanout in reachedScanouts)
                {

                    scanout.WebhookBatchId = webHookBatch.WebHookBatchId;
                }
                scanoutsSaved += await OnBulkUpdateScanouts(reachedScanouts);
                reachedScanouts.Clear();
            }
            return scanoutsSaved;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending UPS Add or Remove from ULO request for trailer number {trailerNumber} and list of tracking numbers {trackingNumbers}", trailerNumber, shippingScanoutList);
            return -1;
        }

    }

    public async Task SaveShippingManifestsAsync(List<ShippingManifest> manifests)
    {
        await _unitOfWork.ShippingManifest.AddRangeAsync(manifests);
        await _unitOfWork.SaveChangesAsync();
    }

    public Task<(List<string>, int)> GetScannedUspsTrackingNumbersAsync((string name, string state, string postalCode) shipFrom)
        => _unitOfWork.ShippingScanouts.GetScannedUspsTrackingNumbersAsync(shipFrom);

    public async Task<List<ShipEngineCarriers>> FetchCarriers<T>()
    {
        using HttpClient client = _httpClientFactory.CreateClient("ShipEngineV1");
        ShipEngineCarrierRoot response = await client.GetFromJsonAsync<ShipEngineCarrierRoot>("carriers");
        return response.Carriers;
    }

    public async Task<UspsManifestResponse> GenerateUspsManifest(
        ShipEngineWarehouse warehouse, List<string> validTrackingNumbers = null, int validShipmentsCount = 0
    )
    {
        var (trackingNumbers, shipmentsCount) = await GetScannedUspsTrackingNumbersAsync((warehouse.OriginAddress.Name, warehouse.OriginAddress.State, warehouse.OriginAddress.PostalCode));
        var fromAddress = warehouse.OriginAddress.ConvertToUspsAddress();

        StringContent payload = new(JsonSerializer.Serialize(new JsonObject
        {
            ["form"] = "5630",
            ["imageType"] = "PDF",
            ["labelType"] = "8.5x11LABEL",
            ["mailingDate"] = _now.ToString("yyyy-MM-dd"),
            ["overwriteMailingDate"] = false,
            ["entryFacilityZIPCode"] = fromAddress.ZipCode,
            ["destinationEntryFacilityType"] = "NONE",
            ["shipment"] = new JsonObject
            {
                ["trackingNumbers"] = new JsonArray((validTrackingNumbers != null) ? validTrackingNumbers.Select(x => JsonValue.Create(x)).ToArray() : [.. trackingNumbers.Select(x => JsonValue.Create(x))]),
            },
            ["fromAddress"] = new JsonObject
            {
                ["streetAddress"] = fromAddress.StreetAddress,
                ["secondaryAddress"] = fromAddress.SecondaryAddress,
                ["city"] = fromAddress.City,
                ["state"] = fromAddress.State,
                ["ZIPCode"] = fromAddress.ZipCode,
                ["firm"] = warehouse.OriginAddress.CompanyName,
                ["ignoreBadAddress"] = true
            }
        }), Encoding.Default, "application/json");

        using HttpClient client = _httpClientFactory.CreateClient("USPS");
        var response = await client.PostAsync("scan-forms/v3/scan-form", payload);
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            validTrackingNumbers = [.. trackingNumbers.Where(x => !errorContent.Contains(x))];
            if (validTrackingNumbers.Count > 0)
                return await GenerateUspsManifest(warehouse, validTrackingNumbers, validTrackingNumbers.Count);
            else
                response.EnsureSuccessStatusCode();
        }
        response.EnsureSuccessStatusCode();
        var manifestContent = await ParseUSPSResponseAsync(response);
        manifestContent.TryGetValue("application/pdf", out string pdfContent);
        if (string.IsNullOrEmpty(pdfContent))
        {
            throw new InvalidOperationException("Manifest PDF content is missing in the response.");
        }
        var pdfBytes = Convert.FromBase64String(pdfContent);
        var fileName = $"Usps{_now:yyyyMMddHHmmssfff}manifest.pdf";
        var filePath = await filesService.UploadToAzureAsync(
            pdfBytes,
            fileName,
            FileType.ShippingManifests,
            "pdf-container"
        );

        var manifest = new ShippingManifest
        {
            Id = Guid.NewGuid(),
            ManifestId = $"Usps{_now:yyyyMMddHHmmssfff}",
            WarehouseId = warehouse.WarehouseId,
            Warehouse = warehouse.WarehouseName,
            CarrierId = "",
            Carrier = "USPS",
            CreatedDate = _now,
            ShipDate = _now,
            ShipmentCount = (validShipmentsCount > 0) ? validShipmentsCount : shipmentsCount,
            ManifestFile = filePath
        };

        await SaveShippingManifestsAsync([manifest]);
        return new UspsManifestResponse(true, "Manifest created", manifestContent);
    }
    public async Task<Dictionary<string, string>> ParseUSPSResponseAsync(HttpResponseMessage response)
    {
        var contentDictionary = new Dictionary<string, string>();

        try
        {
            // Validate Content-Type
            if (response.Content.Headers.ContentType == null)
            {
                throw new InvalidOperationException("Response does not contain a Content-Type header.");
            }

            // Extract boundary and remove quotes if present
            string boundary = response.Content.Headers.ContentType.Parameters
                .FirstOrDefault(p => p.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase))?.Value
                ?.Trim('"');

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidOperationException("Boundary is missing or invalid in the response.");
            }

            // Read the entire content as a string first
            var content = await response.Content.ReadAsStringAsync();

            // Split the content by the boundary
            var sections = content.Split([$"--{boundary}"], StringSplitOptions.RemoveEmptyEntries)
                .Where(s => !string.IsNullOrWhiteSpace(s) && !s.Trim().EndsWith("--"))
                .ToList();

            foreach (var sectionContent in sections)
            {
                var contentType = GetContentTypeFromSection(sectionContent);
                if (string.IsNullOrWhiteSpace(contentType))
                {
                    continue; // Skip sections with no ContentType
                }

                var contentDisposition = GetContentDispositionFromSection(sectionContent);
                var sectionBody = GetSectionBody(sectionContent);

                switch (contentType.ToLowerInvariant())
                {
                    case "application/json":
                        contentDictionary.TryAdd(contentType, sectionBody);
                        break;

                    case "application/pdf":
                        var filename = GetFileNameFromContentDisposition(contentDisposition);
                        var fileBytes = Convert.FromBase64String(sectionBody);
                        var fileType = contentType == "application/pdf" ? FileType.Pdf : FileType.Image;
                        await _fileService.UploadToAzureAsync(fileBytes, filename, fileType, contentType);
                        contentDictionary.TryAdd(contentType, sectionBody);
                        break;

                    default:
                        _logger.LogWarning($"Unsupported ContentType: {contentType}");
                        break;
                }
            }

            // Ensure we have both required content types
            if (!contentDictionary.ContainsKey("application/json") || !contentDictionary.ContainsKey("application/pdf"))
            {
                throw new InvalidOperationException("Response must contain both JSON and PDF content.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while parsing USPS response.");
            throw;
        }

        return contentDictionary;
    }

    private string GetContentTypeFromSection(string sectionContent)
    {
        var contentTypeMatch = System.Text.RegularExpressions.Regex.Match(sectionContent, @"Content-Type:\s*([^\r\n]+)");
        return contentTypeMatch.Success ? contentTypeMatch.Groups[1].Value.Trim() : null;
    }

    private string GetContentDispositionFromSection(string sectionContent)
    {
        var contentDispositionMatch = System.Text.RegularExpressions.Regex.Match(sectionContent, @"Content-Disposition:\s*([^\r\n]+)");
        return contentDispositionMatch.Success ? contentDispositionMatch.Groups[1].Value.Trim() : null;
    }

    private string GetSectionBody(string sectionContent)
    {
        // Find the first blank line after headers
        var bodyStart = sectionContent.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        if (bodyStart == -1)
        {
            bodyStart = sectionContent.IndexOf("\n\n", StringComparison.Ordinal);
        }

        if (bodyStart == -1)
        {
            return string.Empty;
        }

        // Skip the blank line
        bodyStart += 4;
        return sectionContent.Substring(bodyStart).Trim();
    }

    private string GetFileNameFromContentDisposition(string contentDisposition)
    {
        if (string.IsNullOrEmpty(contentDisposition))
            return "SCANFormImage.pdf";

        // Look for filename in the content disposition header
        var filenameMatch = System.Text.RegularExpressions.Regex.Match(contentDisposition, @"filename=""?([^"";]+)""?");
        if (filenameMatch.Success)
        {
            return filenameMatch.Groups[1].Value.Trim('"');
        }

        return "SCANFormImage.pdf";
    }
}
