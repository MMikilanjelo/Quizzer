using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Web.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddOpenApiWithAuth(this IServiceCollection services)
    {
        services.AddOpenApi(options => options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                const string schemeName = JwtBearerDefaults.AuthenticationScheme;

                var jwtScheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your Firebase/JWT token in this field"
                };

                document.Components ??= new OpenApiComponents();

                document.Components?.SecuritySchemes?.Add(schemeName, jwtScheme);

                document.Security ??= [];

                document.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(schemeName, document)] = []
                });

                return Task.CompletedTask;
            }));

        return services;
    }
}
