using Polly;

namespace ERPWebApp.Providers.Interfaces;

public interface ICachedTokenService
{
    ValueTask<Token> GetTokenAsync(ResilienceContext context);
    Task<Token> RefreshTokenAsync(ResilienceContext context);
    ValueTask<Token> GetPaymentAuthTokenAsync(ResilienceContext context);
    Task<Token> RefreshPaymentAuthTokenAsync(ResilienceContext context);
} 