using ERPWebApp.Providers.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERPWebApp.Providers;
public class CachedTokenService(
    IMemoryCache memoryCache,
    IExternalTokenService externalTokenService) : ICachedTokenService
{
    private const string CacheKey = nameof(CachedTokenService);
    private const string PaymentAuthCacheKey = $"{CacheKey}_Payment";
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly IExternalTokenService _externalTokenService = externalTokenService;

    public async ValueTask<Token> GetTokenAsync(ResilienceContext context)
    {
        if (!_memoryCache.TryGetValue(CacheKey, out Token cacheValue))
        {
            cacheValue = await RefreshTokenAsync(context);
        }
        return cacheValue!;
    }

    public async Task<Token> RefreshTokenAsync(ResilienceContext context)
    {
        var token = await _externalTokenService.GetTokenAsync();

        if (token != Token.Empty)
        {
            var expiresIn = token.ExpiresIn > 0 ? token.ExpiresIn - 10 : token.ExpiresIn;

            _memoryCache.Set(CacheKey, token, new MemoryCacheEntryOptions()
                 .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                 .SetAbsoluteExpiration(TimeSpan.FromSeconds(expiresIn)));
        }

        context.Properties.Set(new ResiliencePropertyKey<Token>("AccessToken"), token);

        return token;
    }

    public async ValueTask<Token> GetPaymentAuthTokenAsync(ResilienceContext context)
    {
        if (!_memoryCache.TryGetValue(PaymentAuthCacheKey, out Token cacheValue))
        {
            cacheValue = await RefreshPaymentAuthTokenAsync(context);
        }
        return cacheValue!;
    }

    public async Task<Token> RefreshPaymentAuthTokenAsync(ResilienceContext context)
    {
        var token = await _externalTokenService.GetAuthAsync();

        if (token != Token.Empty)
        {
            var expiresIn = token.ExpiresIn > 0 ? token.ExpiresIn - 10 : token.ExpiresIn;

            _memoryCache.Set(PaymentAuthCacheKey, token, new MemoryCacheEntryOptions()
                 .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                 .SetAbsoluteExpiration(TimeSpan.FromSeconds(expiresIn)));
        }

        context.Properties.Set(new ResiliencePropertyKey<Token>("PaymentAuthToken"), token);

        return token;
    }
}
public class ExternalTokenService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IExternalTokenService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Token> GetTokenAsync() => await GetAccessTokenAsync();

    private async Task<Token> GetAccessTokenAsync()
    {
        StringContent stringContent = new(
            JsonSerializer.Serialize(new
            {
                grant_type = "client_credentials",
                client_id = _configuration["Usps:ClientId"],
                client_secret = _configuration["Usps:ClientSecret"],
                scope = _configuration["Usps:scope"]
            }), Encoding.Default, "application/json"
        );
        //NOTE:: DO NOT USE CLIENT FACTORY OR YOU WILL GET OVERFLOW ERRORS
        using HttpClient client = new();
        client.BaseAddress = new Uri(_configuration["Usps:ApiUrl"]!);
        using HttpResponseMessage response = await client.PostAsync("oauth2/v3/token", stringContent);
        response.EnsureSuccessStatusCode();
        var tokenData = await response.Content.ReadFromJsonAsync<Token>();
        return tokenData;
    }

    public async Task<Token> GetAuthAsync() => await GetPaymentAuthAsync();

    private async Task<Token> GetPaymentAuthAsync()
    {
        var roles = new List<USPSRole>
        {
            new()
            {
                RoleName = "PAYER",
                CRID = _configuration["Usps:CRID"],
                MID = _configuration["Usps:MID"],
                ManifestMID = _configuration["Usps:manifestMID"],
                AccountType = _configuration["Usps:accountType"],
                AccountNumber = _configuration["Usps:accountNumber"]
            },
            new()
            {
                RoleName = "LABEL_OWNER",
                CRID = _configuration["Usps:CRID"],
                MID = _configuration["Usps:MID"],
                ManifestMID = _configuration["Usps:manifestMID"],
                AccountType = _configuration["Usps:accountType"],
                AccountNumber = _configuration["Usps:accountNumber"]
            }
        };

        StringContent stringContent = new(
            JsonSerializer.Serialize(new { roles }),
            Encoding.Default,
            "application/json"
        );

        using HttpClient client = _httpClientFactory.CreateClient("USPS");
        var responseMessage = await client.PostAsync("payments/v3/payment-authorization", stringContent);
        responseMessage.EnsureSuccessStatusCode();
        var responseBody = await responseMessage.Content.ReadAsStringAsync();
        var paymentAuthorizationResponse = JsonSerializer.Deserialize<PaymentAuthorizationResponse>(responseBody);
        
        return new Token
        {
            PaymentAuthorizationToken = paymentAuthorizationResponse.PaymentAuthorizationToken,
            ExpiresIn = 3600 // Default expiration time for payment tokens
        };
    }
}
public class PaymentAuthorizationResponse
{
    [JsonPropertyName("paymentAuthorizationToken")]
    public string PaymentAuthorizationToken { get; set; } = default!;

    [JsonPropertyName("roles")]
    public List<USPSRole> Roles { get; set; } = new();
}

public class USPSRole
{
    [JsonPropertyName("roleName")]
    public string RoleName { get; set; } = default!;

    [JsonPropertyName("CRID")]
    public string CRID { get; set; } = default!;

    [JsonPropertyName("MID")]
    public string? MID { get; set; }

    [JsonPropertyName("manifestMID")]
    public string? ManifestMID { get; set; }

    [JsonPropertyName("accountType")]
    public string? AccountType { get; set; }

    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }
}

public class TokenRetrievalHandler(ICachedTokenService cachedTokenService) : DelegatingHandler
{
    private readonly ICachedTokenService _cachedTokenService = cachedTokenService;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = ResilienceContextPool.Shared.Get(cancellationToken);
        try
        {
            // Get regular access token
            context.Properties.TryGetValue(new ResiliencePropertyKey<Token>("AccessToken"), out var token);
            token ??= await _cachedTokenService.GetTokenAsync(context);

            if (token == Token.Empty || string.IsNullOrEmpty(token.AccessToken))
            {
                throw new InvalidOperationException("Invalid or missing access token");
            }

            // Add regular authorization header
            request.Headers.Authorization = new AuthenticationHeaderValue(token.Scheme, token.AccessToken);

            // Get payment authorization token if needed
            if (request.Headers.Contains("X-Require-Payment-Auth"))
            {
                var paymentToken = await _cachedTokenService.GetPaymentAuthTokenAsync(context);
                if (paymentToken != Token.Empty && !string.IsNullOrEmpty(paymentToken.PaymentAuthorizationToken))
                {
                    request.Headers.Add("X-Payment-Authorization-Token", paymentToken.PaymentAuthorizationToken);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }
}
public record Token
{
    public static Token Empty => new();

    [JsonPropertyName("token_type")]
    public string Scheme { get; set; } = default!;

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = default!;

    [JsonPropertyName("expires_in")]
    public double ExpiresIn { get; set; } = default!;

    [JsonPropertyName("paymentAuthorizationToken")]
    public string PaymentAuthorizationToken { get; set; } = default!;
}