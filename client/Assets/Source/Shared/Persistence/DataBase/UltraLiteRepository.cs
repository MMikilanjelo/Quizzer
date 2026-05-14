using System;
using System.Collections.Generic;
using Source.Shared.Persistence.DataBase.Converters;
using UltraLiteDB;

namespace Source.Shared.Persistence.DataBase
{
    public class UltraLiteRepository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class, IPersistable<TId>
    {
        private readonly UltraLiteCollection<TEntity> _collection;

        private readonly IIdConverter<TId> _idConverter;

        public UltraLiteRepository(
            UltraLiteDocumentStore db,
            IIdConverter<TId> idConverter
        )
        {
            _idConverter = idConverter;

            _collection = db.GetCollection<TEntity, TId>(nameof(TEntity));
        }

        public Result<TId> Insert(TEntity entity)
        {
            try
            {
                var bsonId = _collection.Insert(entity);

                return Result<TId>.Success(_idConverter.FromBsonId(bsonId));
            }
            catch (Exception)
            {
                return Error.Local(ErrorCodes.StorageSaveFailed);
            }
        }

        public Result<TEntity> GetById(TId id)
        {
            try
            {
                var bsonId = _idConverter.ToBsonId(id);

                var entity = _collection.FindById(bsonId);

                if (entity == null)
                {
                    return Error.Local(ErrorCodes.StorageDataNotFound);
                }

                return entity;
            }
            catch (Exception)
            {
                return Error.Local(ErrorCodes.StorageAccessFailed);
            }
        }

        public Result<IEnumerable<TEntity>> GetAll()
        {
            try
            {
                var entities = new List<TEntity>(_collection.FindAll());

                return Result<IEnumerable<TEntity>>.Success(entities);
            }
            catch (Exception)
            {
                return Error.Local(ErrorCodes.StorageAccessFailed);
            }
        }

        public Result<bool> Update(TEntity entity)
        {
            try
            {
                var success = _collection.Update(entity);

                return Result<bool>.Success(success);
            }
            catch (Exception)
            {
                return Error.Local(ErrorCodes.StorageSaveFailed);
            }
        }

        public Result<bool> Upsert(TEntity entity)
        {
            try
            {
                var success = _collection.Upsert(entity);

                return Result<bool>.Success(success);
            }
            catch (Exception)
            {
                return Error.Local(ErrorCodes.StorageSaveFailed);
            }
        }

        public Result<bool> Delete(TId id)
        {
            try
            {
                var bsonId = _idConverter.ToBsonId(id);

                var success = _collection.Delete(bsonId);

                return Result<bool>.Success(success);
            }
            catch (Exception)
            {
                return Error.Local(ErrorCodes.StorageSaveFailed);
            }
        }
    }
}