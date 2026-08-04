using Hangfire;
using Hangfire.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Common.BackendJobs
{
    public class HangfireJob : IBackendJob
    {
        private readonly BackendJobOptions _jobOptions;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IMediator _mediator;
        private readonly ILogger<HangfireJob> _logger;
        private readonly Dictionary<string, IRequest> _jobServiceDic = new Dictionary<string, IRequest>();

        public HangfireJob(ILogger<HangfireJob> logger,
            IOptions<BackendJobOptions> jobOptions,
            IServiceProvider serviceProvider,
            Func<string, IRequest> serviceAccessor
            )
        {
            _jobOptions = jobOptions.Value;
            _logger = logger;
            _mediator = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IMediator>();
            _recurringJobManager = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IRecurringJobManager>();
            foreach (var jobItem in _jobOptions.JobItems)
            {
                if (jobItem != null && !string.IsNullOrWhiteSpace(jobItem.Name) && !_jobServiceDic.ContainsKey(jobItem.Name))
                {
                    var service = serviceAccessor(jobItem.Name);
                    if (service != null)
                    {
                        _jobServiceDic.Add(jobItem.Name, service);
                    }
                }
            }
            foreach (var jobItem in _jobOptions.JobItems)
            {
                if (jobItem != null && !string.IsNullOrWhiteSpace(jobItem.Name) && _jobServiceDic.ContainsKey(jobItem.Name))
                {
                    if (string.IsNullOrWhiteSpace(jobItem.Cron))
                    {
                        _recurringJobManager.AddOrUpdate(jobItem.Name, Job.FromExpression(() => RunAsync(jobItem)), Cron.Minutely(), TimeZoneInfo.Local);
                    }
                    else
                    {
                        _recurringJobManager.AddOrUpdate(jobItem.Name, Job.FromExpression(() => RunAsync(jobItem)), jobItem.Cron, TimeZoneInfo.Local);
                    }
                }
            }
        }

        public async Task RunAsync(JobItem jobItem)
        {
            _logger.LogInformation("The backend job [{JobName}] is triggered", jobItem.Name);
            if (_jobServiceDic.ContainsKey(jobItem.Name))
            {
                await _mediator.Send(_jobServiceDic[jobItem.Name]);
            }
        }
    }
}
