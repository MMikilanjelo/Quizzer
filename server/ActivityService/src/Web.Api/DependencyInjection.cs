using ActivityService.QuizActivity.Worker;
using Web.Api.Infrastructure;

namespace Web.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options => options.CustomSchemaIds(type => type.DeclaringType is not null ? $"{type.DeclaringType.Name}{type.Name}" : type.Name));

        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddProblemDetails();

        return services;
    }
}
