namespace PI_API.services;

public abstract class ScheduledService : BackgroundService
{
    protected DateTime _dateScheduled;
    private readonly ILogger _logger;

    protected ScheduledService(DateTime dateScheduled, ILogger logger)
    {
        _dateScheduled = dateScheduled;
        _logger = logger;
    }

    protected abstract Task ExecuteJob(CancellationToken stoppingToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            TimeSpan delay = _dateScheduled - DateTime.Now;

            if (delay > TimeSpan.Zero)
            {
                _logger.LogInformation($"Backup agendado para {_dateScheduled}");
                await Task.Delay(delay, stoppingToken);
            }

            await ExecuteJob(stoppingToken);
        }
    }
}
