using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Web.Api.Endpoints;

namespace Web.Api.Extensions;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }).ToList();

        var modules = types.Where(t => t.IsAssignableTo(typeof(IEndpointGroup)));
        
        foreach (var module in modules)
        {
            services.AddSingleton(typeof(IEndpointGroup), module);
        }

        var endpoints = types.Where(t => t.IsAssignableTo(typeof(IEndpoint)));
        
        foreach (var endpoint in endpoints)
        {
            var interfaceType = endpoint.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEndpoint<>));

            services.AddTransient(interfaceType, endpoint);
        }

        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app)
    {
        var modules = app.Services.GetServices<IEndpointGroup>();

        foreach (var module in modules)
        {
            var group = app.MapGroup(module.RoutePrefix).WithTags(module.Tag);

            var endpointType = typeof(IEndpoint<>).MakeGenericType(module.GetType());

            var endpoints = app.Services.GetServices(endpointType).Cast<IEndpoint>();

            foreach (var endpoint in endpoints)
            {
                endpoint.MapEndpoint(group);
            }
        }

        return app;
    }
}