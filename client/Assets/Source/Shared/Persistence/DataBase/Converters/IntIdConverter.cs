using UltraLiteDB;

namespace Source.Shared.Persistence.DataBase.Converters
{
    public class IntIdConverter : IIdConverter<int>
    {
        public BsonValue ToBsonId(int id) => new(id);
        public int FromBsonId(BsonValue bsonValue) => bsonValue.AsInt32;
    }
}