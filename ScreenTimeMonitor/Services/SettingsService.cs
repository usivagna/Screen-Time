using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Service for managing application settings using Windows Storage APIs
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDataContainer _localSettings;

        public SettingsService()
        {
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        #region General Settings

        public Task<bool> GetIsMonitoringEnabledAsync()
        {
            return Task.FromResult(_localSettings.Values.TryGetValue("IsMonitoringEnabled", out var value) 
                ? (bool)value : true);
        }

        public Task SetIsMonitoringEnabledAsync(bool enabled)
        {
            _localSettings.Values["IsMonitoringEnabled"] = enabled;
            return Task.CompletedTask;
        }

        public Task<bool> GetStartWithWindowsAsync()
        {
            return Task.FromResult(_localSettings.Values.TryGetValue("StartWithWindows", out var value) 
                ? (bool)value : false);
        }

        public Task SetStartWithWindowsAsync(bool startWithWindows)
        {
            _localSettings.Values["StartWithWindows"] = startWithWindows;
            return Task.CompletedTask;
        }

        #endregion

        #region Usage Limits

        public Task<TimeSpan?> GetDailyUsageLimitAsync(string appName)
        {
            var key = $"UsageLimit_{appName}";
            if (_localSettings.Values.TryGetValue(key, out var value) && value is long ticks)
            {
                return Task.FromResult<TimeSpan?>(TimeSpan.FromTicks(ticks));
            }
            return Task.FromResult<TimeSpan?>(null);
        }

        public Task SetDailyUsageLimitAsync(string appName, TimeSpan? limit)
        {
            var key = $"UsageLimit_{appName}";
            if (limit.HasValue)
            {
                _localSettings.Values[key] = limit.Value.Ticks;
            }
            else
            {
                _localSettings.Values.Remove(key);
            }
            return Task.CompletedTask;
        }

        public Task<Dictionary<string, TimeSpan>> GetAllUsageLimitsAsync()
        {
            var limits = new Dictionary<string, TimeSpan>();
            
            foreach (var kvp in _localSettings.Values.Where(x => x.Key.StartsWith("UsageLimit_")))
            {
                var appName = kvp.Key.Substring("UsageLimit_".Length);
                if (kvp.Value is long ticks)
                {
                    limits[appName] = TimeSpan.FromTicks(ticks);
                }
            }
            
            return Task.FromResult(limits);
        }

        #endregion

        #region Break Reminders

        public Task<bool> GetBreakRemindersEnabledAsync()
        {
            return Task.FromResult(_localSettings.Values.TryGetValue("BreakRemindersEnabled", out var value) 
                ? (bool)value : true);
        }

        public Task SetBreakRemindersEnabledAsync(bool enabled)
        {
            _localSettings.Values["BreakRemindersEnabled"] = enabled;
            return Task.CompletedTask;
        }

        public Task<TimeSpan> GetBreakReminderIntervalAsync()
        {
            if (_localSettings.Values.TryGetValue("BreakReminderInterval", out var value) && value is long ticks)
            {
                return Task.FromResult(TimeSpan.FromTicks(ticks));
            }
            return Task.FromResult(TimeSpan.FromHours(1)); // Default to 1 hour
        }

        public Task SetBreakReminderIntervalAsync(TimeSpan interval)
        {
            _localSettings.Values["BreakReminderInterval"] = interval.Ticks;
            return Task.CompletedTask;
        }

        #endregion

        #region Categories

        public Task<string> GetApplicationCategoryAsync(string appName)
        {
            var key = $"AppCategory_{appName}";
            return Task.FromResult(_localSettings.Values.TryGetValue(key, out var value) 
                ? (string)value : "Uncategorized");
        }

        public Task SetApplicationCategoryAsync(string appName, string category)
        {
            var key = $"AppCategory_{appName}";
            _localSettings.Values[key] = category;
            return Task.CompletedTask;
        }

        public Task<List<string>> GetAvailableCategoriesAsync()
        {
            var categories = new List<string>
            {
                "Productivity",
                "Development",
                "Web Browsing",
                "Entertainment",
                "Communication",
                "Gaming",
                "System",
                "Uncategorized"
            };
            
            return Task.FromResult(categories);
        }

        #endregion

        #region Data Retention

        public Task<int> GetDataRetentionDaysAsync()
        {
            return Task.FromResult(_localSettings.Values.TryGetValue("DataRetentionDays", out var value) 
                ? (int)value : 90); // Default to 90 days
        }

        public Task SetDataRetentionDaysAsync(int days)
        {
            _localSettings.Values["DataRetentionDays"] = days;
            return Task.CompletedTask;
        }

        #endregion

        #region UI Preferences

        public Task<string> GetThemeAsync()
        {
            return Task.FromResult(_localSettings.Values.TryGetValue("Theme", out var value) 
                ? (string)value : "Default");
        }

        public Task SetThemeAsync(string theme)
        {
            _localSettings.Values["Theme"] = theme;
            return Task.CompletedTask;
        }

        #endregion
    }
}
