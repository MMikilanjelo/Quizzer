using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : throw new ApplicationException("User id is unavailable");
    }

    public static Guid GetSessionId(this ClaimsPrincipal principal)
    {
        var sessionIdString = principal.FindFirst(JwtRegisteredClaimNames.Sid)?.Value;

        return Guid.TryParse(sessionIdString, out var sessionId) ? sessionId : throw new ApplicationException("Session ID claim is missing or invalid.");
    }

    public static IEnumerable<string> GetRoles(this ClaimsPrincipal? principal)
    {
        return principal?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? [];
    }
}