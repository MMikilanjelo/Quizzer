using Domain.Sessions;
using Domain.Users;

namespace Application.Authentication;

public interface ITokenProvider
{
    string GenerateAccessToken(User user, string sessionId);
    string GenerateRefreshToken();
    string HashRefreshToken(string token);
}