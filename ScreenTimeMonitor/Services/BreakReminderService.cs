using Microsoft.UI.Dispatching;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Service for managing periodic break reminders
    /// </summary>
    public class BreakReminderService : IBreakReminderService
    {
        private readonly ISettingsService _settingsService;
        private readonly INotificationService _notificationService;
        private readonly DispatcherQueue _dispatcherQueue;
        private DispatcherQueueTimer? _reminderTimer;
        private DateTime _lastBreakTime;

        public bool IsRunning { get; private set; }

        public BreakReminderService(
            ISettingsService settingsService,
            INotificationService notificationService)
        {
            _settingsService = settingsService;
            _notificationService = notificationService;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _lastBreakTime = DateTime.UtcNow;
        }

        public async Task StartAsync()
        {
            if (IsRunning)
                return;

            var enabled = await _settingsService.GetBreakRemindersEnabledAsync();
            if (!enabled)
            {
                Debug.WriteLine("Break reminders are disabled in settings");
                return;
            }

            var interval = await _settingsService.GetBreakReminderIntervalAsync();
            
            _reminderTimer = _dispatcherQueue.CreateTimer();
            _reminderTimer.Interval = interval;
            _reminderTimer.IsRepeating = true;
            _reminderTimer.Tick += OnReminderTimerTick;
            
            _lastBreakTime = DateTime.UtcNow;
            _reminderTimer.Start();
            IsRunning = true;

            Debug.WriteLine($"Break reminder service started with interval: {interval}");
        }

        public Task StopAsync()
        {
            if (!IsRunning)
                return Task.CompletedTask;

            _reminderTimer?.Stop();
            _reminderTimer = null;
            IsRunning = false;

            Debug.WriteLine("Break reminder service stopped");
            return Task.CompletedTask;
        }

        public Task ResetTimerAsync()
        {
            _lastBreakTime = DateTime.UtcNow;
            Debug.WriteLine("Break timer reset");
            return Task.CompletedTask;
        }

        public TimeSpan GetTimeSinceLastBreak()
        {
            return DateTime.UtcNow - _lastBreakTime;
        }

        private async void OnReminderTimerTick(object sender, object e)
        {
            try
            {
                var timeSinceLastBreak = GetTimeSinceLastBreak();
                await _notificationService.ShowBreakReminderAsync(timeSinceLastBreak);
                
                Debug.WriteLine($"Break reminder shown after {timeSinceLastBreak}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing break reminder: {ex.Message}");
            }
        }
    }
}
