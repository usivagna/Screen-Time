using System;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Interface for application notification services
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Show a toast notification
        /// </summary>
        Task ShowNotificationAsync(string title, string message);
        
        /// <summary>
        /// Show a usage limit warning notification
        /// </summary>
        Task ShowUsageLimitWarningAsync(string appName, TimeSpan usageTime, TimeSpan limit);
        
        /// <summary>
        /// Show a break reminder notification
        /// </summary>
        Task ShowBreakReminderAsync(TimeSpan continuousUsage);
        
        /// <summary>
        /// Show daily summary notification
        /// </summary>
        Task ShowDailySummaryAsync(TimeSpan totalTime, string mostUsedApp);
    }
}
