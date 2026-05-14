using ErrorOr;
using Identity.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Web.Api.Infrastructure;

public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult is { Forbidden: true, AuthorizationFailure: not null })
        {
            var failedRequirements = authorizeResult.AuthorizationFailure.FailedRequirements;

            var authorizationRequirements = failedRequirements as IAuthorizationRequirement[] ?? failedRequirements.ToArray();

            if (authorizationRequirements.OfType<ClaimsAuthorizationRequirement>().Any(req => req.ClaimType == IdentityHeaders.OnboardingState))
            {
                var error = Error.Forbidden("Onboarding.Required", "User has not completed the onboarding process.");

                await CustomResults.Problem(error).ExecuteAsync(context);

                return;
            }

            var fallbackError = Error.Forbidden("Access.Denied", "You do not have permission to perform this action.");

            await CustomResults.Problem(fallbackError).ExecuteAsync(context);

            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}