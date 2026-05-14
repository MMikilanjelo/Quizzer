using Domain;

namespace Application.Authentication;

public interface IUserContext
{
    string UserId { get; }
}