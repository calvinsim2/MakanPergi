namespace Common.BackendJobs
{
    public class BackendJobOptions
    {
        public List<JobItem> JobItems { get; set; }
    }

    public class JobItem
    {
        public string Name { get; set; }
        public string Cron { get; set; }
    }
}
