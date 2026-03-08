using System.Security.Claims;

namespace Web.Api.Middleware;

public class GatewayAuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("User-Id", out var extractedId))
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, extractedId.ToString()) };
            
            var identity = new ClaimsIdentity(claims, "Gateway");

            context.User = new ClaimsPrincipal(identity);
        }

        await next(context);
    }
}