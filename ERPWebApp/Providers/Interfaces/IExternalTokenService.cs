namespace ERPWebApp.Providers.Interfaces;

public interface IExternalTokenService
{
    Task<Token> GetTokenAsync();
    Task<Token> GetAuthAsync();
}