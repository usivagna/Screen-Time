using Microsoft.UI.Dispatching;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Service for monitoring active windows using Windows APIs
    /// </summary>
    public class WindowMonitoringService : IWindowMonitoringService
    {
        #region Windows API Declarations
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        #endregion

        private DispatcherQueueTimer? _monitoringTimer;
        private WindowInfo? _currentWindow;
        private readonly DispatcherQueue _dispatcherQueue;

        public event EventHandler<WindowChangedEventArgs>? WindowChanged;
        public bool IsMonitoring { get; private set; }

        public WindowMonitoringService()
        {
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        public Task StartMonitoringAsync()
        {
            if (IsMonitoring)
                return Task.CompletedTask;

            _monitoringTimer = _dispatcherQueue.CreateTimer();
            _monitoringTimer.Interval = TimeSpan.FromMilliseconds(1000); // Check every second
            _monitoringTimer.IsRepeating = true;
            _monitoringTimer.Tick += OnMonitoringTimerTick;
            
            _monitoringTimer.Start();
            IsMonitoring = true;

            return Task.CompletedTask;
        }

        public Task StopMonitoringAsync()
        {
            if (!IsMonitoring)
                return Task.CompletedTask;

            _monitoringTimer?.Stop();
            _monitoringTimer = null;
            IsMonitoring = false;

            return Task.CompletedTask;
        }

        public async Task<WindowInfo?> GetCurrentWindowAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var windowHandle = GetForegroundWindow();
                    if (windowHandle == IntPtr.Zero)
                        return null;

                    return GetWindowInfo(windowHandle);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting current window: {ex.Message}");
                    return null;
                }
            });
        }

        private async void OnMonitoringTimerTick(object sender, object e)
        {
            try
            {
                var currentWindow = await GetCurrentWindowAsync();
                
                if (currentWindow != null && !IsSameWindow(currentWindow, _currentWindow))
                {
                    var previousWindow = _currentWindow;
                    _currentWindow = currentWindow;

                    WindowChanged?.Invoke(this, new WindowChangedEventArgs
                    {
                        PreviousWindow = previousWindow,
                        CurrentWindow = currentWindow,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in monitoring timer: {ex.Message}");
            }
        }

        private static WindowInfo? GetWindowInfo(IntPtr windowHandle)
        {
            try
            {
                // Get window title
                var titleBuilder = new StringBuilder(256);
                GetWindowText(windowHandle, titleBuilder, titleBuilder.Capacity);
                var windowTitle = titleBuilder.ToString();

                // Get process ID
                GetWindowThreadProcessId(windowHandle, out uint processId);
                
                // Get process information
                string processName = "Unknown";
                string executablePath = "Unknown";
                
                try
                {
                    using var process = Process.GetProcessById((int)processId);
                    processName = process.ProcessName;
                    executablePath = process.MainModule?.FileName ?? processName;
                }
                catch
                {
                    // If we can't get process info, use what we have
                }
                
                var windowInfo = new WindowInfo
                {
                    WindowHandle = windowHandle,
                    WindowTitle = windowTitle,
                    ProcessName = processName,
                    ProcessId = (int)processId,
                    ExecutablePath = executablePath
                };

                return windowInfo;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting window info: {ex.Message}");
                return null;
            }
        }

        private static bool IsSameWindow(WindowInfo? window1, WindowInfo? window2)
        {
            if (window1 == null || window2 == null)
                return false;

            return window1.WindowHandle == window2.WindowHandle &&
                   window1.ProcessId == window2.ProcessId;
        }
    }
}
