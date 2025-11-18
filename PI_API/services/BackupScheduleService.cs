namespace PI_API.services
{
    public class BackupScheduleService : ScheduledService
    {
        public BackupScheduleService(ILogger<BackupScheduleService> logger): base(new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 6, 0, 0), logger)
        {
        }

        protected override async Task ExecuteJob(CancellationToken stoppingToken)
        {
            await BackupService.RunBackup();
            _dateScheduled = _dateScheduled.AddDays(1);
        }
    }
}
