using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace authica.Services
{

    public class GeolocationDbUpdateService : IHostedService, IDisposable
    {
        const int INITIAL_UPDATE_IN_SECONDS = 15;
        const int REGULAR_UPDATE_IN_DAYS = 7;
        readonly ILogger<GeolocationDbUpdateService> _logger;
        readonly GeolocationDbDownloadService _downloadService;
        Timer _timer;
        public GeolocationDbUpdateService(ILogger<GeolocationDbUpdateService> logger, GeolocationDbDownloadService downloadService)
        {
            _logger = logger;
            _downloadService = downloadService;
            _timer = new Timer(Update);
        }

        public void Dispose() => _timer?.Dispose();
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(C.Configuration.Current.MaxMindLicenseKey))
            {
                _logger.LogInformation("No MaxMind license key configured, disabling Geolocation Update Service");
                return Task.CompletedTask;
            }

            _logger.LogInformation("Geolocation Update Service starting");

            var now = DateTime.UtcNow;
            var updateIn = TimeSpan.FromSeconds(INITIAL_UPDATE_IN_SECONDS);

            if (C.GeoLocationDbFile.Exists)
            {
                var updateAfter = C.GeoLocationDbFile.LastWriteTimeUtc.AddDays(REGULAR_UPDATE_IN_DAYS);
                if (updateAfter > now)
                    updateIn = updateAfter - now;
            }

            ScheduleNext(updateIn);
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Geolocation Update Service stopped");

            _timer?.Change(Timeout.Infinite, Timeout.Infinite);

            return Task.CompletedTask;
        }

        void ScheduleNext(TimeSpan updateIn)
        {
            _logger.LogInformation(
                "Geolocation Db will update in {Days} day(s) {Hour:00}h {Minute:00}m @ {@Scheduled}",
                updateIn.Days,
                updateIn.Hours,
                updateIn.Minutes,
                DateTime.UtcNow.Add(updateIn)
            );

            _timer.Change(updateIn, Timeout.InfiniteTimeSpan);
        }
        async void Update(object? state)
        {
            if (string.IsNullOrWhiteSpace(C.Configuration.Current.MaxMindLicenseKey))
                return;

            _logger.LogInformation("Updating Geolocation Db...");

            var downloaded = await _downloadService.DownloadDb();
            if (downloaded)
            {
                _logger.LogInformation("Geolocation Db updated");
                ScheduleNext(TimeSpan.FromDays(REGULAR_UPDATE_IN_DAYS));
            }
            else
                _logger.LogError("Geolocation Db Update failed");
        }
    }
}