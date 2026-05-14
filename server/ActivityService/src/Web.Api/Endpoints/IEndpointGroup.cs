namespace Web.Api.Endpoints;

public interface IEndpointGroup
{
    string RoutePrefix { get; }
    string Tag { get; }
}