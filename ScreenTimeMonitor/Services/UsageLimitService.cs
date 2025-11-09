using Microsoft.UI.Dispatching;
using ScreenTimeMonitor.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Service for monitoring application usage limits and showing warnings
    /// </summary>
    public class UsageLimitService : IUsageLimitService
    {
        private readonly IDataService _dataService;
        private readonly ISettingsService _settingsService;
        private readonly INotificationService _notificationService;
        private readonly DispatcherQueue _dispatcherQueue;
        private DispatcherQueueTimer? _checkTimer;

        public bool IsMonitoring { get; private set; }

        public UsageLimitService(
            IDataService dataService,
            ISettingsService settingsService,
            INotificationService notificationService)
        {
            _dataService = dataService;
            _settingsService = settingsService;
            _notificationService = notificationService;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        public Task StartMonitoringAsync()
        {
            if (IsMonitoring)
                return Task.CompletedTask;

            _checkTimer = _dispatcherQueue.CreateTimer();
            _checkTimer.Interval = TimeSpan.FromMinutes(5); // Check every 5 minutes
            _checkTimer.IsRepeating = true;
            _checkTimer.Tick += OnCheckTimerTick;
            
            _checkTimer.Start();
            IsMonitoring = true;

            Debug.WriteLine("Usage limit monitoring started");
            return Task.CompletedTask;
        }

        public Task StopMonitoringAsync()
        {
            if (!IsMonitoring)
                return Task.CompletedTask;

            _checkTimer?.Stop();
            _checkTimer = null;
            IsMonitoring = false;

            Debug.WriteLine("Usage limit monitoring stopped");
            return Task.CompletedTask;
        }

        public async Task<bool> HasExceededLimitAsync(string appName, DateTime date)
        {
            var limit = await _settingsService.GetDailyUsageLimitAsync(appName);
            if (!limit.HasValue)
                return false;

            var usage = await _dataService.GetApplicationUsageAsync(date);
            if (usage.TryGetValue(appName, out var usageTime))
            {
                return usageTime >= limit.Value;
            }

            return false;
        }

        public async Task<double> GetLimitUsagePercentageAsync(string appName)
        {
            var limit = await _settingsService.GetDailyUsageLimitAsync(appName);
            if (!limit.HasValue)
                return 0;

            var usage = await _dataService.GetApplicationUsageAsync(DateTime.Today);
            if (usage.TryGetValue(appName, out var usageTime))
            {
                return (usageTime.TotalSeconds / limit.Value.TotalSeconds) * 100;
            }

            return 0;
        }

        private async void OnCheckTimerTick(object sender, object e)
        {
            try
            {
                await CheckUsageLimitsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking usage limits: {ex.Message}");
            }
        }

        private async Task CheckUsageLimitsAsync()
        {
            var today = DateTime.Today;
            var limits = await _settingsService.GetAllUsageLimitsAsync();

            foreach (var (appName, limit) in limits)
            {
                var usage = await _dataService.GetApplicationUsageAsync(today);
                if (usage.TryGetValue(appName, out var usageTime))
                {
                    var percentage = (usageTime.TotalSeconds / limit.TotalSeconds) * 100;

                    // Show warning at 80% and 100%
                    if (percentage >= 80 && percentage < 100)
                    {
                        await _notificationService.ShowUsageLimitWarningAsync(appName, usageTime, limit);
                    }
                    else if (percentage >= 100)
                    {
                        var title = "Usage Limit Exceeded";
                        var message = $"You've exceeded your daily limit for {appName} ({FormatTimeSpan(usageTime)} / {FormatTimeSpan(limit)})";
                        await _notificationService.ShowNotificationAsync(title, message);
                    }
                }
            }
        }

        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            }
            return $"{timeSpan.Minutes}m";
        }
    }
}
