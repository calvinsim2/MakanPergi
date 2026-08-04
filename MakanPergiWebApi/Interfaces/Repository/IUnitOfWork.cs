using MakanPergi.Entities.Base;

namespace MakanPergiWebApi.Interfaces.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : AbstractBaseEntity;
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
    }
}
