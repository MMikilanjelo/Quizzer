using System.Reflection;
using Application;
using Infrastructure;
using Serilog;
using Web.Api;
using Web.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .ReadFrom
    .Configuration(context.Configuration)
    .Enrich.FromLogContext()
);

builder.Services.AddOpenApiWithAuth();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();


app.UseExceptionHandler((_) => { });

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();

    app.ApplyMigrations();
}

app.UseGatewayAuthentication();

app.UseAuthorization();

app.MapEndpoints();

await app.RunAsync();

namespace Web.Api
{
    public partial class Program;
}