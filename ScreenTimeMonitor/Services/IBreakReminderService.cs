using System;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Service for managing break reminders
    /// </summary>
    public interface IBreakReminderService
    {
        /// <summary>
        /// Start the break reminder service
        /// </summary>
        Task StartAsync();
        
        /// <summary>
        /// Stop the break reminder service
        /// </summary>
        Task StopAsync();
        
        /// <summary>
        /// Reset the timer (e.g., when user takes a break)
        /// </summary>
        Task ResetTimerAsync();
        
        /// <summary>
        /// Get the time since last break
        /// </summary>
        TimeSpan GetTimeSinceLastBreak();
        
        /// <summary>
        /// Indicates whether the service is currently running
        /// </summary>
        bool IsRunning { get; }
    }
}
