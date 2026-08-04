namespace Common.BackendJobs
{
    public interface IBackendJob
    {
        Task RunAsync(JobItem jobItem);
    }
}
