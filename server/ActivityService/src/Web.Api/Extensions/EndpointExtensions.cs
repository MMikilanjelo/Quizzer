using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Web.Api.Endpoints;

namespace Web.Api.Extensions;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }).ToList();

        IEnumerable<Type> modules = types.Where(t => t.IsAssignableTo(typeof(IEndpointGroup)));
        
        foreach (Type? module in modules)
        {
            services.AddSingleton(typeof(IEndpointGroup), module);
        }

        IEnumerable<Type> endpoints = types.Where(t => t.IsAssignableTo(typeof(IEndpoint)));
        
        foreach (Type? endpoint in endpoints)
        {
            Type interfaceType = endpoint.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEndpoint<>));

            services.AddTransient(interfaceType, endpoint);
        }

        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app)
    {
        IEnumerable<IEndpointGroup> modules = app.Services.GetServices<IEndpointGroup>();

        foreach (IEndpointGroup module in modules)
        {
            RouteGroupBuilder group = app.MapGroup(module.RoutePrefix).WithTags(module.Tag);

            Type endpointType = typeof(IEndpoint<>).MakeGenericType(module.GetType());

            IEnumerable<IEndpoint> endpoints = app.Services.GetServices(endpointType).Cast<IEndpoint>();

            foreach (IEndpoint endpoint in endpoints)
            {
                endpoint.MapEndpoint(group);
            }
        }

        return app;
    }
}