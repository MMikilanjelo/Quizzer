namespace Domain.Users;

public class User
{
    public required Guid Id { get; init; }
    public required string Email { get; set; }
}