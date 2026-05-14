using Application.Abstractions;

namespace Infrastructure.Authentication;
internal sealed class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);
    }

    public bool Verify(string plainPassword, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(plainPassword, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}
