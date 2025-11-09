using System;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Service for managing system tray integration
    /// </summary>
    public interface ISystemTrayService
    {
        /// <summary>
        /// Initialize the system tray icon
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Show the system tray icon
        /// </summary>
        Task ShowAsync();
        
        /// <summary>
        /// Hide the system tray icon
        /// </summary>
        Task HideAsync();
        
        /// <summary>
        /// Update the tray icon tooltip
        /// </summary>
        Task UpdateTooltipAsync(string tooltip);
        
        /// <summary>
        /// Indicates whether the tray icon is visible
        /// </summary>
        bool IsVisible { get; }
    }
}
