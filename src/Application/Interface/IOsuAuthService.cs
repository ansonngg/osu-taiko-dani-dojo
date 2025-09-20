using Application.Model;

namespace Application.Interface;

public interface IOsuAuthService
{
    string GetAuthorizeUrl();
    Task<UserToken> ExchangeTokenAsync(string code);
    Task<UserToken?> RefreshTokenAsync(string refreshToken);
    Task<int> GetUserIdAsync(string accessToken);
}
