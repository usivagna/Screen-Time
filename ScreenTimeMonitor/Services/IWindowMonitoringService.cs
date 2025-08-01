using System;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Interface for monitoring active windows and applications
    /// </summary>
    public interface IWindowMonitoringService
    {
        /// <summary>
        /// Event fired when the active window changes
        /// </summary>
        event EventHandler<WindowChangedEventArgs>? WindowChanged;
        
        /// <summary>
        /// Start monitoring window changes
        /// </summary>
        Task StartMonitoringAsync();
        
        /// <summary>
        /// Stop monitoring window changes
        /// </summary>
        Task StopMonitoringAsync();
        
        /// <summary>
        /// Get information about the currently active window
        /// </summary>
        Task<WindowInfo?> GetCurrentWindowAsync();
        
        /// <summary>
        /// Whether monitoring is currently active
        /// </summary>
        bool IsMonitoring { get; }
    }

    /// <summary>
    /// Event arguments for window change events
    /// </summary>
    public class WindowChangedEventArgs : EventArgs
    {
        public WindowInfo? PreviousWindow { get; set; }
        public WindowInfo CurrentWindow { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Information about a window and its associated application
    /// </summary>
    public class WindowInfo
    {
        public string WindowTitle { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public IntPtr WindowHandle { get; set; }
    }
}
