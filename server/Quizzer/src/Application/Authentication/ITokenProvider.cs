using Domain.Users;

namespace Application.Authentication;

public interface ITokenProvider
{
    string Create(User user);
}
