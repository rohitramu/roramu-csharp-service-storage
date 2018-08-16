namespace RoRamu.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public abstract class EntityStore<TEntity>
    {
        public abstract TEntity Add(TEntity entity);

        public abstract TEntity Get(string key);

        public abstract IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null);

        public abstract TEntity Update(Delta<TEntity> patchSet);

        public abstract TEntity Delete(string key);
    }
}
