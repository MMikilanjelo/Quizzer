namespace Application.Authentication;

public interface IUserContext
{
    string UserId { get; }
    string SessionId { get; }
}