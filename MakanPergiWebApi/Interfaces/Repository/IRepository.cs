using MakanPergi.Entities.Base;
using System.Linq.Expressions;

namespace MakanPergiWebApi.Interfaces.Repository
{
    public interface IRepository<T> where T : AbstractBaseEntity
    {
        Task<T> FindAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> FindAllAsync(CancellationToken cancellationToken = default);
        IQueryable<T> FindBy(Expression<Func<T, bool>> expression);
        IQueryable<T> Table { get; }
        IQueryable<T> TableNoTracking { get; }
        void Add(T entity);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        void AddRange(IEnumerable<T> entities);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Update(T entity, byte[] rowVersion);
        void UpdateRange(IEnumerable<T> entities);
        void Remove(Guid id);
        void Remove(Guid id, Guid compositeKey);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
