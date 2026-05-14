namespace Source.Shared.Persistence
{
    public interface IPersistable<TId>
    {
        TId Id { get; set; }
    }

    public interface IPersistable
    {
    }
}