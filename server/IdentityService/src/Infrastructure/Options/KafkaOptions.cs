namespace Infrastructure.Options;

public record KafkaOptions
{
    public const string SectionName = "Kafka";
    public required string BootstrapServers { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}