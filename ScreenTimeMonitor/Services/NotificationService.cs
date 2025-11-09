using System;
using System.Threading.Tasks;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Service for showing toast notifications using Windows App SDK
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly AppNotificationManager _notificationManager;

        public NotificationService()
        {
            _notificationManager = AppNotificationManager.Default;
            _notificationManager.NotificationInvoked += OnNotificationInvoked;
        }

        public Task ShowNotificationAsync(string title, string message)
        {
            try
            {
                var notification = new AppNotificationBuilder()
                    .AddText(title)
                    .AddText(message)
                    .BuildNotification();

                _notificationManager.Show(notification);
            }
            catch (Exception ex)
            {
                // Fallback to debug output if notifications fail
                System.Diagnostics.Debug.WriteLine($"Notification failed: {title} - {message}. Error: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public async Task ShowUsageLimitWarningAsync(string appName, TimeSpan usageTime, TimeSpan limit)
        {
            var title = "Usage Limit Warning";
            var percentUsed = (usageTime.TotalMinutes / limit.TotalMinutes) * 100;
            var message = $"You've used {appName} for {FormatTimeSpan(usageTime)} ({percentUsed:F0}% of your {FormatTimeSpan(limit)} daily limit)";
            
            try
            {
                var notification = new AppNotificationBuilder()
                    .AddText(title)
                    .AddText(message)
                    .AddArgument("action", "limitWarning")
                    .AddArgument("app", appName)
                    .BuildNotification();

                _notificationManager.Show(notification);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Usage limit notification failed: {ex.Message}");
            }
        }

        public async Task ShowBreakReminderAsync(TimeSpan continuousUsage)
        {
            var title = "Time for a Break! 🧘";
            var message = $"You've been active for {FormatTimeSpan(continuousUsage)}. Take a short break to rest your eyes and stretch.";
            
            try
            {
                var notification = new AppNotificationBuilder()
                    .AddText(title)
                    .AddText(message)
                    .AddArgument("action", "breakReminder")
                    .AddButton(new AppNotificationButton("Snooze 5 min")
                        .AddArgument("snooze", "5"))
                    .AddButton(new AppNotificationButton("Dismiss")
                        .AddArgument("dismiss", "true"))
                    .BuildNotification();

                _notificationManager.Show(notification);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Break reminder notification failed: {ex.Message}");
            }
        }

        public async Task ShowDailySummaryAsync(TimeSpan totalTime, string mostUsedApp)
        {
            var title = "Daily Screen Time Summary 📊";
            var message = $"Total time today: {FormatTimeSpan(totalTime)}. Most used: {mostUsedApp}";
            
            try
            {
                var notification = new AppNotificationBuilder()
                    .AddText(title)
                    .AddText(message)
                    .AddArgument("action", "dailySummary")
                    .AddButton(new AppNotificationButton("View Details")
                        .AddArgument("view", "reports"))
                    .BuildNotification();

                _notificationManager.Show(notification);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Daily summary notification failed: {ex.Message}");
            }
        }

        private void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            // Handle notification interactions
            System.Diagnostics.Debug.WriteLine($"Notification invoked with arguments: {args.Argument}");
            // TODO: Navigate to appropriate page based on action
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
