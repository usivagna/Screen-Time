using System;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Service for monitoring usage limits and triggering notifications
    /// </summary>
    public interface IUsageLimitService
    {
        /// <summary>
        /// Start monitoring usage limits
        /// </summary>
        Task StartMonitoringAsync();
        
        /// <summary>
        /// Stop monitoring usage limits
        /// </summary>
        Task StopMonitoringAsync();
        
        /// <summary>
        /// Check if a specific application has exceeded its limit
        /// </summary>
        Task<bool> HasExceededLimitAsync(string appName, DateTime date);
        
        /// <summary>
        /// Get the percentage of limit used for an application today
        /// </summary>
        Task<double> GetLimitUsagePercentageAsync(string appName);
        
        /// <summary>
        /// Indicates whether the service is currently monitoring
        /// </summary>
        bool IsMonitoring { get; }
    }
}
