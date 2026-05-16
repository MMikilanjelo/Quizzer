using Source.Shared;

namespace Source.Features.Auth.Models
{
    internal interface ITokensStore
    {
        Result SaveTokens(string accessToken, string refreshToken);
        Result<string> GetAccessToken();
        Result<string> GetRefreshToken();
        Result<bool> HasTokens();
        Result ClearTokens();
    }
}