using System;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Service for showing toast notifications using Windows App SDK
    /// </summary>
    public class NotificationService : INotificationService
    {
        public NotificationService()
        {
            // Initialize notification support
            // Note: Full implementation requires Microsoft.Windows.AppNotifications
            // which is available in Windows App SDK 1.6+
        }

        public Task ShowNotificationAsync(string title, string message)
        {
            try
            {
                // TODO: Implement with Microsoft.Windows.AppNotifications once available
                // For now, fallback to debug output
                System.Diagnostics.Debug.WriteLine($"Notification: {title} - {message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Notification failed: {title} - {message}. Error: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public Task ShowUsageLimitWarningAsync(string appName, TimeSpan usageTime, TimeSpan limit)
        {
            var title = "Usage Limit Warning";
            var percentUsed = (usageTime.TotalMinutes / limit.TotalMinutes) * 100;
            var message = $"You've used {appName} for {FormatTimeSpan(usageTime)} ({percentUsed:F0}% of your {FormatTimeSpan(limit)} daily limit)";
            
            return ShowNotificationAsync(title, message);
        }

        public Task ShowBreakReminderAsync(TimeSpan continuousUsage)
        {
            var title = "Time for a Break! 🧘";
            var message = $"You've been active for {FormatTimeSpan(continuousUsage)}. Take a short break to rest your eyes and stretch.";
            
            return ShowNotificationAsync(title, message);
        }

        public Task ShowDailySummaryAsync(TimeSpan totalTime, string mostUsedApp)
        {
            var title = "Daily Screen Time Summary 📊";
            var message = $"Total time today: {FormatTimeSpan(totalTime)}. Most used: {mostUsedApp}";
            
            return ShowNotificationAsync(title, message);
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
