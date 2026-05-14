using Source.Shared.Persistence;
using UltraLiteDB;

namespace Source.Features.Auth.Models
{
    internal class TokenModel : IPersistable<int>
    {
        [BsonId] public int Id { get; set; }
        public string EncryptedAccess { get; set; }
        public string EncryptedRefresh { get; set; }
    }
}