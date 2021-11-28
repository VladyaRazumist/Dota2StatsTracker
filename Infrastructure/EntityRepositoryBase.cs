using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure
{
    public abstract class EntityRepositoryBase<TEntity> where TEntity : EntityBase, new()
    {
        private DbContext DbContext { get; }
        protected DbSet<TEntity> DbSet { get; }

        protected Expression<Func<TEntity, bool>> GetIdPredicate(int id)
        {
            return e => e.Id == id;
        }

        protected EntityRepositoryBase(DbContext dbContext)
        {
            DbContext = dbContext;
            DbSet = dbContext.Set<TEntity>();
        }

        public virtual void Create(TEntity entity)
        {
            if (entity.CreatedAtUtc.IsEmpty())
                entity.CreatedAtUtc = DateTimeOffset.UtcNow;

            DbSet.Add(entity);
        }

        public virtual void Create(IEnumerable<TEntity> entities)
        {
            entities.ToList().ForEach(Create);
        }

        public void Update(TEntity entity)
        {
            static bool IsOwnedEntityChanged(EntityEntry entry) =>
                entry.State == EntityState.Modified || entry.State == EntityState.Added;

            static bool AnyOwnedEntityPropsChanged(IEnumerable<ReferenceEntry> references) => references.Any(r =>
                r.TargetEntry != null &&
                r.TargetEntry.Metadata.IsOwned() &&
                (IsOwnedEntityChanged(r.TargetEntry) || AnyOwnedEntityPropsChanged(r.TargetEntry.References)));

            static bool IsEntityStateModified(EntityEntry entry) =>
                entry.State == EntityState.Modified || AnyOwnedEntityPropsChanged(entry.References);

            var entityEntry = DbContext.Entry(entity);
            if (IsEntityStateModified(entityEntry) && !entityEntry.Property(nameof(EntityBase.UpdatedAtUtc)).IsModified)
                entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            else if (entityEntry.State == EntityState.Detached)
                throw new InvalidOperationException("Can't save detached entity.");
        }

        public virtual void Update(IEnumerable<TEntity> entities)
        {
            entities.ToList().ForEach(Update);
        }

        public void Delete(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public virtual void Delete(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            entities.ToList().ForEach(Delete);
        }

        public void Delete(int id)
        {
            var found = GetById(id);
            if (found == null)
                throw new ArgumentException($"Entity '{id}' doesnt't exist.");

            Delete(found);
        }

        public void Delete(IEnumerable<int> ids)
        {
            Delete(e => ids.Contains(e.Id));
        }

        public void Delete(Expression<Func<TEntity, bool>> where)
        {
            foreach (var entity in GetMany(where))
                Delete(entity);
        }

        public Task DeleteAsync(IEnumerable<int> ids)
        {
            return DeleteAsync(e => ids.Contains(e.Id));
        }

        public async Task DeleteAsync(Expression<Func<TEntity, bool>> where)
        {
            foreach (var entity in await GetMany(where).ToArrayAsync())
                Delete(entity);
        }

        public void DeleteAll()
        {
            DbSet.RemoveRange(DbSet);
        }

        public virtual TEntity GetById(int id, bool throwExceptionIfNotFound = true)
        {
            var result = GetMany(GetIdPredicate(id)).Take(1).ToArray();
            if (!result.Any() && throwExceptionIfNotFound)
                throw new InvalidOperationException($"Failed to find entity of type '{typeof(TEntity)}' by id '{id}'.");

            return result.First();
        }

        public virtual TResult GetById<TResult>(int id, Expression<Func<TEntity, TResult>> selector, bool throwExceptionIfNotFound = true)
        {
            var result = GetMany(GetIdPredicate(id), selector).Take(1).ToArray();
            if (!result.Any() && throwExceptionIfNotFound)
                throw new InvalidOperationException($"Failed to find entity of type '{typeof(TEntity)}' by id '{id}'.");

            return result.First();
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            return DbSet;
        }

        public virtual IQueryable<TResult> GetAll<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return GetAll().AsNoTracking().Select(selector);
        }

        public virtual IQueryable<TEntity> GetMany(Expression<Func<TEntity, bool>> where)
        {
            return GetAll().Where(where);
        }

        public virtual IQueryable<TResult> GetMany<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return GetAll().AsNoTracking().Where(where).Select(selector);
        }

        public virtual TEntity FirstOrDefault()
        {
            return GetAll().FirstOrDefault();
        }

        public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> where)
        {
            return GetAll().FirstOrDefault(where);
        }

        public virtual TEntity First()
        {
            return GetAll().First();
        }

        public TEntity First(Expression<Func<TEntity, bool>> where)
        {
            return GetAll().First(where);
        }

        public TEntity SingleOrDefault()
        {
            return GetAll().SingleOrDefault();
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> where)
        {
            return GetAll().SingleOrDefault(where);
        }

        public TEntity Single()
        {
            return GetAll().Single();
        }

        public TEntity Single(Expression<Func<TEntity, bool>> where)
        {
            return GetAll().Single(where);
        }

        public TResult FirstOrDefault<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return GetAll().Select(selector).FirstOrDefault();
        }

        public virtual TResult FirstOrDefault<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return GetMany(where).Select(selector).FirstOrDefault();
        }

        public TResult First<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return GetAll().Select(selector).First();
        }

        public virtual TResult First<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return GetMany(where).Select(selector).First();
        }

        public TResult SingleOrDefault<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return GetAll().Select(selector).SingleOrDefault();
        }

        public TResult SingleOrDefault<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return GetMany(where).Select(selector).SingleOrDefault();
        }

        public TResult Single<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return GetAll().Select(selector).Single();
        }

        public TResult Single<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return GetMany(where).Select(selector).Single();
        }

        public virtual bool Any(Expression<Func<TEntity, bool>> where)
        {
            return GetAll().Any(where);
        }

        public virtual bool Any()
        {
            return GetAll().Any();
        }

        public virtual bool Exist(int id)
        {
            return Any(GetIdPredicate(id));
        }

        public virtual int Count(Expression<Func<TEntity, bool>> where = null)
        {
            return where == null ? GetAll().Count() : GetAll().Count(where);
        }

        public virtual IQueryable<TEntity> AsNoTracking()
        {
            return DbSet.AsNoTracking();
        }

        public async Task<TEntity> GetByIdAsync(int id, bool throwExceptionIfNotFound = true)
        {
            // DbSet.FindAsync can't be used here because it always ignores global query filters by design
            // https://github.com/aspnet/EntityFrameworkCore/issues/9405

            var result = await GetMany(GetIdPredicate(id)).Take(1).ToArrayAsync();
            if (!result.Any() && throwExceptionIfNotFound)
                ThrowEntityNotFoundException(id);

            return result.FirstOrDefault();
        }

        public async Task<TResult> GetByIdAsync<TResult>(int id, Expression<Func<TEntity, TResult>> selector, bool throwExceptionIfNotFound = true)
        {
            var result = await GetMany(GetIdPredicate(id), selector).Take(1).ToArrayAsync();
            if (!result.Any() && throwExceptionIfNotFound)
                ThrowEntityNotFoundException(id);

            return result.FirstOrDefault();
        }

        public Task<TEntity> FirstOrDefaultAsync()
        {
            return GetAll().FirstOrDefaultAsync();
        }

        public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> where)
        {
            return GetAll().FirstOrDefaultAsync(where);
        }

        public Task<TEntity> FirstAsync()
        {
            return GetAll().FirstAsync();
        }

        public Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> where)
        {
            return GetAll().FirstAsync(where);
        }

        public Task<TEntity> SingleOrDefaultAsync()
        {
            return GetAll().SingleOrDefaultAsync();
        }

        public Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> where)
        {
            return GetAll().SingleOrDefaultAsync(where);
        }

        public Task<TEntity> SingleAsync()
        {
            return GetAll().SingleAsync();
        }

        public Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> where)
        {
            return GetAll().SingleAsync(where);
        }

        public Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return GetAll(selector).FirstOrDefaultAsync();
        }

        public Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return GetMany(where, selector).FirstOrDefaultAsync();
        }

        public Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return GetAll(selector).FirstAsync();
        }

        public Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return GetMany(where, selector).FirstAsync();
        }

        public Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return GetAll(selector).SingleOrDefaultAsync();
        }

        public Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return GetMany(where, selector).SingleOrDefaultAsync();
        }

        public Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            return GetAll(selector).SingleAsync();
        }

        public Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector)
        {
            return GetMany(where, selector).SingleAsync();
        }

        public Task<bool> AnyAsync()
        {
            return GetAll().AnyAsync();
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAll().AnyAsync(predicate);
        }

        public Task<bool> AllAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return GetAll().AllAsync(predicate);
        }

        public virtual Task<bool> ExistAsync(int id)
        {
            return AnyAsync(GetIdPredicate(id));
        }

        public Task<int> CountAsync()
        {
            return GetAll().CountAsync();
        }

        public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return GetMany(predicate).CountAsync();
        }

        private void ThrowEntityNotFoundException(int entityId)
        {
            throw new InvalidOperationException($"Failed to find entity of type '{typeof(TEntity)}' by id '{entityId}'.");
        }

    }
}
