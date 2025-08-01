using System;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Service for showing toast notifications
    /// </summary>
    public class NotificationService : INotificationService
    {
        public async Task ShowNotificationAsync(string title, string message)
        {
            // For now, we'll implement basic notifications
            // In a full implementation, you would use Windows App SDK notifications
            await Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine($"Notification: {title} - {message}");
                // TODO: Implement proper toast notifications using Windows App SDK
            });
        }

        public async Task ShowUsageLimitWarningAsync(string appName, TimeSpan usageTime, TimeSpan limit)
        {
            var title = "Usage Limit Warning";
            var message = $"You've been using {appName} for {usageTime:hh\\:mm} out of your {limit:hh\\:mm} daily limit.";
            await ShowNotificationAsync(title, message);
        }

        public async Task ShowBreakReminderAsync(TimeSpan continuousUsage)
        {
            var title = "Time for a Break";
            var message = $"You've been active for {continuousUsage:hh\\:mm}. Consider taking a short break!";
            await ShowNotificationAsync(title, message);
        }

        public async Task ShowDailySummaryAsync(TimeSpan totalTime, string mostUsedApp)
        {
            var title = "Daily Screen Time Summary";
            var message = $"Today's total: {totalTime:hh\\:mm}. Most used: {mostUsedApp}";
            await ShowNotificationAsync(title, message);
        }
    }
}
