using System.Collections.Generic;

namespace Source.Shared.Persistence
{
    public interface IRepository<TEntity, TId> where TEntity : class, IPersistable<TId>
    {
        Result<TId> Insert(TEntity entity);
        Result<TEntity> GetById(TId id);
        Result<IEnumerable<TEntity>> GetAll();
        Result<bool> Update(TEntity entity);
        Result<bool> Upsert(TEntity entity);
        Result<bool> Delete(TId id);
    }
}