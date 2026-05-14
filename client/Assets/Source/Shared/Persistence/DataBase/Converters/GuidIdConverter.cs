using System;
using UltraLiteDB;

namespace Source.Shared.Persistence.DataBase.Converters
{
    public class GuidIdConverter : IIdConverter<Guid>
    {
        public BsonValue ToBsonId(Guid id) => new(id);
        public Guid FromBsonId(BsonValue bsonValue) => bsonValue.AsGuid;
    }
}