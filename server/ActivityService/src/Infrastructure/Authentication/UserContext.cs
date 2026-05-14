using System.Security.Authentication;
using Application.Authentication;
using Domain;
using Identity.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private HttpContext Context => httpContextAccessor.HttpContext ?? throw new UnauthorizedAccessException("HTTP Context is unavailable");

    public string UserId
    {
        get
        {
            if (Context.Request.Headers.TryGetValue(IdentityHeaders.UserId, out var userIdStr) &&
                !StringValues.IsNullOrEmpty(userIdStr))
            {
                return userIdStr.ToString();
            }

            throw new UnauthorizedAccessException("User ID header is missing or invalid.");
        }
    }
}