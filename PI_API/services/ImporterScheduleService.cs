namespace PI_API.services
{
    public class ImporterScheduleService : ScheduledService
    {
        public ImporterScheduleService(ILogger<ImporterScheduleService> logger) : base(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 28, 3, 0, 0), logger)
        {

        }

        protected override async Task ExecuteJob(CancellationToken stoppingToken)
        {
            await ImporterService.Start();
            _dateScheduled = _dateScheduled.AddMonths(1);
        }

        /*protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
        }*/
    }
}
