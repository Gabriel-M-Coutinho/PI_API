namespace PI_API.services
{
    public class ImporterScheduleService : ScheduledService
    {
        private ImporterService _importerService;
        public ImporterScheduleService(ILogger<ImporterScheduleService> logger, ImporterService importerService) : base(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 21, 14, 08, 0), logger)
        {
            _importerService = importerService;
        }

        protected override async Task ExecuteJob(CancellationToken stoppingToken)
        {
           // await _importerService.Start();
            _dateScheduled = _dateScheduled.AddMonths(1);
        }

        /*protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
        }*/
    }
}
