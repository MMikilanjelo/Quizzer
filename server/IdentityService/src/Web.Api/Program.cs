using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application;
using FluentValidation;
using Identity.Contracts;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using Web.Api;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .ReadFrom
    .Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
});

ValidatorOptions.Global.PropertyNameResolver = (type, memberInfo, expression) =>
    memberInfo != null ? JsonNamingPolicy.SnakeCaseLower.ConvertName(memberInfo.Name) : null;

builder.Services.AddOpenApiWithAuth();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);


builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.RequireOnboardingComplete, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(IdentityHeaders.OnboardingState, OnboardingClaimValues.Completed);
    });

var app = builder.Build();

app.UseExceptionHandler((_) => { });

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();
}

app.UseGatewayAuthentication();

app.UseAuthorization();

app.MapEndpoints();

await app.RunAsync();

namespace Web.Api
{
    public partial class Program;
}