using ERPWebApp.Data.DTOModels;
using ERPWebApp.Middleware;
using ERPWebApp.Models;
using ERPWebApp.Models.Common;
using ERPWebApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Serialization;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using static ERPWebApp.Data.DTOModels.ZazzleDTO;
using static ERPWebApp.Models.Orders.Order;
using JsonSerializer = System.Text.Json.JsonSerializer;
using ERPWebApp.Models.Orders;
using System.Collections.Immutable;

namespace ERPWebApp.Data;

/// <summary>  
/// Provides a base class for API controllers with common properties and methods.  
/// </summary>  
public abstract class ApiCall(IWebhookBatchService webhookBatchService) : Controller
{
    /// <summary>  
    /// A static HttpClient instance used to send HTTP requests.  
    /// </summary> 
    private static readonly HttpClient _client = new();
    /// <summary>  
    /// The current date and time in the Central Standard Time timezone.  
    /// </summary>
    protected readonly DateTime _now = TimeZoneInfo.ConvertTime(
        DateTime.Now,
        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
    );

    private async Task<HttpResponseMessage> HttpSendMessageInternal(
        Uri requestUri,
        HttpMethod httpMethod,
        AuthenticationHeaderValue authenticationHeaderValue = null,
        Dictionary<string, string> headers = null,
        StringContent stringContent = null,
        FormUrlEncodedContent formUrlEncodedContent = null,
        int maxRetries = 3
    )
    {
        var webHookBatch = new WebHookBatch
        {
            CreateDate = _now,
            WebhookURL = requestUri.ToString(),
            RequestHeaders = headers != null ? JsonConvert.SerializeObject(headers) : null,
            RequestBody = stringContent?.ReadAsStringAsync().Result,
        };

        HttpResponseMessage httpResponse = new();

        for (int retryCount = 0; retryCount < maxRetries; retryCount++)
        {
            try
            {
                httpResponse = await SendHttpRequest(requestUri, httpMethod, authenticationHeaderValue, headers, stringContent, formUrlEncodedContent);

                webHookBatch.ResponseStatus = (int)httpResponse.StatusCode;
                webHookBatch.ResponseHeaders = httpResponse.Headers.ToString();
                webHookBatch.ResponseBody = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    break;
                }
                else if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    break;
                }
                else if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    break;
                }
                // Check if the response status code is 429
                else if (httpResponse.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    // Check if the X-Rate-Limit-Reset header exists
                    if (httpResponse.Headers.TryGetValues("X-Rate-Limit-Reset", out var headerValues))
                    {
                        // Parse the header value to a double
                        if (double.TryParse(headerValues.FirstOrDefault(), out double resetTime))
                        {
                            // Convert the reset time to a TimeSpan and wait before the next retry
                            TimeSpan delay = TimeSpan.FromSeconds(resetTime + 1);
                            await Task.Delay(delay);
                        }
                        else
                        {
                            // If the header value cannot be parsed, wait for 25 seconds by default
                            await Task.Delay(TimeSpan.FromSeconds(25));
                        }
                    }
                    else
                    {
                        // If the header does not exist, wait for 25 seconds by default
                        await Task.Delay(TimeSpan.FromSeconds(25));
                    }
                }
            }
            catch (Exception ex)
            {
                webHookBatch.ErrorMessage = ex.Message;
                webHookBatch.ErrorStackTrace = ex.StackTrace;
                break;
            }

            webHookBatch.RetryCount++;
        }

        webhookBatchService.Update(webHookBatch);

        return httpResponse;
    }


    /// <summary>  
    /// Sends an HTTP request and retries if necessary.  
    /// </summary>  
    /// <param name="requestUri">The request URI.</param>  
    /// <param name="httpMethod">The HTTP method to use for the request.</param>  
    /// <param name="authenticationHeaderValue">Optional authentication header for the request.</param>  
    /// <param name="headers">Optional headers for the request.</param>  
    /// <param name="stringContent">Optional content for the request.</param>
    /// <param name="formUrlEncodedContent"></param>  
    /// <param name="maxRetries">The maximum number of retries for the request.</param>  
    /// <returns>A string containing the response content, if the request is successful.</returns>
    protected async Task<HttpResponseMessage> HttpSendMessage(
        Uri requestUri,
        HttpMethod httpMethod,
        AuthenticationHeaderValue authenticationHeaderValue = null,
        Dictionary<string, string> headers = null,
        StringContent stringContent = null,
        FormUrlEncodedContent formUrlEncodedContent = null,
        int maxRetries = 3
    )
    {
        var httpResponseMessage = await HttpSendMessageInternal(requestUri, httpMethod, authenticationHeaderValue, headers, stringContent, formUrlEncodedContent, maxRetries);

        return httpResponseMessage;
    }
    /// <summary>  
    /// Sends an HTTP request and retries if necessary.  
    /// </summary>  
    /// <param name="requestUri">The request URI.</param>  
    /// <param name="httpMethod">The HTTP method to use for the request.</param>  
    /// <param name="authenticationHeaderValue">Optional authentication header for the request.</param>  
    /// <param name="headers">Optional headers for the request.</param>  
    /// <param name="stringContent">Optional content for the request.</param>
    /// <param name="formUrlEncodedContent"></param>  
    /// <param name="maxRetries">The maximum number of retries for the request.</param>  
    /// <returns>The full WebhookBatch object</returns>
    public async Task<HttpResponseMessage> HttpSendMessageAsWebHookBatch(
        [FromRoute] Uri requestUri,
        [FromRoute] HttpMethod httpMethod,
        [FromRoute] AuthenticationHeaderValue authenticationHeaderValue = null,
        [FromQuery] Dictionary<string, string> headers = null,
        [FromBody] StringContent stringContent = null,
        [FromQuery] int maxRetries = 3
    )
    {
        return await HttpSendMessageInternal(requestUri, httpMethod, authenticationHeaderValue, headers, stringContent, null, maxRetries);
    }



    /// <summary>  
    /// Sends an HTTP request using the specified parameters.  
    /// </summary>  
    /// <param name="requestUri">The request URI.</param>  
    /// <param name="httpMethod">The HTTP method to use for the request.</param>  
    /// <param name="authenticationHeaderValue">Optional authentication header for the request.</param>  
    /// <param name="headers">Optional headers for the request.</param>  
    /// <param name="stringContent">Optional content for the request.</param>
    /// <param name="formUrlEncodedContent">Optional encoded content for the request.</param>  
    /// <returns>An HttpResponseMessage containing the response data.</returns>
    private static async Task<HttpResponseMessage> SendHttpRequest(
        Uri requestUri,
        HttpMethod httpMethod,
        AuthenticationHeaderValue authenticationHeaderValue = null,
        Dictionary<string, string> headers = null,
        StringContent stringContent = null,
        FormUrlEncodedContent formUrlEncodedContent = null
    )
    {
        var request = new HttpRequestMessage
        {
            Method = httpMethod,
            RequestUri = requestUri
        };
        if (authenticationHeaderValue != null)
        {
            request.Headers.Authorization = authenticationHeaderValue;
        }

        if (headers != null)
        {
            foreach (var headervalue in headers)
            {
                request.Headers.Add(headervalue.Key, headervalue.Value);
            }
        }

        if (stringContent != null)
        {
            request.Content = stringContent;
        }
        else if (formUrlEncodedContent != null)
        {
            request.Content = formUrlEncodedContent;
        }

        return await _client.SendAsync(request);
    }
}


