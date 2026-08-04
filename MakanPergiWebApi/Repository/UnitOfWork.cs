using MakanPergi.Data;
using MakanPergi.Entities.Base;
using MakanPergiWebApi.Interfaces.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MakanPergiWebApi.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        protected readonly IDbContext _dbContext;
        private bool disposed = false;
        private IDictionary<Type, object> _repositories;

        public UnitOfWork(IDbContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new Dictionary<Type, object>();
        }

        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbContext.Instance.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("Concurrency Update Exception occured.");
            }
            catch (DbUpdateException e)
            {
                SqlException? innerException = e.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2601 || innerException.Number == 2607)) 
                {
                    throw new Exception("Duplicate record detected.");
                }

                throw new Exception("Something went wrong.");

            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : AbstractBaseEntity
        {
            if (_repositories.ContainsKey(typeof(TEntity))) 
            {
                return (IRepository<TEntity>)_repositories[typeof(TEntity)];
            }

            var repository = new Repository<TEntity>(_dbContext);
            _repositories.Add(typeof(TEntity), repository);
            return repository;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _dbContext.Instance.Dispose();
                }
            }

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
