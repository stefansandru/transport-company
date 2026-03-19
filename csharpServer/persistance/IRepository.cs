using System.Collections.Generic;

namespace persistance
{
    public interface IRepository<TId, TEntity> where TEntity : class
    {
        TEntity? FindById(TId id);

        IEnumerable<TEntity> FindAll();

        TEntity? Save(TEntity entity);

        TEntity? Update(TEntity entity);

        TEntity? Delete(TId id);
    }
}