using MakanPergi.Data;
using MakanPergi.Entities.Base;
using MakanPergiWebApi.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace MakanPergiWebApi.Repository
{
    public partial class Repository<T> : IRepository<T> where T : AbstractBaseEntity
    {
        private DbSet<T> _entities;
        protected IDbContext DbContext { private set; get; }

        public Repository(IDbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        protected virtual DbSet<T> DbSet
        {
            get
            {
                if (_entities == null)
                {
                    _entities = DbContext.Instance.Set<T>();
                }
                return _entities;
            }
        }

        public void Add(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                DbSet.Add(entity);
            }
            catch (DbUpdateException)
            {
                throw;
            }
        }

        public Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                DbSet.AddAsync(entity, cancellationToken);
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return Task.CompletedTask;
        }

        public void AddRange(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                DbSet.AddRange(entities);
            }
            catch (DbUpdateException)
            {
                throw;
            }
        }

        public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                DbSet.AddRangeAsync(entities, cancellationToken);
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return Task.CompletedTask;
        }

        public IQueryable<T> FindBy(Expression<Func<T, bool>> expression)
        {
            return DbSet.Where(expression).AsNoTracking();
        }

        public IQueryable<T> Table => DbSet;

        public IQueryable<T> TableNoTracking => DbSet.AsNoTracking().AsQueryable();

        public async Task<IEnumerable<T>> FindAllAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<T> FindAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await DbSet.FindAsync(new object[] { id }, cancellationToken) ?? Activator.CreateInstance<T>();
        }

        public void Remove(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    throw new ArgumentNullException(nameof(id));

                var entity = DbSet.Find(id);

                if (entity == null)
                    throw new ArgumentException("No record exists in database.");

                DbSet.Remove(entity);
            }
            catch (DbUpdateException)
            {
                throw;
            }
        }

        /// <summary>
        /// Remove with composite key
        /// </summary>
        /// <param name="id"></param>
        /// <param name="compositeKey"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Remove(Guid id, Guid compositeKey)
        {
            try
            {
                if (id == Guid.Empty || compositeKey == Guid.Empty)
                    throw new ArgumentNullException(nameof(id) + " " + nameof(compositeKey));

                var entity = DbSet.Find(id, compositeKey);

                if (entity == null)
                    throw new ArgumentException("No record exists in database.");

                DbSet.Remove(entity);
            }
            catch (DbUpdateException)
            {
                throw;
            }
        }

        public void Remove(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                DbSet.Remove(entity);
            }
            catch (DbUpdateException)
            {
                throw;
            }
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                DbSet.RemoveRange(entities);
            }
            catch (DbUpdateException)
            {
                throw;
            }
        }

        public void Update(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                EntityEntry<T> entry = ((DbContext)DbContext).Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    DbSet.Attach(entity);
                }

                entry.State = EntityState.Modified;
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (DbUpdateException)
            {
                throw;
            }
        }

        /// <summary>
        /// Updates the entity and sets the original value of row version
        /// to the passed row version for concurrency checks.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rowVersion"></param>
        public void Update(T entity, byte[] rowVersion)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                EntityEntry<T> entry = ((DbContext)DbContext).Entry(entity);

                entry.Property("RowVersion").OriginalValue = rowVersion;

                if (entry.State == EntityState.Detached)
                {
                    DbSet.Attach(entity);
                }

                entry.State = EntityState.Modified;
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (DbUpdateException)
            {
                throw;
            }
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                DbSet.UpdateRange(entities);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            catch (DbUpdateException)
            {
                throw;
            }
        }
    }
}