/// <summary>  
/// Provides webhook API endpoints for processing updates from Webhook APIs.  
/// </summary>
/// <remarks>  
/// Initializes a new instance of the WebhookApi class with the specified context and configuration.  
/// </remarks>
/// <param name="webhookBatchService"></param>  
/// <param name="configuration">The IConfiguration instance.</param>
/// <param name="orderService">The OrderService instance.</param>
/// <param name="shippingScanoutService">The ShippingScanoutService instance.</param>
[ApiController]
[AllowAnonymous]
[Route("api/[action]")]
[CwaFeatureGate(CwaFeatures.WEBHOOKS)]
public class WebhookApi(
    IWebhookBatchService webhookBatchService,
    IConfiguration configuration,
    IOrderService orderService,
    IOrderShippingService orderShippingService,
    IShippingScanoutService shippingScanoutService,
    IOrderBatchService orderBatchService,
    IUserService userService
) : ApiCall(webhookBatchService)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IOrderService _orderService = orderService;
    private readonly IOrderShippingService _orderShippingService = orderShippingService;
    private readonly IShippingScanoutService _shippingScanoutService = shippingScanoutService;
    private readonly IOrderBatchService _orderBatchService = orderBatchService;
    private readonly IUserService _userService = userService;

    /// <summary>  
    /// Receives updates from ShipStation and processes them accordingly.  
    /// </summary>  
    /// <param name="payload">The webhook payload from ShipStation.</param>  
    /// <returns>An IActionResult indicating the success or failure of the operation.</returns>
    [HttpPost]
    public async Task<IActionResult> ReceiveShipStationOrderUpdate([FromBody] ShipStationWebhookDTO payload)
    {

        if (string.IsNullOrEmpty(payload.resource_type) || string.IsNullOrEmpty(payload.resource_url))
        {
            return BadRequest(new { error = $"Payload is missing required fields: {nameof(payload.resource_type)} or {nameof(payload.resource_url)}" });
        }

        try
        {
            bool hasMorePages;
            do
            {
                var httpResponse = await HttpSendMessage(
                        new Uri(payload.resource_url),
                        HttpMethod.Get,
                        new AuthenticationHeaderValue("Basic", _configuration["ShipStationAuth"]),
                        new Dictionary<string, string>() { { "x-partner", _configuration["ShipStationXKey"] } }
                        );
                var responseBody = await httpResponse.Content.ReadAsStringAsync();
                ShipStationWebhookOrderDTO shipStationWebhookOrder = JsonConvert.DeserializeObject<ShipStationWebhookOrderDTO>(responseBody);

                switch (payload.resource_type)
                {
                    case "ORDER_NOTIFY":
                        // Process Order Notify payload  
                        await _orderService.ProcessOrderNotify(shipStationWebhookOrder.Orders).ConfigureAwait(false);
                        break;

                    case "ITEM_SHIP_NOTIFY":
                        // Process Item Ship Notify payload  
                        await _orderService.ProcessItemShipNotify(shipStationWebhookOrder.Shipments).ConfigureAwait(false);
                        break;

                    case "FULFILLMENT_SHIPPED":
                        // Process Fulfillment Shipped payload  
                        await _orderService.ProcessFulfillmentShipped(shipStationWebhookOrder.Fulfillments).ConfigureAwait(false);
                        break;

                    default:
                        throw new NotSupportedException($"Unsupported payload resource type: {payload.resource_type}");
                }

                hasMorePages = shipStationWebhookOrder.Page < shipStationWebhookOrder.Pages;
                if (hasMorePages)
                {
                    payload.resource_url = shipStationWebhookOrder.Page == 1
                        ? payload.resource_url + "&page=" + ++shipStationWebhookOrder.Page
                        : payload.resource_url.Replace("&page=" + shipStationWebhookOrder.Page, "&page=" + ++shipStationWebhookOrder.Page);
                }

            } while (hasMorePages);
            return Ok("Successfully Updated/Added Order");
        }
        catch (Exception)
        {
            throw;
        }

    }

    [HttpPost]
    public async Task<IActionResult> ForceUpdateOrders([FromBody] ShipStationOrderFilter parameterPayload)
    {
        try
        {
            await _orderService.ForceUpdateOrders(parameterPayload);
            return Ok("Successfully Updated/Added Order");
        }
        catch (Exception ex)
        {
            //return an appropriate error response to the client
            return StatusCode(500, new { error = "An error occurred while processing the request.\n" + ex });
        }
    }
    /// <summary>
    /// Pulls the tracking number from the payload and creates a new ShippingScanout record.
    /// </summary>
    /// <param name="payload">ShippingScanout payload </param>
    /// <returns>An IActionResult indicating the success or failure of the operation.</returns>
    [HttpPost]
    public async Task<IActionResult> ReceiveTrackingNumberScan([FromBody] ShippingScanout payload)
    {
        if (payload.ScannedTrackingNumber == null || payload.ScannedTrackingNumber == "")
        {
            return BadRequest(new { error = $"Payload is missing required fields: {nameof(payload.ScannedTrackingNumber)}" });
        }
        try
        {
            payload.CreatedBy = "Automation";
            await _shippingScanoutService.CreateNewShippingScanout(payload);
        }
        catch (Exception ex)
        {
            //return an appropriate error response to the client
            return StatusCode(500, new { error = "An error occurred while processing the request.\n" + ex });
        }
        return Ok("Successfully Created Scanout");
    }
    [HttpPost]
    public async Task<IActionResult> ShipScannedOrderNumber([FromBody] JsonObject payload)
    {
        try
        {
            string orderNumber = payload["OrderNumber"].GetValue<string>();
            if (!orderNumber.Any() || orderNumber.Contains("^#^"))
            {
                return BadRequest();
            }

            Order scannedOrder = await _orderService.GetOrderByOrderNumberAsync(orderNumber);
            OrderWeight orderWeight = null;
            if (payload.TryGetPropertyValue("Weight", out var weightJson) && payload.TryGetPropertyValue("WeightUnits", out var weightUnitsJson))
            {
                orderWeight = new OrderWeight
                {
                    value = weightJson.GetValue<decimal>(),
                    WeightUnits = weightUnitsJson.GetValue<string>().ToLowerInvariant() switch
                    {
                        "pounds" => 0,
                        "ounces" => 1,
                        "grams" => 2,
                        _ => throw new NotImplementedException()
                    },
                    units = weightUnitsJson.GetValue<string>().ToLowerInvariant() switch
                    {
                        "pounds" => OrderWeight.Units.pounds,
                        "ounces" => OrderWeight.Units.ounces,
                        "grams" => OrderWeight.Units.grams,
                        _ => throw new NotImplementedException()
                    }
                };
            }
            OrderDimensions orderDimensions = null;
            if (payload.TryGetPropertyValue("Height", out var heightJson) && payload.TryGetPropertyValue("Width", out var widthJson) && payload.TryGetPropertyValue("Length", out var lengthJson)
                && payload.TryGetPropertyValue("DimUnits", out var dimUnitsJson))
            {
                orderDimensions = new()
                {
                    height = heightJson.GetValue<decimal>(),
                    width = widthJson.GetValue<decimal>(),
                    length = lengthJson.GetValue<decimal>(),
                    units = dimUnitsJson.GetValue<string>().ToLowerInvariant() switch
                    {
                        "inches" => OrderDimensions.Units.inches,
                        "centimeters" => OrderDimensions.Units.centimeters,
                        _ => throw new NotImplementedException()
                    }
                };
            }
            IdentityUser user = new IdentityUser();
            if (payload.TryGetPropertyValue("UserId", out var userIdJson) && userIdJson.GetValue<string>().Any())
            {
                user = _userService.Get(u => u.Id == userIdJson.GetValue<string>());
            }

            string orderSource = payload["OrderSource"]?.GetValue<string>();

            //Need to parse the order number into a long from a string.
            var orderNumberString = payload["order_number"]?.GetValue<string>();
            long orderId = 0;
            if (!string.IsNullOrEmpty(orderNumberString) && long.TryParse(orderNumberString, out long parsedOrderId))
            {
                orderId = parsedOrderId;
            }

            if (orderSource == "skulabs")
            {
                var skulabsDto = System.Text.Json.JsonSerializer.Deserialize<SkulabsDTO>(payload.ToString());
                string requiredLocationId = _configuration["ERPSkulabsLocationId"];
                var order = _orderService.MapSkulabsDtoToOrder(skulabsDto, requiredLocationId);

                await _orderShippingService.GenerateLabelShipEngine(order, order.userId);
                return Ok();
            }
            else
            {
                switch (scannedOrder.orderStatus)
                {
                    case OrderStatus.shipped:
                        return BadRequest(new { error = "Order already shipped" });
                    case OrderStatus.cancelled:
                        return BadRequest(new { error = "Can not ship a cancelled order" });
                    default:
                        {
                            scannedOrder.dimensions = orderDimensions ?? scannedOrder.dimensions;
                            scannedOrder.weight = orderWeight ?? scannedOrder.weight;
                            scannedOrder.userId = user.Id ?? scannedOrder.userId;
                            if (scannedOrder.dimensions == null || scannedOrder.dimensions == default ||
                                scannedOrder.dimensions.length * scannedOrder.dimensions.width * scannedOrder.dimensions.height <= 1)
                            {
                                scannedOrder = _orderService.SetOrderDimensionsFromItems(scannedOrder);
                            }

                            if (scannedOrder.weight == null || scannedOrder.weight == default || scannedOrder.weight.value == 0)
                            {
                                scannedOrder = _orderService.SetOrderWeightFromItems(scannedOrder);
                            }

                            scannedOrder = scannedOrder.Sources.FirstOrDefault().Name switch
                            {
                                OrderSourceEnum.zazzle => await _orderShippingService.GenerateLabelZazzle(scannedOrder, scannedOrder.userId),
                                OrderSourceEnum.shipstation or OrderSourceEnum.orderdesk or OrderSourceEnum.custom or OrderSourceEnum.skulabs => await _orderShippingService.GenerateLabelShipEngine(scannedOrder, scannedOrder.userId),
                                _ => throw new NotImplementedException()
                            };
                            if (!scannedOrder.orderShipments.Any())
                            {
                                return BadRequest(new { error = "Shipment creation failed" });
                            }

                            OrderShipment orderShipment = scannedOrder.orderShipments.FirstOrDefault();
                            if (!orderShipment.labelData.Any())
                            {
                                return BadRequest(new { error = "Failed to load label data" });
                            }

                            return Ok(orderShipment.labelData);
                        }
                }
            }
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpGet, HttpPost]
    public async Task<IActionResult> GetScannedOrderNumberDetails([FromBody] JsonObject payload)
    {
        try
        {
            string orderNumber = payload["OrderNumber"].GetValue<string>();
            if (!orderNumber.Any() || orderNumber.Contains("^#^"))
            {
                return BadRequest(new { error = "Invalid Scan" });
            }

            Order scannedOrder = await _orderService.GetOrderByOrderNumberAsync(orderNumber);
            if (scannedOrder == null)
            {
                return BadRequest(new { error = "Order number not found" });
            }

            return Ok(scannedOrder);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });

        }
    }
    [HttpGet]
    public async Task<IActionResult> GetOrderNumberDetails(string orderNumber)
    {
        return await GetScannedOrderNumberDetails(new JsonObject { ["OrderNumber"] = orderNumber });
    }

    public async Task<IActionResult> GetLloydsOfLindonZazzleOrderStatus([FromBody] JsonObject payload)
    {
        string orderNumber = payload["OrderNumber"].GetValue<string>();
        if (!orderNumber.Any() || orderNumber.Contains("^#^"))
        {
            return BadRequest(new { error = "Invalid Scan" });
        }

        var httpResponse = await HttpSendMessageAsWebHookBatch(
            new Uri("https://lloydsoflindon.com/zazzle/api/status?order-number=" + orderNumber),
            HttpMethod.Get
            );
        var responseBody = await httpResponse.Content.ReadAsStringAsync();
        return Ok(responseBody);
    }
    public async Task<IActionResult> GetLloydsOfLindonZazzleCriticalOrders()
    {
        var httpResponse = await HttpSendMessageAsWebHookBatch(
            new Uri("https://lloydsoflindon.com/zazzle/api/critical?days=3"),
            HttpMethod.Get
            );
        var responseBody = await httpResponse.Content.ReadAsStringAsync();
        return Ok(responseBody);
    }
    public async Task<IActionResult> GenerateLloydsOfLindonZazzleOrderLabel([FromBody] JsonObject payload)
    {
        string orderNumber = payload["OrderNumber"].GetValue<string>();
        if (!orderNumber.Any() || orderNumber.Contains("^#^"))
        {
            return BadRequest(new { error = "Invalid Scan" });
        }

        var httpResponse = await HttpSendMessageAsWebHookBatch(
            new Uri("https://lloydsoflindon.com/zazzle/api/generate-label?order-number=" + orderNumber),
            HttpMethod.Get
            );
        var responseBody = await httpResponse.Content.ReadAsStringAsync();
        return Ok(responseBody);
    }

    [HttpPost]
    public async Task<IActionResult> GetShopifyOrderDetails([FromBody] ShopifyDTO payload)
    {
        try
        {
            var conversion = await _orderService.ConvertShopifyToOrder(payload);
            return Ok(new { message = "Order details processed successfully" });
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    public async Task<IActionResult> GetOrderBatchItemsByOrderBatchId([FromBody] JsonObject payload)
    {
        try
        {
            int OrderBatchId = payload["OrderBatchId"].GetValue<int>();
            List<OrderBatchItem> orderBatchItems = await _orderBatchService.GetOrderBatchItemsByOrderBatchId(OrderBatchId);
            if (!orderBatchItems.Any())
            {
                return BadRequest(new { error = "No items found for this Batch." });
            }

            return Ok(orderBatchItems);
        }
        catch (Exception ex) when (ex is FormatException or InvalidOperationException)
        {

            return BadRequest(new { error = $"Payload is malformed" });
        }
        catch (Exception Ex)
        {
            return Problem(Ex.Message);
        }
    }
    [HttpPost]
    public async Task<IActionResult> UpdateOrderBatchProgress([FromBody] JsonObject payload)
    {
        try
        {
            int orderBatchItemId = payload["OrderBatchItemId"].GetValue<int>();
            int? desiredBatchStatusId = payload.ContainsKey("DesiredBatchStatusId") ? payload["DesiredBatchStatusId"].GetValue<int?>() : null;
            OrderBatchItem orderBatchItem = await _orderBatchService.UpdateOrderBatchProgressAsync(orderBatchItemId, desiredBatchStatusId);
            return Ok(orderBatchItem);
        }
        catch (Exception ex) when (ex is FormatException or InvalidOperationException)
        {
            return BadRequest(new { error = $"Payload is malformed" });
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    public async Task<IActionResult> GetZazzleOrders()
    {
        var getHash = MD5.HashData(Encoding.UTF8.GetBytes(_configuration["Zazzle:VendorId"] + _configuration["Zazzle:SecretKey"]));
        var sBuilder = new StringBuilder();
        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        for (int i = 0; i < getHash.Length; i++)
        {
            sBuilder.Append(getHash[i].ToString("x2"));
        }
        var httpResponse = await HttpSendMessageAsWebHookBatch(
             new Uri($"https://vendor.Zazzle.com/v100/api.aspx?method=listneworders&vendorid={_configuration["Zazzle:VendorId"]}&hash={sBuilder}&rev=2"),
             HttpMethod.Get
             );
        var responseBody = await httpResponse.Content.ReadAsStringAsync();
        ZazzleDTO zazzleDTO = new()
        {
            VendorId = _configuration["Zazzle:VendorId"],
            zazzleRequest = new()
        };
        try
        {
            var serializer = new XmlSerializer(typeof(ZazzleRequest.Response));
            using TextReader reader = new StringReader(responseBody);
            zazzleDTO.zazzleRequest.response = (ZazzleRequest.Response)serializer.Deserialize(reader);
            Console.WriteLine(zazzleDTO.zazzleRequest.response.responseStatus);
            await _orderService.ConvertZazzleToOrder(zazzleDTO);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
        return Ok(zazzleDTO);
    }
    public async Task<IActionResult> GetZazzleOrderUpdates()
    {

        var getHash = MD5.HashData(Encoding.UTF8.GetBytes(_configuration["Zazzle:VendorId"] + _configuration["Zazzle:SecretKey"]));
        var sBuilder = new StringBuilder();
        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        for (int i = 0; i < getHash.Length; i++)
        {
            sBuilder.Append(getHash[i].ToString("x2"));
        }
        var httpResponse = await HttpSendMessageAsWebHookBatch(
             new Uri($"https://vendor.Zazzle.com/v100/api.aspx?method=listupdatedorders&vendorid={_configuration["Zazzle:VendorId"]}&hash={sBuilder}&rev=2"),
             HttpMethod.Get
             );
        var responseBody = await httpResponse.Content.ReadAsStringAsync();
        ZazzleDTO zazzleDTO = new()
        {
            VendorId = _configuration["Zazzle:VendorId"],
            zazzleRequest = new()
        };
        try
        {
            var serializer = new XmlSerializer(typeof(ZazzleRequest.Response));
            using TextReader reader = new StringReader(responseBody);
            zazzleDTO.zazzleRequest.response = (ZazzleRequest.Response)serializer.Deserialize(reader);
            Console.WriteLine(zazzleDTO.zazzleRequest.response.responseStatus);
            await _orderService.ConvertZazzleToOrder(zazzleDTO);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message + " : " + ex.InnerException);
        }

        return Ok(zazzleDTO);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDepartmentStatuses([FromBody] JsonObject payload)
    {
        try
        {
            List<DepartmentStatusDto> departmentStatuses = await _orderBatchService.GetDepartmentStatusesAsync();
            return Ok(departmentStatuses);

        }
        catch (Exception ex) when (ex is FormatException or InvalidOperationException)
        {
            return BadRequest(new { error = $"Payload is malformed" });
        }
        catch (Exception Ex)
        {
            return Problem(Ex.Message);
        }
    }
    [HttpPost]
    public async Task<IActionResult> AddorRemoveShipStationTagAsync([FromBody] JsonObject payload)
    {
        try
        {
            long orderId = payload["OrderId"].GetValue<long>();
            int tagId = payload["TagId"].GetValue<int>();
            bool AddOrRemoveTag = payload["AddOrRemoveTag"].GetValue<bool>();
            string requestBody = new JsonObject
            {
                ["orderId"] = orderId,
                ["tagId"] = tagId
            }.ToString();
            var httpResponse = await HttpSendMessage(
                new Uri("https://ssapi.shipstation.com/orders/" + ((AddOrRemoveTag == true) ? "addtag" : "removetag")),
                HttpMethod.Post,
                new AuthenticationHeaderValue("Basic", _configuration["ShipStationAuth"]),
                new Dictionary<string, string>() { { "x-partner", _configuration["ShipStationXKey"] } },
                new StringContent(requestBody, Encoding.Default, "application/json")
                );
            var responseBody = await httpResponse.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject(responseBody);
            if (!httpResponse.IsSuccessStatusCode)
            {
                return BadRequest();
            }

            var response = JObject.FromObject(deserializedResponse);
            return Ok(response);
        }
        catch (Exception ex) when (ex is FormatException or InvalidOperationException)
        {
            return BadRequest(new { error = $"Payload is malformed" });
        }
        catch (Exception Ex)
        {
            return Problem(Ex.Message);
        }
    }
}
/// <summary>  
/// Provides methods for working with various webhooks and related services.  
/// </summary>  
public interface IWebhooks
{
    public Task<Order> CreateShipStationOrder(string jsonBody);
    /// <summary>  
    /// Retrieves the OSM estimated shipment rate based on the provided parameters.  
    /// </summary>  
    /// <param name="destinationZipCode">The destination zip code.</param>  
    /// <param name="weight">The weight of the shipment.</param>  
    /// <param name="weightUnits">The weight units (e.g., "pounds" or "ounces").</param>  
    /// <param name="length">The length of the shipment.</param>  
    /// <param name="height">The height of the shipment.</param>  
    /// <param name="width">The width of the shipment.</param>  
    /// <param name="dimensionUnits">The dimension units (e.g., "inches" or "centimeters").</param>  
    /// <returns>An IActionResult containing the OSM estimated shipment rate, or an error if the operation fails.</returns>  
    //Task<IActionResult> GetOsmEstimatedShipmentRate(string destinationZipCode, decimal weight, string weightUnits, decimal length, decimal height, decimal width, string dimensionUnits);

    /// <summary>  
    /// Sends a message to the specified employees.  
    /// </summary>  
    /// <param name="Message">The message to send.</param>  
    /// <param name="SendTo">A dictionary containing the email addresses and names of the recipients.</param>  
    /// <param name="SendFrom">The email address of the sender.</param>  
    void SendMessageToEmployee(string Message, Dictionary<string, string> SendTo, string SendFrom);

    Task<IActionResult> AcknowledgeZazzleOrder(string orderId, string acknowledgeType);
    Task<IActionResult> CreateZazzleLabel(string orderId, OrderWeight weight, OrderDimensions dimensions);
    Task<IActionResult> AcknowledgeShopifyOrder(string orderId, string acknowledgeType);

    Task<IActionResult> CreateShopifyLabel(string orderId, OrderWeight weight, OrderDimensions dimensions);
    Task<IActionResult> SkulabsAddManualShipment([FromBody] SkulabsDTO request);
    Task<T> GenerateManifest<T>(string warehouseId, string carrierId, DateTime shipDate);
    Task DownloadManifest(string fileUrl);
    string GenerateUloRequestBody(string trailerNumber, string trackingNumbersEvents);
    Task<(string, string)> SendUPSListAddOrRemoveFromUloRequest(string trailerNumber, string trackingNumberEvents);
};

/// <summary>  
/// Provides methods for working with various webhooks and related services.  
/// </summary>  
[AllowAnonymous]
[CwaFeatureGate(CwaFeatures.WEBHOOKS)]
internal class Webhooks : ApiCall, IWebhooks
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, string> passwordDictionary;
    private readonly ILogger<Webhooks> _logger;
    /// <summary>  
    /// Initializes a new instance of the Webhooks class with the specified configuration and context.  
    /// </summary>
    /// <param name="webhookBatchService">The IWebhookBatchService instance.</param>  
    /// <param name="configuration">The IConfiguration instance.</param>  
    /// /// <param name="logger">The ILogger instance.</param>  
    public Webhooks(
        IWebhookBatchService webhookBatchService,
        IConfiguration configuration,
        ILogger<Webhooks> logger
    ) : base(webhookBatchService)
    {
        _configuration = configuration;
        _logger = logger;
        passwordDictionary = new Dictionary<string, string>() {
            { "SendGridAccount",_configuration["SendGridAccount"]},
            { "SendGridPassword",_configuration["SendGridPassword"]},
            { "ShipStationAuth",_configuration["ShipStationAuth"]},
            { "ShipStationXKey",_configuration["ShipStationXKey"]},
            { "ShipEngineAuth",_configuration["ShipEngineAuth"]},
            { "DeputyAuth",_configuration["DeputyAuth"]},
            { "UPSUserName",_configuration["UPSUserName"]},
            { "UPSPassword",_configuration["UPSPassword"]},
            { "UPSALN",_configuration["UPSALN"]},
            { "UPSCTRKey",_configuration["UPSCTRKey"]},
            { "DHLClientId",_configuration["DHLAuth:client_id"]},
            { "DHLClientSecret",_configuration["DHLAuth:client_secret"]},
            { "DHLAccessToken",_configuration["DHLAuth:access_token"]},
            { "DHLClientPickupId", _configuration["DHLAuth:ClientPickupId"]},
            { "DHLDistributionCenter", _configuration["DHLAuth:DistributionCenter"]},
            { "ZazzleVendorId",_configuration["Zazzle:VendorId"]},
            { "ZazzleSecretKey",_configuration["Zazzle:SecretKey"]},
            { "SkuLabsAuth", _configuration["SkuLabsAuth"]}
        };
    }

    /// <summary>  
    /// Sends a message to the specified employees.  
    /// </summary>  
    /// <param name="Message">The message to send.</param>  
    /// <param name="SendTo">A dictionary containing the email addresses and names of the recipients.</param>  
    /// <param name="SendFrom">The email address of the sender.</param> 
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IActionResult))]
    public void SendMessageToEmployee(string Message, Dictionary<string, string> SendTo, string SendFrom)
    {
        string FullMessage =
            "Company Message From "
            + SendFrom
            + "\n"
            + Message
            + "\n Please do not respond to this message. Contact your superviser for any questions.";
        TwilioClient.Init(passwordDictionary.GetValueOrDefault("SendGridAccount"), passwordDictionary.GetValueOrDefault("SendGridPassword"));

        foreach (var person in SendTo)
        {
            MessageResource.Create(
                from: new PhoneNumber("+1(337)541-2364"),
                to: new PhoneNumber(person.Key),
                body: FullMessage
            );
            Console.WriteLine($"Sent message to {person.Value}");
        }
    }

    
    public async Task<IActionResult> CreateShipEngineManifest(List<string> labelIds)
    {
        JsonObject requestBody = new()
        {
            ["label_ids"] = new JsonArray { labelIds }
        };
        // initialize the HttpRequestMessage
        var httpResponse = await HttpSendMessage(
            new Uri("https://api.shipengine.com/v1/Manifests"),
            HttpMethod.Post,
            null,
            new Dictionary<string, string>() { { "API-Key", passwordDictionary.GetValueOrDefault("ShipEngineAuth") } },
            new StringContent(requestBody.ToString(), Encoding.Default, "application/json")
            );
        var responseBody = await httpResponse.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(responseBody);
        if (!httpResponse.IsSuccessStatusCode)
        {
            return BadRequest();
        }

        return Ok(jsonResponse);
    }

    /// <summary>  
    /// Retrieves the OSM estimated shipment rate based on the provided parameters.  
    /// </summary>  
    /// <param name="destinationZipCode">The destination zip code.</param>  
    /// <param name="weight">The weight of the shipment.</param>  
    /// <param name="weightUnits">The weight units (e.g., "pounds" or "ounces").</param>  
    /// <param name="length">The length of the shipment.</param>  
    /// <param name="height">The height of the shipment.</param>  
    /// <param name="width">The width of the shipment.</param>  
    /// <param name="dimensionUnits">The dimension units (e.g., "inches" or "centimeters").</param>  
    /// <returns>An IActionResult containing the OSM estimated shipment rate, or an error if the operation fails.</returns> 
    //[HttpPost]
    //[ProducesResponseType(200, Type = typeof(IActionResult))]
    //public async Task<IActionResult> GetOsmEstimatedShipmentRate(
    //    string destinationZipCode,
    //    decimal weight,
    //    string weightUnits,
    //    decimal length,
    //    decimal height,
    //    decimal width,
    //    string dimensionUnits
    //)
    //{
    //    if (weightUnits == null || dimensionUnits == null)
    //    {
    //        _logger.LogWarning("Weight units or dimension units are null.");
    //        return BadRequest("Weight units or dimension units cannot be null.");
    //    }

    //    int serviceCodeId = 1; //DEFAULT Service Code ID for OSM
    //    int mailClassId;
    //    string postalCode = "70570";

    //    // ensure that weight is in lbs for the api call
    //    var weightInLbs = weightUnits switch
    //    {
    //        "ounces" or "ounce" or "oz" => weight / 16,
    //        "grams" or "gram" or "g" => weight * (decimal)0.0022,
    //        "pound" or "pounds" or "lbs" => weight,
    //        _ => throw new Exception("Unexpected weight units used. Cannot check weight correctly.")
    //    };

    //    switch (weightInLbs)
    //    {
    //        case >= 1:
    //            mailClassId = 1;//osm_parcel_select
    //            break;
    //        case < 1:
    //            mailClassId = 2;//osm_parcel_select_lightweight
    //            break;
    //    }

    //    // fix the zip code to 5 chars
    //    if (destinationZipCode.Length > 5)
    //    {
    //        destinationZipCode = destinationZipCode[..5];
    //    }

    //    // ensure that the dimensions are in inches:  as of right now,
    //    // database only has occurrences of inches, if other units are used later then account for conversion here

    //    string xmlBodyStr =
    //        @$"<Context>
    //        <RequestType>0</RequestType>
    //        <CostCenterId>0</CostCenterId>
    //        <Packages>
    //            <Package>
    //                <PkgNumber>1</PkgNumber>
    //                <ServiceLevel>{serviceCodeId}</ServiceLevel>
    //                <MailClass>{mailClassId}</MailClass>
    //                <OriginZip>{postalCode}</OriginZip>
    //                <DestinationZip>{destinationZipCode}</DestinationZip>
    //                <WeightLbs>{weightInLbs}</WeightLbs>
    //                <Length>{length}</Length>
    //                <Width>{width}</Width>
    //                <Height>{height}</Height>
    //            </Package>
    //        </Packages>
    //      </Context>
    //    ";

    //    byte[] xmlStringAsByteArray = Encoding.UTF8.GetBytes(xmlBodyStr);
    //    var base64XmlString = Convert.ToBase64String(xmlStringAsByteArray);

    //    try
    //    {
    //        var httpResponse = await HttpSendMessage(
    //            new Uri("https://domrate.osmworldwide.us/API/PackageEstimate"),
    //            HttpMethod.Post,
    //       new AuthenticationHeaderValue("Basic", passwordDictionary.GetValueOrDefault("ShipStationAuth")),
    //       new Dictionary<string, string>() {
    //           { "ClientId", "1294" },
    //           { "APIKey", "353b26ae-f8cd-3790" },
    //           { "RatingKey", "1767db11-a78d-5a0e" }
    //       },
    //       new StringContent(base64XmlString, Encoding.Default, "application/xml")
    //       );
    //        var responseBody = await httpResponse.Content.ReadAsStringAsync();
    //        var xmlDoc = new XmlDocument();
    //        xmlDoc.LoadXml(responseBody);
    //        var jsonResponse = JsonConvert.SerializeXmlNode(xmlDoc, Formatting.None, false);
    //        JObject responseJObject = JObject.Parse(jsonResponse);

    //        return Ok(responseJObject);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error occurred while getting OSM estimated shipment rate.");
    //        return StatusCode(StatusCodes.Status500InternalServerError, "OSM API request failed.");
    //    }
    //}

    public string GenerateUloRequestBody(string trailerNumber, string trackingNumbersEvents)
    {
        if (string.IsNullOrEmpty(_configuration["UPSUserName"]) ||
            string.IsNullOrEmpty(_configuration["UPSPassword"]) ||
            string.IsNullOrEmpty(_configuration["UPSALN"]) ||
            string.IsNullOrEmpty(_configuration["UPSCTRKey"]))
        {
            throw new Exception("UPS credentials not found in appsettings.json.");
        }
        else if (!trailerNumber.Any() || !trackingNumbersEvents.Any())
        {
            throw new Exception(!trailerNumber.Any() ? "Trailer number is null or empty." : "Tracking numbers and events is null or empty.");
        }

        return $@"<soapenv:Envelope  
                xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'  
                xmlns:v1='http://www.ups.com/XMLSchema/XOLTWS/UPSS/v1.0'  
                xmlns:v11='http://www.ups.com/XMLSchema/XOLTWS/Tundra/v1.0'  
                xmlns:v12='http://www.ups.com/XMLSchema/XOLTWS/Common/v1.0'>  
                <soapenv:Header>  
                    <v1:UPSSecurity>  
                        <v1:UsernameToken>  
                            <v1:Username>{_configuration["UPSUserName"]}</v1:Username>  
                            <v1:Password>{_configuration["UPSPassword"]}</v1:Password>  
                        </v1:UsernameToken>  
                        <v1:ServiceAccessToken>  
                            <v1:AccessLicenseNumber>{_configuration["UPSALN"]}</v1:AccessLicenseNumber>  
                        </v1:ServiceAccessToken>  
                    </v1:UPSSecurity>  
                </soapenv:Header>  
                <soapenv:Body>  
                    <v11:AddOrRemoveFromUloRequest>  
                        <v12:Request>  
                            <v12:RequestOption/>  
                            <v12:TransactionReference>  
                                <v12:CustomerContext/>  
                                <v12:TransactionIdentifier/>  
                            </v12:TransactionReference>  
                        </v12:Request>  
                        <v11:CommonTundraRequest>  
                            <v11:Location>COMPLLAFLA</v11:Location>  
                            <v11:UserName>CV_COMPLLA</v11:UserName>  
                            <v11:DeviceName>CVIS</v11:DeviceName>  
                            <v11:AppVersion>5.2.0.4</v11:AppVersion>  
                            <v11:Key>{_configuration["UPSCTRKey"]}</v11:Key>  
                            <v11:LoadKey>  
                                <v11:Discriminator>DOMESTIC</v11:Discriminator>  
                                <v11:Origin>  
                                    <v11:Country>US</v11:Country>  
                                    <v11:Slic>7058</v11:Slic>  
                                    <v11:SortCode>07</v11:SortCode>  
                                </v11:Origin>  
                                <v11:Processing>  
                                    <v11:Country>US</v11:Country>  
                                    <v11:Slic>7058</v11:Slic>  
                                    <v11:SortCode>07</v11:SortCode>  
                                </v11:Processing>  
                                <v11:Destination>  
                                    <v11:Country>US</v11:Country>  
                                    <v11:Slic>7050</v11:Slic>  
                                    <v11:SortCode>07</v11:SortCode>  
                                </v11:Destination>  
                                <v11:Uld>  
                                    <v11:Type>04</v11:Type>  
                                    <v11:Number>{trailerNumber}</v11:Number>  
                                </v11:Uld>  
                                <v11:Service>004</v11:Service>  
                                <v11:PkgLocRole>12</v11:PkgLocRole>  
                                <v11:Disposition>00</v11:Disposition>  
                                <v11:SortDate>{_now:yyyy-MM-dd}</v11:SortDate>  
                            </v11:LoadKey>
                    </v11:CommonTundraRequest>  
                        {trackingNumbersEvents}  
                    </v11:AddOrRemoveFromUloRequest>  
                </soapenv:Body>  
            </soapenv:Envelope>";
    }

    public async Task<(string, string)> SendUPSListAddOrRemoveFromUloRequest(string trailerNumber, string trackingNumberEvents)
    {
        try
        {
            List<string> deserializedResponses = new();
            string xml = GenerateUloRequestBody(trailerNumber, trackingNumberEvents.ToString());
            var httpResponse = await HttpSendMessageAsWebHookBatch(
                new Uri("https://onlinetools.ups.com/webservices/CVIS"),
                HttpMethod.Post, null, new Dictionary<string, string>()
                {
                    { "Access-Control-Allow-Headers", "Origin,X-Requested-With,Content-Type,Accept" },
                    { "Access-Control-Allow-Methods", "POST" },
                    { "Access-Control-Allow-Origin", "*" }
                },
                new StringContent(xml, Encoding.Default, "application/xml"));
            var responseBody = await httpResponse.Content.ReadAsStringAsync();
            return (httpResponse.Headers.ToString(), responseBody);
        }
        catch (Exception)
        {
            //_logger.LogError(ex, $"Error sending UPS Add or Remove from ULO request for trailer number {trailerNumber}");
            throw;
        }
    }

    /// <summary>
    /// This will create a new ship station order using ship station API end
    /// </summary>
    /// <param name="jsonBody">The json string for the Order object to send to the ship station.</param>
    /// <returns>This will returns true if the ship station order created successfully. Throws an error if it's failed.</returns>
    /// <exception cref="Exception">If the order creation failed through ship station API, this method will throw an exception.</exception>
    public async Task<Order> CreateShipStationOrder(string jsonBody)
    {
        var httpResponse = await HttpSendMessageAsWebHookBatch(
            new Uri("https://ssapi.shipstation.com/orders/createorder"),
            HttpMethod.Post,
            new AuthenticationHeaderValue("Basic", _configuration["ShipStationAuth"]),
            new Dictionary<string, string>() { { "x-partner", passwordDictionary.GetValueOrDefault("ShipStationXKey") },
                {"Access-Control-Allow-Headers","Origin,X-Requested-With,Content-Type,Accept" },
                { "Access-Control-Allow-Methods","POST"},
                {"Access-Control-Allow-Origin","*" }},
            new StringContent(jsonBody, Encoding.Default, "application/json"));
        var responseBody = await httpResponse.Content.ReadAsStringAsync();
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to create order in ShipStation. Status Code: {httpResponse.StatusCode}, Response: {responseBody}");
        }

        return JsonConvert.DeserializeObject<Order>(responseBody);
    }

    public async Task<IActionResult> AcknowledgeZazzleOrder(string orderId, string acknowledgeType)
    {
        var getHash = MD5.HashData(Encoding.UTF8.GetBytes(passwordDictionary["ZazzleVendorId"] + orderId + acknowledgeType + passwordDictionary["ZazzleSecretKey"]));
        var sBuilder = new StringBuilder();
        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        for (int i = 0; i < getHash.Length; i++)
        {
            sBuilder.Append(getHash[i].ToString("x2"));
        }
        var httpResponse = await HttpSendMessageAsWebHookBatch(
            new Uri($"https://vendor.Zazzle.com/v100/api.aspx?method=ackorder&vendorid={passwordDictionary["ZazzleVendorId"]}&orderId={orderId}&type={acknowledgeType}&action=accept&hash={sBuilder}"),
            HttpMethod.Get
            );
        var responseBody = await httpResponse.Content.ReadAsStringAsync();
        return Ok(responseBody);
    }

    public async Task<IActionResult> CreateZazzleLabel(string orderId, OrderWeight weight, OrderDimensions dimensions)
    {
        if (weight.units == OrderWeight.Units.ounces)
        {
            weight.units = OrderWeight.Units.pounds;
            weight.value /= 16;
        }
        if (dimensions.units == OrderDimensions.Units.centimeters)
        {
            dimensions.units = OrderDimensions.Units.inches;
            dimensions.height /= 2.54m;
            dimensions.length /= 2.54m;
            dimensions.width /= 2.54m;
        }
        var getHash = MD5.HashData(Encoding.UTF8.GetBytes(passwordDictionary["ZazzleVendorId"] + orderId + weight.value + "PDF" + passwordDictionary["ZazzleSecretKey"]));
        var sBuilder = new StringBuilder();
        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        for (int i = 0; i < getHash.Length; i++)
        {
            sBuilder.Append(getHash[i].ToString("x2"));
        }
        var httpResponse = await HttpSendMessageAsWebHookBatch(
            new Uri($"https://vendor.Zazzle.com/v100/api.aspx?method=getshippinglabel&vendorid={passwordDictionary["ZazzleVendorId"]}&orderid={orderId}&weight={weight.value}&format=PDF&hash={sBuilder}"),
            HttpMethod.Get
            );
        var responseBody = await httpResponse.Content.ReadAsStringAsync();

        return Ok(responseBody);
    }

    public async Task<IActionResult> AcknowledgeShopifyOrder(string orderId, string acknowledgeType)
    {
        var getHash = MD5.HashData(Encoding.UTF8.GetBytes(passwordDictionary["ShopifyVendorId"] + orderId + acknowledgeType + passwordDictionary["ShopifySecretKey"]));
        var sBuilder = new StringBuilder();
        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        for (int i = 0; i < getHash.Length; i++)
        {
            sBuilder.Append(getHash[i].ToString("x2"));
        }
        return Ok();
    }

    public async Task<IActionResult> CreateShopifyLabel(string orderId, OrderWeight weight, OrderDimensions dimensions)
    {
        if (weight.units == OrderWeight.Units.ounces)
        {
            weight.units = OrderWeight.Units.pounds;
            weight.value /= 16;
        }
        if (dimensions.units == OrderDimensions.Units.centimeters)
        {
            dimensions.units = OrderDimensions.Units.inches;
            dimensions.height /= 2.54m;
            dimensions.length /= 2.54m;
            dimensions.width /= 2.54m;
        }
        var getHash = MD5.HashData(Encoding.UTF8.GetBytes(passwordDictionary["ShopifyVendorId"] + orderId + weight.value + "PDF" + passwordDictionary["ShopifySecretKey"]));
        var sBuilder = new StringBuilder();
        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        for (int i = 0; i < getHash.Length; i++)
        {
            sBuilder.Append(getHash[i].ToString("x2"));
        }
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> SkulabsAddManualShipment([FromBody] SkulabsDTO request)
    {
        if (string.IsNullOrEmpty(request.StoreId) || string.IsNullOrEmpty(request.OrderNumber))
        {
            return BadRequest("Required parameters are missing.");
        }

        var bearerToken = passwordDictionary.GetValueOrDefault("SkuLabsAuth");
        if (string.IsNullOrEmpty(bearerToken))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Bearer token is missing.");
        }

        try
        {
            // Create the request object  
            JsonObject requestObject = new()
            {
                ["store_id"] = request.StoreId,
                ["order_number"] = request.OrderNumber,
                ["address"] = request.Address,
                ["carrier"] = request.Carrier,
                ["tracking_number"] = request.TrackingNumber,
                ["service"] = request.Service,
                ["order_items"] = new JsonArray(request.OrderItems.Select(item => new JsonObject
                {
                    ["quantity"] = item.Quantity,
                    ["location_id"] = item.LocationId,
                    ["item_id"] = item.ItemId,
                    ["line_id"] = item.LineId,
                    // Unsure if this is necessary. Took a bit of finagling to get this working.
                    ["serial_numbers"] = item.SerialNumbers != null ? new JsonArray(item.SerialNumbers.Select(sn => JsonValue.Create(sn)).ToArray()) : new JsonArray(),
                    ["_id"] = item.Id
                }).ToArray()),
                ["warehouse_id"] = _configuration["ERPSkulabsWarehouseId"],
                ["dropship"] = "true",
                ["no_refresh"] = request.NoRefresh,
                ["notes"] = request.Notes,
                ["force_deduction"] = "true",
                ["cost"] = request.Cost,
                ["options"] = request.Options,
                ["booking"] = request.Booking,
                ["seal"] = request.Seal,
                ["origin_address"] = request.OriginAddress,
                ["bol_pro_number"] = request.BolProNumber,
                ["total_bundles"] = request.TotalBundles,
                ["parcel"] = request.Parcel
            };

            var jsonRequestBody = requestObject.ToString();

            var httpResponse = await HttpSendMessage(
                new Uri("https://api.skulabs.com/order/add_manual_shipment"),
                HttpMethod.Post,
                new AuthenticationHeaderValue("Bearer", bearerToken),
                new Dictionary<string, string>(),
                new StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
            );
            var responseBody = await httpResponse.Content.ReadAsStringAsync();
            if (!httpResponse.IsSuccessStatusCode)
            {
                return BadRequest();
            }

            var responseDto = JsonSerializer.Deserialize<SkulabsResponseDTO>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Ok(responseDto);
        }
        catch (Exception)
        {
            //_logger.LogError(ex, "Error occurred while adding manual shipment.");
            throw;
        }
    }

    //public async Task<IActionResult> RequestDHLAccessToken()
    //{
    //    var collection = new List<KeyValuePair<string, string>>
    //    {
    //        new("grant_type", "client_credentials"),
    //        new("client_id", passwordDictionary.GetValueOrDefault("DHLClientId")),
    //        new("client_secret",  passwordDictionary.GetValueOrDefault("DHLClientSecret"))
    //    };

    //    var httpResponse = await HttpSendMessage(
    //        new Uri("https://api.dhlecs.com/auth/v4/accesstoken"),
    //        HttpMethod.Post,
    //        null,
    //        null,
    //        null,
    //        new FormUrlEncodedContent(collection)
    //        );
    //    var responseBody = await httpResponse.Content.ReadAsStringAsync();
    //    var deserializedResponse = JsonConvert.DeserializeObject(responseBody);
    //    if (!httpResponse.IsSuccessStatusCode)
    //    {
    //        return BadRequest();
    //    }

    //    var response = JObject.FromObject(deserializedResponse);
    //    //If successful, the next new Webhook service call referencing this configuration will be contained within the password dictionary.
    //    _configuration["DHLAuth:access_token"] = response.GetValue("access_token").ToString();
    //    return Ok();
    //}

    //public async Task<IActionResult> GenerateDHLLabel(Order order)
    //{
    //    var collection = new List<KeyValuePair<string, string>>
    //    {
    //        new("grant_type", "client_credentials"),
    //        new("client_id", passwordDictionary.GetValueOrDefault("DHLClientId")),
    //        new("client_secret",  passwordDictionary.GetValueOrDefault("DHLClientSecret"))
    //    };

    //    var requestShipmentBody = new JsonObject()
    //    {
    //        ["pickup"] = passwordDictionary.GetValueOrDefault("DHLClientPickupId"),
    //        ["distributionCenter"] = passwordDictionary.GetValueOrDefault("DHLDistributionCenter"),
    //        ["consigneeAddress"] = new JsonObject
    //        {
    //            ["name"] = order.shipTo.name,
    //            ["companyName"] = order.shipTo.company,
    //            ["phone"] = order.shipTo.phone,
    //            ["address1"] = order.shipTo.street1,
    //            ["address2"] = order.shipTo.street2,
    //            ["city"] = order.shipTo.city,
    //            ["state"] = order.shipTo.state,
    //            ["postalCode"] = order.shipTo.postalCode,
    //            ["countryCode"] = order.shipTo.country,
    //        },
    //        ["returnAddress"] = new JsonObject
    //        {
    //            ["name"] = order.shipFrom.name,
    //            ["companyName"] = order.shipFrom.company,
    //            ["phone"] = order.shipFrom.phone,
    //            ["address1"] = order.shipFrom.street1,
    //            ["address2"] = order.shipFrom.street2,
    //            ["city"] = order.shipFrom.city,
    //            ["state"] = order.shipFrom.state,
    //            ["postalCode"] = order.shipFrom.postalCode,
    //            ["countryCode"] = order.shipFrom.country,
    //        },
    //        ["packageDetail"] =
    //        new JsonObject
    //        {
    //            ["packageId"] = order.orderId,
    //            ["packageDescription"] = order.orderNumber,
    //            ["weight"] = new JsonObject
    //            {
    //                ["unitOfMeasure"] = order.weight.units.ToString() switch { "ounces" => "OZ", "pounds" => "LB", _ => order.weight.units.ToString() },
    //                ["value"] = order.weight.value.ToString(),
    //            },
    //            ["service"] = order.serviceCode,
    //        },
    //    };

    //    var httpResponse = await HttpSendMessage(
    //        new Uri("https://api.dhlecs.com/shipping/v4/label?format=PDF"),
    //        HttpMethod.Post,
    //        null,
    //        null,
    //        new StringContent(requestShipmentBody.ToString(), Encoding.Default, "application/json"),
    //        new FormUrlEncodedContent(collection)
    //        );
    //    var responseBody = await httpResponse.Content.ReadAsStringAsync();
    //    var deserializedResponse = JsonConvert.DeserializeObject(responseBody);
    //    if (!httpResponse.IsSuccessStatusCode)
    //    {
    //        return BadRequest();
    //    }

    //    var response = JObject.FromObject(deserializedResponse);
    //    return Ok(response);
    //}
    //public async Task<IActionResult> CreateDHLManifest()
    //{
    //    var collection = new List<KeyValuePair<string, string>>
    //    {
    //        new("grant_type", "client_credentials"),
    //        new("client_id", passwordDictionary.GetValueOrDefault("DHLClientId")),
    //        new("client_secret",  passwordDictionary.GetValueOrDefault("DHLClientSecret"))
    //    };
    //    var httpResponse = await HttpSendMessage(
    //        new Uri("https://api.dhlecs.com/shipping/v4/manifest"),
    //        HttpMethod.Post,
    //        null,
    //         new Dictionary<string, string>() { { "Authorization", $"Bearer {_configuration["DHLAuth:access_token"]}" },
    //             {"Content-Type","application/json" } },
    //        null,
    //        new FormUrlEncodedContent(collection)
    //        );
    //    var responseBody = await httpResponse.Content.ReadAsStringAsync();
    //    var deserializedResponse = JsonConvert.DeserializeObject(responseBody);
    //    if (!httpResponse.IsSuccessStatusCode)
    //    {
    //        return BadRequest();
    //    }

    //    var response = JObject.FromObject(deserializedResponse);
    //    return Ok(response);
    //}
    //public async Task<IActionResult> FetchDHLManifest(string manifestRequestId)
    //{
    //    var collection = new List<KeyValuePair<string, string>>
    //    {
    //        new("grant_type", "client_credentials"),
    //        new("client_id", passwordDictionary.GetValueOrDefault("DHLClientId")),
    //        new("client_secret",  passwordDictionary.GetValueOrDefault("DHLClientSecret"))
    //    };

    //    var httpResponse = await HttpSendMessage(
    //        new Uri($"https://api.dhlecs.com/shipping/v4/manifest/{passwordDictionary.GetValueOrDefault("DHLClientPickupId")}/{manifestRequestId}"),
    //        HttpMethod.Get,
    //        null,
    //        null,
    //        null,
    //        new FormUrlEncodedContent(collection)
    //    );
    //    var responseBody = await httpResponse.Content.ReadAsStringAsync();

    //    var deserializedResponse = JsonConvert.DeserializeObject(responseBody);
    //    if (!httpResponse.IsSuccessStatusCode)
    //    {
    //        return BadRequest();
    //    }

    //    var response = JObject.FromObject(deserializedResponse);
    //    return Ok(response);
    //}

    public async Task<T> GenerateManifest<T>(string warehouseId, string carrierId, DateTime shipDate)
    {
        try
        {
            var requestBody = new JsonObject
            {
                ["carrier_id"] = carrierId,
                ["warehouse_id"] = warehouseId,
                ["ship_date"] = shipDate
            };

            var httpResponse = await HttpSendMessage(
                new Uri("https://api.shipengine.com/v1/manifests"),
                HttpMethod.Post,
                null,
                new Dictionary<string, string>() { { "API-Key", passwordDictionary.GetValueOrDefault("ShipEngineAuth") } },
                new StringContent(requestBody.ToString(), Encoding.Default, "application/json")
            );
            var responseBody = await httpResponse.Content.ReadAsStringAsync();
            if (!httpResponse.IsSuccessStatusCode)
            {
                return default;
            }

            var jsonResponse = JsonDocument.Parse(responseBody);

            var root = jsonResponse.RootElement;

            if (root.TryGetProperty("manifests", out var carriersElement))
                return JsonSerializer.Deserialize<T>(
                    carriersElement.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

            return default;

        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task DownloadManifest(string fileUrl) => HttpSendMessage(
        new Uri(fileUrl),
        HttpMethod.Get,
        null,
        new Dictionary<string, string>()
        {
            { "API-Key", passwordDictionary.GetValueOrDefault("ShipEngineAuth") },
            { "Accept-Type", "application/pdf" }
        }
    );
}
