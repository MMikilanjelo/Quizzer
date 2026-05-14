namespace Web.Api.Endpoints.Auth;

public class AuthEndpointGroup : IEndpointGroup
{
    public string RoutePrefix => "api/auth";
    public string Tag => "Authentication";
}