using UltraLiteDB;

namespace Source.Shared.Persistence.DataBase.Converters
{
    public interface IIdConverter<TId>
    {
        BsonValue ToBsonId(TId id);
        TId FromBsonId(BsonValue bsonValue);
    }
}