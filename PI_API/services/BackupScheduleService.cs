/*namespace PI_API.services
{
    public class BackupScheduleService : ScheduledService
    {
        public BackupScheduleService(ILogger<BackupScheduleService> logger)
            : base(TimeSpan.FromHours(1), logger)   // exemplo: roda de hora em hora
        {
        }

        protected override async Task ExecuteJob(CancellationToken stoppingToken)
        {
            //await BackupService.RunBackup();
        }
    }
}*/
