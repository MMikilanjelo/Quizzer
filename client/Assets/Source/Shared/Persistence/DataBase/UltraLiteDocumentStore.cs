using System.IO;
using UltraLiteDB;

namespace Source.Shared.Persistence.DataBase
{
    public class UltraLiteDocumentStore
    {
        private readonly UltraLiteDatabase _database;

        public UltraLiteDocumentStore()
        {
            var dbPath = Path.Combine(UnityEngine.Application.persistentDataPath, "MySecureData.db");

            var password = "test";

            var connectionString = $"Filename={dbPath};Password={password}";

            _database = new UltraLiteDatabase(connectionString);
        }

        public UltraLiteCollection<TEntity> GetCollection<TEntity, TId>(string collectionName) where TEntity : IPersistable<TId> =>
            _database.GetCollection<TEntity>(collectionName);
    }
}