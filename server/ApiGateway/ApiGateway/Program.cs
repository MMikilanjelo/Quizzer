using System.Security.Claims;
using System.Text;
using Identity.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy("RequireAuthenticatedUser", policy => policy.RequireAuthenticatedUser());


builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(transformContext =>
        {
            transformContext.ProxyRequest.Headers.Remove(IdentityHeaders.UserId);
            transformContext.ProxyRequest.Headers.Remove(IdentityHeaders.SessionId);
            transformContext.ProxyRequest.Headers.Remove(IdentityHeaders.UserRoles);
            transformContext.ProxyRequest.Headers.Remove(IdentityHeaders.OnboardingState);
            transformContext.ProxyRequest.Headers.Remove("Authorization");

            var user = transformContext.HttpContext.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                return ValueTask.CompletedTask;
            }

            var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                transformContext.ProxyRequest.Headers.Add(IdentityHeaders.UserId, userId);
            }

            var sessionId = user.FindFirst(JwtRegisteredClaimNames.Sid)?.Value;
            if (!string.IsNullOrEmpty(sessionId))
            {
                transformContext.ProxyRequest.Headers.Add(IdentityHeaders.SessionId, sessionId);
            }

            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (roles.Count > 0)
            {
                transformContext.ProxyRequest.Headers.Add(IdentityHeaders.UserRoles, roles);
            }

            var onboardingState = user.FindFirst(IdentityClaimNames.OnboardingState)?.Value;
            if (!string.IsNullOrEmpty(onboardingState))
            {
                transformContext.ProxyRequest.Headers.Add(IdentityHeaders.OnboardingState, onboardingState);
            }

            return ValueTask.CompletedTask;
        });
    });


var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapReverseProxy();

app.Run();