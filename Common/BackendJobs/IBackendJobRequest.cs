using MediatR;

namespace Common.BackendJobs
{
    public interface IBackendJobRequest<T> : IRequest<T> { }
    public interface IBackendJobRequest : IRequest { }
}
