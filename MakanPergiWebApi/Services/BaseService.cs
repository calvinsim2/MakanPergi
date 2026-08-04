using MakanPergiWebApi.Interfaces.Repository;

namespace MakanPergiWebApi.Services
{
    public class BaseService
    {
        protected readonly IUnitOfWork WorkContext;

        public BaseService(IUnitOfWork workContext)
        {
            WorkContext = workContext;
        }
    }
}
