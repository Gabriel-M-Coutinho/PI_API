namespace PI_API.services;

public class ScheduleService : BackgroundService
{
    private readonly TimeSpan time = TimeSpan.FromSeconds(1);
    private readonly ILogger<ScheduleService> _logger;
    public ScheduleService(ILogger<ScheduleService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
    {
        using PeriodicTimer timer = new PeriodicTimer(time);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            Console.WriteLine("Fui executado");
        }
    }
}
