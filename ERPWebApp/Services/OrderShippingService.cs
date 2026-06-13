using ERPWebApp.Data;
using ERPWebApp.Data.DTOModels;
using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models;
using ERPWebApp.Models.Orders;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Serialization;
using static ERPWebApp.Data.DTOModels.ZazzleDTO;
using static ERPWebApp.Models.Orders.Order;
using System.Text;
using ERPWebApp.Data.DTOModels.ShipEngineDtos;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json.Serialization;

namespace ERPWebApp.Services;

public class OrderShippingService : Service<OrderShipment>, IOrderShippingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebhooks _webhooks;
    private readonly DateTime _now;
    private readonly IOrderService _orderService;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IFilesService _fileService;
    private readonly ILogger<OrderShippingService> _logger;
    public OrderShippingService(
        IUnitOfWork unitOfWork,
        IWebhooks webhooks,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IOrderService orderService,
        IFilesService filesService,
        ILogger<OrderShippingService> logger
    ) : base(unitOfWork)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _webhooks = webhooks;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _orderService = orderService;
        _fileService = filesService;
        _now = TimeZoneInfo.ConvertTime(
            DateTime.Now,
            TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
        );
    }

    public async Task<Order> GetOrderByOrderIdAndKeyCustomSelectAsync(long orderId, string orderKey)
    {
        return await _unitOfWork.Orders.GetOrderByOrderIdAndKeyCustomSelectAsync(orderId, orderKey);
    }

    public async Task<Order> GenerateLabelShipEngine(
        Order ssosData,
        string shippedByUsername
    )
    {
        long preservedStoreId = ssosData.advancedOptions.storeId;
        ssosData = await GetRateEstimate(ssosData);
        ssosData.IsCreateShipmentClicked = true;
        //Add custom class values here that isn't normally pulled from the API model for this class
        string shipEngineId = "";
        string labelData = "";
        string returnedTrackingNumber = "";
        var carrierCode = ssosData.carrierCode.ToLower();
        if (carrierCode.Contains("stamps_com") || carrierCode.Contains("usps"))
        {
            var contentDictionary = await GenerateUspsShippingLabel(ssosData);
            contentDictionary.TryGetValue("application/json", out string jsonString);
            JsonObject JsonSerialized = (JsonObject)JsonNode.Parse(jsonString);
            //Add custom class values here that isn't normally pulled from the API model for this class
            labelData = $"data:application/pdf;base64,{contentDictionary.GetValueOrDefault("application/pdf")}";
            returnedTrackingNumber = JsonSerialized?["trackingNumber"].ToString();
        }
        else
        {
            ShipEngineLabel shipEngineLabelResponse = await _orderService.CreateShipEngineShipmentLabel(ssosData);
            //Add custom class values here that isn't normally pulled from the API model for this class
            shipEngineId = shipEngineLabelResponse.ShipmentId;
            labelData = shipEngineLabelResponse.LabelDownload.Href;
            returnedTrackingNumber = shipEngineLabelResponse.TrackingNumber;
        }

        if (ssosData.Sources.Any(s => s.Name == OrderSourceEnum.skulabs))
        {
            var addManualShipmentRequest = new SkulabsDTO
            {
                StoreId = preservedStoreId.ToString(),
                OrderNumber = ssosData.orderNumber,
                Address = ssosData.shipTo.street1,
                Carrier = ssosData.carrierCode,
                TrackingNumber = returnedTrackingNumber,
                Service = ssosData.serviceCode,
                OrderItems = ssosData.items.Select(item => new SkulabsItemDTO
                {
                    Quantity = item.quantity,
                    LocationId = item.warehouseLocation,
                    ItemId = item.options.FirstOrDefault(option => option.Name == "item_id")?.value,
                    LineId = item.lineItemKey.ToString(),
                    SerialNumbers = null,
                    Id = item.options.FirstOrDefault(option => option.Name == "_id")?.value,
                }).ToList(),
                Dropship = "null",
                NoRefresh = false,
                Notes = "Shipment created by system.",
                ForceDeduction = "false",
                Cost = ssosData.estimatedShipmentCost,
                Options = new JsonObject(),
                Booking = null,
                Seal = null,
                OriginAddress = null,
                BolProNumber = null,
                TotalBundles = null,
                TrackingUrl = null,
                Parcel = null
            };

            var response = await _webhooks.SkulabsAddManualShipment(addManualShipmentRequest);
        }
        else
        {
            //Create the new Shipment info for the order  
            OrderShipment newShipment = new()
            {
                dimensions = ssosData.dimensions,
                weight = ssosData.weight,
                trackingNumber = returnedTrackingNumber,
                orderId = ssosData.orderId,
                userId = ssosData.userId,
                orderKey = ssosData.orderKey,
                createDate = _now,
                ERPModifyDate = _now,
                shipDate = _now,
                shipmentCost = ssosData.estimatedShipmentCost,
                insuranceCost = ssosData.insuranceOptions.insuredValue,
                carrierCode = ssosData.carrierCode,
                serviceCode = ssosData.serviceCode,
                packageCode = ssosData.packageCode,
                confirmation = ssosData.confirmation,
                voided = false,
                marketplaceNotified = true,
                shipFrom = ssosData.shipFrom,
                shipTo = ssosData.shipTo,
                advancedOptions = ssosData.advancedOptions,
                ShipEngineShipmentId = shipEngineId,
                shipmentItems = ssosData.items,
                labelData = labelData,
                testLabel = false,
                IsExpedited = ssosData.isExpedited,
                ERPModifyByUserId = shippedByUsername
            };

            ssosData.orderShipments ??= new List<OrderShipment>();
            ssosData.orderShipments.Add(newShipment);
            await _unitOfWork.SaveChangesAsync();

            await _orderService.SetOrderAsShipped(
            ssosData.orderId.ToString(),
            ssosData.carrierCode,
            ssosData.shipDate ?? _now,
            newShipment.trackingNumber);
        }
        return ssosData;
    }
    private async Task<Dictionary<string, string>> GenerateUspsShippingLabel(Order order)
    {

        try
        {
            JsonObject payload = [];
            if (order.ERPOrderId != 0)
            {
                string fullName = order.shipTo.name;
                string[] nameParts = fullName?.Split(' ') ?? [];
                string firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
                string lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : string.Empty;

                if (string.IsNullOrWhiteSpace(lastName))
                {
                    lastName = "(No Last Name Provided)"; // Fallback value, need to do this to meet API requirements. If needed, we can change this at a later date, leaving this as a default for empty last names.
                }

                JsonObject imageInfo = new()
                {
                    ["imageType"] = "PDF",
                    ["receiptOption"] = "NONE",
                    ["suppressPostage"] = true,
                    ["suppressMailDate"] = true
                };

                JsonObject fromAddress = new()
                {
                    ["streetAddress"] = order.shipFrom.street1,
                    ["secondaryAddress"] = order.shipFrom.street2,
                    ["city"] = order.shipFrom.city,
                    ["state"] = order.shipFrom.state,
                    ["ZIPCode"] = order.shipFrom.postalCode?.Split('-')[0] ?? string.Empty,
                    ["firm"] = "CFWarehouse",
                    ["ignoreBadAddress"] = true
                };

                JsonObject toAddress = new()
                {
                    ["firstName"] = firstName,
                    ["lastName"] = lastName,
                    ["streetAddress"] = order.shipTo.street1,
                    ["secondaryAddress"] = order.shipTo.street2 ?? string.Empty,
                    ["city"] = order.shipTo.city,
                    ["state"] = order.shipTo.state,
                    ["ZIPCode"] = order.shipTo.postalCode?.Split('-')[0] ?? string.Empty,
                    ["ignoreBadAddress"] = true
                };
                if (order.shipTo.postalCode?.Split('-').Length > 1)
                    toAddress.Add("ZIPPlus4", order.shipTo.postalCode?.Split('-')[1]);

                JsonObject packageOptions = new()
                {
                    ["packageValue"] = order.orderTotal,
                    ["nonDeliveryOption"] = "RETURN",
                    ["immediateManifest"] = false
                };

                JsonObject destinationEntryFacilityAddress = new()
                {
                    ["streetAddress"] = order.shipFrom.street1,
                    ["secondaryAddress"] = order.shipFrom.street2,
                    ["city"] = order.shipFrom.city,
                    ["state"] = order.shipFrom.state,
                    ["ZIPCode"] = order.shipFrom.postalCode
                };

                JsonObject packageDescription = new()
                {
                    ["mailClass"] = order.serviceCode switch
                    {
                        "priority_mail" => "PRIORITY_MAIL",
                        "usps_priority_mail" => "PRIORITY_MAIL",
                        "usps_priority_mail_express" => "PRIORITY_MAIL_EXPRESS",
                        "usps_first_class_mail" => "USPS_GROUND_ADVANTAGE",
                        "usps_ground_advantage" => order.serviceCode.ToUpperInvariant(),
                        _ => throw new Exception("Unexpected service code used. Cannot check service code correctly.")
                    },
                    ["weight"] = order.weight.units == OrderWeight.Units.ounces ? order.weight.value / 16m : order.weight.value,
                    ["weightUOM"] = "lb",
                    ["length"] = order.dimensions.length,
                    ["width"] = order.dimensions.width,
                    ["height"] = order.dimensions.height,
                    ["dimensionsUOM"] = order.dimensions switch
                    {
                        { units: OrderDimensions.Units.inches } => "in",
                        _ => throw new Exception("Unexpected dimension units used. Cannot check dimensions correctly.")
                    },
                    ["customerReference"] = new JsonArray
                    {
                        new JsonObject {
                            ["referenceNumber"]= order.orderNumber,
                            ["printReferenceNumber"]= true
                        }
                    },
                    ["processingCategory"] = "MACHINABLE",
                    ["mailingDate"] = _now.ToString("yyyy-MM-dd"),
                    ["extraServices"] = new JsonArray { 920 },
                    ["packageOptions"] = packageOptions,
                    ["destinationEntryFacilityType"] = "NONE",
                    ["destinationEntryFacilityAddress"] = destinationEntryFacilityAddress,
                    ["rateIndicator"] = "SP"
                };

                payload = new JsonObject
                {
                    ["imageInfo"] = imageInfo,
                    ["fromAddress"] = fromAddress,
                    ["toAddress"] = toAddress,
                    ["packageDescription"] = packageDescription
                };
            }
            using HttpClient client = _httpClientFactory.CreateClient("USPS");
            client.DefaultRequestHeaders.Add("X-Require-Payment-Auth", "");
            using StringContent jsonContent = new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await client.PostAsync("labels/v3/label", jsonContent);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            return await ParseUSPSResponseAsync(response);
        }
        catch (JsonException jsonException)
        {
            _logger.LogError("Failed to parse order;{ex}", jsonException.Message);
            // Handle JSON deserialization errors here  
            Console.WriteLine($"JsonException: {jsonException.Message}");
            Console.WriteLine($"Path: {jsonException.Path}");
            Console.WriteLine($"LineNumber: {jsonException.LineNumber}");
            Console.WriteLine($"BytePositionInLine: {jsonException.BytePositionInLine}");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("Failed to generate USPS label;{ex}", ex.Message);
            // Handle HTTP request errors here
            Console.WriteLine($"HttpRequestException: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> VoidUspsLabel(string trackingNumber)
    {
        try
        {
            using HttpClient client = _httpClientFactory.CreateClient("USPS");
            client.DefaultRequestHeaders.Add("X-Require-Payment-Auth", "");
            using HttpResponseMessage response = await client.DeleteAsync($"labels/v3/label/{trackingNumber}");
            response.EnsureSuccessStatusCode();
            UspsVoidLabelResponse voidLabelResponse = await response.Content.ReadFromJsonAsync<UspsVoidLabelResponse>();
            return true;
        }
        catch (JsonException jsonException)
        {
            _logger.LogError("Failed to parse order;{ex}", jsonException.Message);
            // Handle JSON deserialization errors here  
            Console.WriteLine($"JsonException: {jsonException.Message}");
            Console.WriteLine($"Path: {jsonException.Path}");
            Console.WriteLine($"LineNumber: {jsonException.LineNumber}");
            Console.WriteLine($"BytePositionInLine: {jsonException.BytePositionInLine}");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("Failed to void USPS label;{ex}", ex.Message);
            // Handle HTTP request errors here
            Console.WriteLine($"HttpRequestException: {ex.Message}");
            throw;
        }
    }
    public async Task<Dictionary<string, string>> ParseUSPSResponseAsync(HttpResponseMessage response)
    {
        //Fetch the boundary from the Content-Type header and normalize it to be parsed later. If none can be found, throw an exception.
        var boundary = (response.Content.Headers.ContentType.Parameters.FirstOrDefault(p =>
        p.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase))?.Value).Trim('"') ??
        throw new InvalidOperationException("Missing boundary in multipart response.");

        //Setup response body to be parsed by the Multipart reader.
        var stream = await response.Content.ReadAsStreamAsync();
        var reader = new MultipartReader(boundary, stream);
        var contentDictionary = new Dictionary<string, string>();
        MultipartSection section;
        //Read each section of the multipart response and parse it based on its content type.
        //Will continue until no more sections were successfully parsed in which the reader will return null.
        while ((section = await reader.ReadNextSectionAsync()) != null)
        {
            if (section.ContentType == "application/json")
            {
                using var jsonStream = new StreamReader(section.Body);
                var jsonString = await jsonStream.ReadToEndAsync();
                contentDictionary.Add(section.ContentType, jsonString);
            }
            else if (section.ContentType == "application/pdf")
            {
                //Parse the responses filename for the pdf
                var filename = ContentDispositionHeaderValue.Parse(section.ContentDisposition).FileName.Trim('"');
                //Setup memory stream to read the pdf bytes from the section body and upload to Azure.
                var fileBytes = await DecodeBase64FileAsync(section.Body);
                //Upload the file to Azure and add the base64 string to the dictionary to send to the user for download.
                await _fileService.UploadToAzureAsync(fileBytes, filename, FileType.Pdf, section.ContentType);
                contentDictionary.Add(section.ContentType, Convert.ToBase64String(fileBytes));
            }
        }
        return contentDictionary;
    }
    private async Task<byte[]> DecodeBase64FileAsync(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var base64String = Encoding.UTF8.GetString(memoryStream.ToArray());
        return Convert.FromBase64String(base64String);
    }

    public async Task<Order> GenerateLabelZazzle(Order ssosData, string shippedByUsername)
    {
        //NOTE: No get rate estimate here due to Zazzle having their own shipping stored within their account. Zazzle handles their own shipping rates and labels
        // flag the button as clicked
        ssosData.IsCreateShipmentClicked = true;
        ObjectResult returnResult = (ObjectResult)await _webhooks.CreateZazzleLabel(ssosData.orderNumber, ssosData.weight, ssosData.dimensions);
        ZazzleDTO zazzleDTO = new()
        {
            VendorId = _configuration["Zazzle:VendorId"],
            zazzleRequest = new()
        };
        var serializer = new XmlSerializer(typeof(ZazzleRequest.Response));
        using TextReader reader = new StringReader(returnResult.Value.ToString());
        zazzleDTO.zazzleRequest.response = (ZazzleRequest.Response)serializer.Deserialize(reader);
        Console.WriteLine(zazzleDTO.zazzleRequest.response.responseStatus);
        if (returnResult == null || returnResult.StatusCode != 200)
        {
            throw new HttpRequestException($"Failed to create shipment label. Status code: {returnResult?.StatusCode}");
        }

        if (zazzleDTO.zazzleRequest.response.responseStatus.code != ZazzleRequest.Response.ResponseStatus.ResponseCode.success)
        {
            throw new HttpRequestException($"Zazzle API Error. {zazzleDTO.zazzleRequest.response.responseStatus.Info}");
        }
        //Create the new Shipment info for the order
        OrderShipment newShipment = new()
        {
            dimensions = ssosData.dimensions,
            weight = ssosData.weight,
            trackingNumber = zazzleDTO.zazzleRequest.response.result.ShippingInfo.TrackingNumber,
            orderId = ssosData.orderId,
            userId = ssosData.userId,
            orderKey = ssosData.orderKey,
            createDate = _now,
            shipDate = _now,
            shipmentCost = 0.0m,
            insuranceCost = 0.0m,
            voided = false,
            marketplaceNotified = true,
            shipFrom = ssosData.shipFrom,
            shipTo = ssosData.shipTo,
            advancedOptions = ssosData.advancedOptions,
            shipmentItems = ssosData.items,
            labelData = zazzleDTO.zazzleRequest.response.result.ShippingInfo.ShippingDocuments.First().Url,
            testLabel = false,
            IsExpedited = false,
            ERPModifyByUserId = shippedByUsername
        };

        //Check if its the first shipment and if not just add on to it
        ssosData.orderShipments ??= new List<OrderShipment>();
        if (ssosData.orderShipments.Count == 0 || ssosData.orderShipments.Any(x => x.trackingNumber != newShipment.trackingNumber))
        {
            ssosData.orderShipments.Add(newShipment);
        }
        _ = await _unitOfWork.SaveChangesAsync();
        return zazzleDTO.zazzleRequest.response is null
            ? throw new InvalidOperationException("Failed to retrieve label or tracking number correctly. JSON response is empty.")
            : ssosData;
    }

    public async Task<Order> GenerateLabelShopify(Order ssosData, string shippedByUsername)
    {
        throw new NotImplementedException();
    }

    public async Task<Order> GetRateEstimate(Order ssosData)
    {
        //TODO:: REMOVE as implemented
        if (!ssosData.shipTo.country.Contains("US") && !ssosData.shipTo.country.Contains("United States"))
        {
            throw new Exception("Unable to ship international orders.");
        }

        if (ssosData.weight == null || ssosData.weight.value == 0 || ssosData.dimensions == null)
        {
            return ssosData;
        }

        decimal weightInOunces = ssosData.weight.units switch
        {
            OrderWeight.Units.pounds => ssosData.weight.value * 16,
            OrderWeight.Units.ounces => ssosData.weight.value,
            _ => ssosData.weight.value
        };

        int pounds = (int)(weightInOunces / 16);
        int ounces = (int)(weightInOunces % 16);

        // Could use verification on these formulas.

        // As provided by Jimmy;
        // If weight = x lbs and y oz
        // If y > 12, weight = x + 2 and y-y.
        if (ounces > 12)
        {
            pounds += 2;
            ounces = 0;
        }

        //If x=0 and y < 14, then use y+2 but trunk decimals.
        else if (pounds == 0 && ounces < 14)
        {
            ounces = (int)Math.Ceiling((decimal)ounces + 2);
        }

        //If y=>14 and x=0, then use x+2 and y-y.
        else if (pounds == 0 && ounces >= 14)
        {
            pounds = 2;
            ounces = 0;
        }

        // Converting everything to ounces for simplicity.
        int finalWeightInOunces = (pounds * 16) + ounces;

        ssosData.weight.value = finalWeightInOunces;
        ssosData.weight.units = OrderWeight.Units.ounces;
        ssosData.weight.WeightUnits = (int)OrderWeight.Units.ounces;

        // Create a director instance  
        CustomShippingDirector director = new();
        // Create a builder instance  
        CustomShippingBuilder builder = new();

        // Fanrock uses Skulabs, so we change the storeId to fanrocks.
        if (ssosData.Sources.Any(x => x.Name == OrderSourceEnum.skulabs))
        {
            ssosData.advancedOptions.storeId = 1002300;
        }

        // get the "PONumber" field from the model  
        OrderItem.OrderItemOption poNumberOption = ssosData.items.FirstOrDefault()?.options.ToArray().SingleOrDefault(o => o.Name is "PONumber");
        string poNumber = poNumberOption?.value;
        ssosData.advancedOptions.labelMessageReference2 = poNumber;
        if (ssosData.items.All(x => x.Product == null && x.Bundle == null))
        {
            foreach (OrderItem item in ssosData.items)
            {
                item.Product ??= await _unitOfWork.Products.GetByQueryAsync(x => x.Include(y => y.Departments).Where(y => y.ProductId == item.ERPProductId));
                item.Bundle ??= await _unitOfWork.Bundles.GetByQueryAsync(x =>
                x.Include(y => y.BundleItems)
                .ThenInclude(y => y.Product)
                .ThenInclude(y => y.Departments)
                .Where(y => y.BundleId == item.ERPBundleId));
            }
        }
        // Construct a CustomShipping object using the director and builder instances  
        CustomShipping customShipping = director.Construct(builder, ssosData);
        ssosData.shipFrom = customShipping.OrderShippingInfo;

        List<string> carrierIds = customShipping.AppliedShipperIds
                .Where(x => x.Value == CustomShipping.ShipperApi.ShipEngine)
                .Select(x => x.Key).ToList();
        List<ShipEngineShippingEstimate> returnedEstimateInfo = await _orderService.GetShipEngineEstimatedShipmentRate(carrierIds, ssosData);
        //Remove estimates that do not have a shipping amount  
        _ = returnedEstimateInfo.RemoveAll(x => !x.shipping_amount.HasValue);
        builder.RemoveInvalidShippingServicesByShippingEstimates(returnedEstimateInfo);
        builder.AddEnsuredValidServiceCodesByShippingEstimates(returnedEstimateInfo);

        List<ShipEngineShippingEstimate> uspsServices = new();
        foreach (var stampsService in returnedEstimateInfo.Where(x => x.carrier_id == "se-548261"))
        {
            UspsPriceData uspsEstimateReturn = await GetUSPSRateEstimate(stampsService.service_code.ToUpper(), ssosData.shipTo, ssosData.weight, ssosData.dimensions);

            foreach (var rate in uspsEstimateReturn.Rates)
            {
                ShipEngineShippingEstimate uspsServiceCopy = stampsService;
                uspsServiceCopy.service_code = rate.MailClass.ToLower();
                uspsServiceCopy.carrier_code = "USPS";
                uspsServiceCopy.carrier_nickname = "USPS Direct API";
                uspsServiceCopy.service_type = "USPS " + rate.PriceType;
                var extraFees = rate.Fees.Sum(x => x.Price);
                uspsServiceCopy.shipping_amount = new ShipEngineShippingEstimate.Amount
                {
                    currency = "usd",
                    amount = rate.Price + extraFees
                };
                uspsServices.Add(uspsServiceCopy);
            }
        }
        returnedEstimateInfo.AddRange(uspsServices);
        //Either we filtered too much here or we shouldn't be shipping this order  
        if (!customShipping.AppliedShipperIds.Any())
        {
            _logger.LogWarning("No valid carriers for orderNumber: {orderNumber}", ssosData.orderNumber);
            throw new ApplicationException($"No valid carriers for orderNumber: {ssosData.orderNumber} available");
        }

        if (returnedEstimateInfo.Count == 0)
        {
            _logger.LogWarning("No Estimated Cost for orderNumber: {orderNumber} available", ssosData.orderNumber);
            throw new ApplicationException($"No Estimated Cost for orderNumber: {ssosData.orderNumber} available");
        }
        //Sort pulled services by cheapest available option  
        var cheapestService = returnedEstimateInfo.Select(x => new
        {
            x.carrier_code,
            x.service_code,
            totalShipmentEstimate = ((x.shipping_amount != null) ? x.shipping_amount.Value.amount : 0m) + ((x.other_amount != null) ? x.other_amount.Value.amount : 0m),
        }).Where(x => x.totalShipmentEstimate >= 1 || returnedEstimateInfo.Count == 1).OrderBy(x => x.totalShipmentEstimate).FirstOrDefault();
        ShipEngineShippingEstimate sortedEstimates = returnedEstimateInfo.Where(x => x.service_code == cheapestService.service_code && x.carrier_code == cheapestService.carrier_code && (x.shipping_amount.Value.amount >= 1 || returnedEstimateInfo.Count == 1)).FirstOrDefault();
        ssosData.carrierId = sortedEstimates.carrier_id;
        ssosData.carrierCode = sortedEstimates.carrier_code;
        ssosData.carrierNickname = sortedEstimates.carrier_nickname;
        ssosData.serviceCode = sortedEstimates.service_code;
        ssosData.estimatedShipmentCost = cheapestService.totalShipmentEstimate;
        ssosData.packageCode = sortedEstimates.package_type;
        ssosData.shipFrom = customShipping.OrderShippingInfo;

        if (ssosData.advancedOptions.storeName == null)
        {
            string storeName = await _unitOfWork.ShipStationStores.GetStoreNameById(ssosData.advancedOptions.storeId);

            ssosData.advancedOptions.storeName ??= storeName;
        }
        return ssosData;
    }
    //DUP OF GETRATEESTIMATE DIFFERENT RETURN TYPE
    public async Task<List<AvailableShipmentCarrier>> GetBestRates(Order ssosData)
    {
        //TODO:: REMOVE as implemented
        if (ssosData.shipTo.country != null && !ssosData.shipTo.country.Contains("US") && !ssosData.shipTo.country.Contains("United States"))
        {
            return [];
        }

        if (ssosData.weight == null || ssosData.weight.value == 0 || ssosData.dimensions == null)
        {
            return null;
        }
        // Create a director instance  
        CustomShippingDirector director = new();
        // Create a builder instance  
        CustomShippingBuilder builder = new();

        // Construct a CustomShipping object using the director and builder instances  
        CustomShipping customShipping = director.Construct(builder, ssosData);
        ssosData.shipFrom = customShipping.OrderShippingInfo;

        List<string> carrierIds = customShipping.AppliedShipperIds
                .Where(x => x.Value == CustomShipping.ShipperApi.ShipEngine)
                .Select(x => x.Key).ToList();

        List<ShipEngineShippingEstimate> returnedEstimateInfo = await _orderService.GetShipEngineEstimatedShipmentRate(carrierIds, ssosData);

        //Remove estimates that do not have a shipping amount  
        _ = returnedEstimateInfo.RemoveAll(x => !x.shipping_amount.HasValue);

        builder.RemoveInvalidShippingServicesByShippingEstimates(returnedEstimateInfo);
        builder.AddEnsuredValidServiceCodesByShippingEstimates(returnedEstimateInfo);

        //Either we filtered too much here or we shouldn't be shipping this order  
        if (!customShipping.AppliedShipperIds.Any())
        {
            _logger.LogWarning("No valid carriers for orderNumber: {orderNumber}", ssosData.orderNumber);
            throw new ApplicationException($"No valid carriers for orderNumber: {ssosData.orderNumber} available");
        }

        if (returnedEstimateInfo.Count == 0)
        {
            _logger.LogWarning("No Estimated Cost for orderNumber: {orderNumber} available", ssosData.orderNumber);
            throw new ApplicationException($"No Estimated Cost for orderNumber: {ssosData.orderNumber} available");
        }

        var AvailableServices = returnedEstimateInfo.Select(x => new AvailableShipmentCarrier
        {
            CarrierCode = x.carrier_code,
            ServiceCode = x.service_code,
            TotalShipmentEstimate = ((x.shipping_amount != null) ? x.shipping_amount.Value.amount : 0m) + ((x.other_amount != null) ? x.other_amount.Value.amount : 0m),
        }).Where(x => x.TotalShipmentEstimate > 1).OrderBy(x => x.TotalShipmentEstimate).ToList();

        return AvailableServices;
    }

    public async Task<Order> VoidAsync(int orderId, OrderShipment row)
    {
        try
        {
            ShipStationVoidMessage shipStationResult = await _orderService.VoidShipmentLabel(row.shipmentId);
            if (shipStationResult.Approved)
            {
                row.voided = true;
                row.voidDate = _now;

                _unitOfWork.OrderShipments.Update(row);
                _ = await _unitOfWork.SaveChangesAsync();
            }
            else
                _logger.LogError("Unexpected result type:{message}", shipStationResult.Message);
            return await _unitOfWork.Orders.GetOrderByIdCustomSelectAsync(orderId);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Shipment not found for voiding. OrderId: {orderId}, ShipmentId: {shipmentId}", orderId, row.shipmentId);
                row.voided = true;
                row.voidDate = _now;

                _unitOfWork.OrderShipments.Update(row);
                _ = await _unitOfWork.SaveChangesAsync();
                return await _unitOfWork.Orders.GetOrderByIdCustomSelectAsync(orderId);
            }
            _logger.LogError(ex, "Error occurred while voiding shipment.");
            throw;
        }
    }
    public async Task<OrderShipment> GetROWAsync(int id, string timestamp)
    {
        try
        {
            byte[] convertedtimestamp = Convert.FromBase64String(timestamp);

            // Find the row with the specified id and row version
            OrderShipment row = await _unitOfWork.OrderShipments.FilterOneAsync(x => x.OrderShipmentId == id && x.ERPTimestamp == convertedtimestamp);

            return row;
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    public async Task<OrderShipment> GetShipmentAsync(Expression<Func<OrderShipment, bool>> expression, params Expression<Func<OrderShipment, object>>[] includes)
    {
        return await _unitOfWork.OrderShipments.FilterOneAsync(expression, includes);
    }

    public async Task<List<OrderShipment>> GetShipmentListAsync(Expression<Func<OrderShipment, bool>> expression, Expression<Func<OrderShipment, string>>[] orderSelectors = null, params Expression<Func<OrderShipment, object>>[] includes)
    {
        return await _unitOfWork.OrderShipments.GetListByFilterAsync(expression, orderSelectors, includes);
    }

    public List<OrderShipment> GetShipmentList(Expression<Func<OrderShipment, bool>> expression, Expression<Func<OrderShipment, string>>[] orderSelectors = null, Expression<Func<OrderShipment, object>>[] includes = null)
    {
        return _unitOfWork.OrderShipments.GetListByFilter(expression, orderSelectors, includes);
    }

    public async Task<List<DepartmentShippedTotalDTO>> GetDepartmentShippedTotalsListAsync()
    {
        return await _unitOfWork.OrderShipments.GetAllDepartmentShippedTotals().ToListAsync();
    }

    public async Task<List<DepartmentShippedTotalByDateDTO>> GetDepartmentShippedTotalsByDateList() => await _unitOfWork.OrderShipments.GetAllDepartmentShippedTotalsByDateAsync();


    public void OnUpdateShipment(OrderShipment orderShipment)
    {
        _unitOfWork.OrderShipments.Update(orderShipment);
        _ = _unitOfWork.SaveChanges();
    }
    public void OnBulkUpdateShipments(List<OrderShipment> orderShipmentList)
    {
        _unitOfWork.OrderShipments.UpdateRange(orderShipmentList);
        _ = _unitOfWork.SaveChanges();
    }

    public List<Report> GetAvgShippingCostInDateRangeBySku(DateTime StartDate, DateTime EndDate)
    {
        try
        {
            SqlParameter[] parameters = new SqlParameter[]{
                    new SqlParameter("@StartDate", SqlDbType.DateTime2) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime2) { Value = EndDate }
            };

            Func<DbDataReader, Report> mapResult = (reader) =>
            new Report
            {
                Sku = reader.GetString(0),
                Description = reader.GetString(1),
                Average = reader.GetDecimal(2)
            };

            List<Report> reports = _unitOfWork.Orders.GetReports(
                "GetAvgShippingCostInDateRangeBySku",
                parameters,
                mapResult,
                120
            );

            if (reports == null || reports.Count == 0)
            {
                reports.Add(
                    new Report
                    {
                        Sku = "No matching records found",
                        Description = "No matching records found",
                        Average = 0
                    }
                );
            }
            return reports;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting average shipping cost by SKU.");
            return null;
        }
    }

    public List<Report> GetAvgShippingCostInDateRangeByService(DateTime StartDate, DateTime EndDate)
    {
        try
        {
            SqlParameter[] parameters = new SqlParameter[]{
                    new SqlParameter("@StartDate", SqlDbType.DateTime2) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime2) { Value = EndDate }
            };

            Func<DbDataReader, Report> mapResult = (reader) =>
            new Report
            {
                Service = reader.GetString(0),
                CarrierCode = reader.GetString(1),
                Average = reader.GetDecimal(2)
            };

            List<Report> reports = _unitOfWork.Orders.GetReports(
                "GetAvgShippingCostInDateRangeByService",
                parameters,
                mapResult,
                30
            );

            if (reports == null || reports.Count == 0)
            {
                reports.Add(
                    new Report
                    {
                        Service = "No matching records found",
                        CarrierCode = "No matching records found",
                        Average = 0
                    }
                );
            }
            return reports;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting average shipping cost by service.");
            return null;
        }
    }

    public List<Report> GetAmountItemsShippedByDateRange(DateTime StartDate, DateTime EndDate)
    {
        try
        {
            SqlParameter[] parameters = new SqlParameter[]{
                    new SqlParameter("@StartDate", SqlDbType.DateTime2) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime2) { Value = EndDate }
            };

            Func<DbDataReader, Report> mapResult = (reader) =>
            new Report
            {
                Sku = reader.GetString(0),
                Description = reader.GetString(1),
                Amount = reader.GetInt32(2),
                Department = reader.GetString(3)
            };

            List<Report> reports = _unitOfWork.Orders.GetReports(
                "GetAmountItemsShippedByDateRange",
                parameters,
                mapResult,
                30
            );

            if (reports == null || reports.Count == 0)
            {
                reports.Add(
                    new Report
                    {
                        Sku = "No matching records found",
                        Description = "No matching records found",
                        Amount = 0
                    }
                );
            }
            return reports;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting amount of items shipped by date range.");
            return null;
        }
    }

    public List<Report> GetAmountShippedByDateRangeSkuFilter(int? ProductId, DateTime StartDate, DateTime EndDate)
    {
        try
        {
            SqlParameter[] parameters = new SqlParameter[]{
                   new SqlParameter("@ProductId", SqlDbType.Int) { Value = ProductId ?? 0 },
                    new SqlParameter("@StartDate", SqlDbType.DateTime2) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime2) { Value = EndDate }
            };

            Func<DbDataReader, Report> mapResult = (reader) =>
            new Report
            {
                Sku = reader.GetString(0),
                Description = reader.GetString(1),
                Amount = reader.GetInt32(2)
            };

            List<Report> reports = _unitOfWork.Orders.GetReports(
                "GetAmountShippedByDateRangeSkuFilter",
                parameters,
                mapResult,
                30
            );

            if (reports == null || reports.Count == 0)
            {
                reports.Add(
                    new Report
                    {
                        Sku = "No matching records found",
                        Description = "No matching records found",
                        Amount = 0
                    }
                );
            }
            return reports;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting amount of items shipped by date range.");
            return null;
        }
    }

    public async Task<List<DepartmentShippedTotalByDateDTO>> GetAllDepartmentShippedTotalsInRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _unitOfWork.OrderShipments.GetAllDepartmentShippedTotalsInRangeAsync(startDate, endDate);
    }
    private async Task<UspsPriceData> GetUSPSRateEstimate(
        string serviceCode,
        OrderShippingInfo shippingInfo,
        OrderWeight orderWeight,
        OrderDimensions orderDimension)
    {
        var weightInLbs = orderWeight.units switch
        {
            OrderWeight.Units.ounces => orderWeight.value / 16,
            OrderWeight.Units.grams => orderWeight.value * (decimal)0.0022,
            OrderWeight.Units.pounds => orderWeight.value,
            _ => throw new Exception("Unexpected weight units used. Cannot check weight correctly.")
        };
        string processsingCategory;
        if (orderDimension.length > 22 || orderDimension.width > 18 || orderDimension.height > 15 || weightInLbs > 25)
            processsingCategory = "NONSTANDARD";
        else
            processsingCategory = "MACHINABLE";
        var payload = new JsonObject
        {
            ["originZIPCode"] = "70501",
            ["destinationZIPCode"] = shippingInfo.postalCode,
            ["weight"] = weightInLbs,
            ["length"] = orderDimension.length,
            ["width"] = orderDimension.width,
            ["height"] = orderDimension.height,
            ["mailClass"] = serviceCode.ToLower() switch
            {
                "usps_priority_mail" => "PRIORITY_MAIL",
                "usps_priority_mail_express" => "PRIORITY_MAIL_EXPRESS",
                "usps_first_class_mail" => "USPS_GROUND_ADVANTAGE",
                "usps_ground_advantage" => serviceCode.ToUpperInvariant(),
                _ => throw new Exception("Unexpected service code used. Cannot check service code correctly.")
            },
            ["processingCategory"] = processsingCategory,
            ["rateIndicator"] = "SP",
            ["destinationEntryFacilityType"] = "NONE",
            ["priceType"] = "COMMERCIAL",
            ["mailingDate"] = _now.Date.ToString("yyyy-MM-dd"),
            ["accountType"] = "EPS",
            ["accountNumber"] = _configuration["Usps:accountNumber"]
        };
        try
        {
            // Convert final JsonObject to JSON string  
            string requestBodyJson = JsonSerializer.Serialize(payload);
            using StringContent jsonContent = new(requestBodyJson, Encoding.UTF8, "application/json");
            using HttpClient client = _httpClientFactory.CreateClient("USPS");
            using HttpResponseMessage response = await client.PostAsync("prices/v3/base-rates/search", jsonContent);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UspsPriceData>(jsonResponse);
        }
        catch (JsonException jsonException)
        {
            _logger.LogError("Failed to parse order;{ex}", jsonException.Message);
            // Handle JSON deserialization errors here  
            Console.WriteLine($"JsonException: {jsonException.Message}");
            Console.WriteLine($"Path: {jsonException.Path}");
            Console.WriteLine($"LineNumber: {jsonException.LineNumber}");
            Console.WriteLine($"BytePositionInLine: {jsonException.BytePositionInLine}");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("Failed to get USPS rate estimate;{ex}", ex.Message);
            // Handle HTTP request errors here
            Console.WriteLine($"HttpRequestException: {ex.Message}");
            throw;
        }
    }
    private record UspsFee
    {
        [JsonPropertyName("name")]
        public string Name { get; init; }
        [JsonPropertyName("SKU")]
        public string SKU { get; init; }
        [JsonPropertyName("price")]
        public decimal Price { get; init; }
    }

    private record UspsRate
    {
        [JsonPropertyName("SKU")]
        public string SKU { get; init; }
        [JsonPropertyName("description")]
        public string Description { get; init; }
        [JsonPropertyName("priceType")]
        public string PriceType { get; init; }
        [JsonPropertyName("price")]
        public decimal Price { get; init; }
        [JsonPropertyName("weight")]
        public decimal Weight { get; init; }
        [JsonPropertyName("dimWeight")]
        public int DimWeight { get; init; }
        [JsonPropertyName("fees")]
        public List<UspsFee> Fees { get; init; }
        [JsonPropertyName("mailClass")]
        public string MailClass { get; init; }
        [JsonPropertyName("zone")]
        public string Zone { get; init; }
        [JsonPropertyName("productName")]
        public string ProductName { get; init; }
        [JsonPropertyName("productDefinition")]
        public string ProductDefinition { get; init; }
        [JsonPropertyName("processingCategory")]
        public string ProcessingCategory { get; init; }
        [JsonPropertyName("rateIndicator")]
        public string RateIndicator { get; init; }
        [JsonPropertyName("destinationEntryFacilityType")]
        public string DestinationEntryFacilityType { get; init; }
        [JsonPropertyName("warnings")]
        public List<object> Warnings { get; init; }
    }

    private record UspsPriceData
    {
        [JsonPropertyName("totalBasePrice")]
        public decimal TotalBasePrice { get; init; }
        [JsonPropertyName("rates")]
        public List<UspsRate> Rates { get; init; }
    }
    private record UspsVoidLabelResponse
    {
        [JsonPropertyName("trackingNumber")]
        public string TrackingNumber { get; init; }
        [JsonPropertyName("status")]
        public UspsLabelStatus UspsLabelStatus { get; init; }
        [JsonPropertyName("disputeId")]
        public string DisputeId { get; init; }
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    private enum UspsLabelStatus
    {
        CANCELED,
        DISPUTED
    }
}