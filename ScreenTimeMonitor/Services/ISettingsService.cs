using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Interface for application settings management
    /// </summary>
    public interface ISettingsService
    {
        // General settings
        Task<bool> GetIsMonitoringEnabledAsync();
        Task SetIsMonitoringEnabledAsync(bool enabled);
        
        Task<bool> GetStartWithWindowsAsync();
        Task SetStartWithWindowsAsync(bool startWithWindows);
        
        // Usage limits
        Task<TimeSpan?> GetDailyUsageLimitAsync(string appName);
        Task SetDailyUsageLimitAsync(string appName, TimeSpan? limit);
        Task<Dictionary<string, TimeSpan>> GetAllUsageLimitsAsync();
        
        // Break reminders
        Task<bool> GetBreakRemindersEnabledAsync();
        Task SetBreakRemindersEnabledAsync(bool enabled);
        
        Task<TimeSpan> GetBreakReminderIntervalAsync();
        Task SetBreakReminderIntervalAsync(TimeSpan interval);
        
        // Categories
        Task<string> GetApplicationCategoryAsync(string appName);
        Task SetApplicationCategoryAsync(string appName, string category);
        Task<List<string>> GetAvailableCategoriesAsync();
        
        // Data retention
        Task<int> GetDataRetentionDaysAsync();
        Task SetDataRetentionDaysAsync(int days);
        
        // UI preferences
        Task<string> GetThemeAsync();
        Task SetThemeAsync(string theme);
    }
}
