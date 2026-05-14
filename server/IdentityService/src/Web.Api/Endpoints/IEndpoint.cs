namespace Web.Api.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(RouteGroupBuilder group);
}

public interface IEndpoint<TModule> : IEndpoint where TModule : IEndpointGroup;
