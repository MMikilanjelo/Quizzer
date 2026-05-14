using UltraLiteDB;

namespace Source.Shared.Persistence.DataBase.Converters
{
    public class StringIdConverter : IIdConverter<string>
    {
        public BsonValue ToBsonId(string id) => new(id);
        public string FromBsonId(BsonValue bsonValue) => bsonValue.AsString;
    }
}