using MediatR;

namespace MakanPergiWebApi.Infrastructure.Jobs
{
    public class DemoBatchJobRequest : IRequest { }
    // MediatR (14.1.0) no longer supports RequestHandler<>, thus we use IRequestHandler<>
    // thus, we do not need the intermediary Interface (which is supposed to be IDemoBatchJobRequest : IRequest {}
    // and back in Program.cs, we no longer need to register that line -> addScoped<IDemoBatchJobRequest, DemoBatchJob>
    public class DemoBatchJob : IRequestHandler<DemoBatchJobRequest>
    {
        private readonly ILogger<DemoBatchJob> _logger;

        public DemoBatchJob(ILogger<DemoBatchJob> logger)
        {
            _logger = logger;
        }

        public Task Handle(DemoBatchJobRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{JobName} Begin.", nameof(DemoBatchJob));
            return Task.CompletedTask;
        }
    }
}
