namespace Application;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
