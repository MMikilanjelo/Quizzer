namespace Infrastructure.Configuration;

public class MongoDbOptions
{
    public const string SectionName = "MongoDb";

    public required string ConnectionString { get; init; } 

    public required string DatabaseName { get; init; } 
}