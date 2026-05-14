using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identity.Contracts;
using Microsoft.Extensions.Primitives;

namespace Web.Api.Middleware;

public class GatewayAuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var claims = new List<Claim>();

        if (context.Request.Headers.TryGetValue(IdentityHeaders.UserId, out var extractedUserId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, extractedUserId.ToString()));
        }

        if (context.Request.Headers.TryGetValue(IdentityHeaders.SessionId, out var extractedSessionId))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sid, extractedSessionId.ToString()));
        }

        if (context.Request.Headers.TryGetValue(IdentityHeaders.OnboardingState, out var onboardingState))
        {
            claims.Add(new Claim(IdentityHeaders.OnboardingState, onboardingState.ToString()));
        }

        if (claims.Count > 0)
        {
            var identity = new ClaimsIdentity(claims, "Gateway");
            context.User = new ClaimsPrincipal(identity);
        }

        await next(context);
    }
}