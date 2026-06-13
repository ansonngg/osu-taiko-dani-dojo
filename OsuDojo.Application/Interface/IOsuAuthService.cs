using OsuDojo.Application.Query;

namespace OsuDojo.Application.Interface;

public interface IOsuAuthService
{
    string GetAuthorizeUrl();
    Task<TokenQuery> ExchangeTokenAsync(string code);
    Task<TokenQuery?> RefreshTokenAsync(string refreshToken);
    Task<int> GetUserIdAsync(string accessToken);
}
