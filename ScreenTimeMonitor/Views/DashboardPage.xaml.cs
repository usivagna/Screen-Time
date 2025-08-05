#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ScreenTimeMonitor.Services;
using System;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Views
{
    public sealed partial class DashboardPage : Page
    {
        private readonly DispatcherTimer _updateTimer;
        private readonly IWindowMonitoringService _monitoringService;
        private readonly IDataService _dataService;
        private DateTime _sessionStartTime;
        private string _currentAppName = "No application detected";
        private DateTime _currentAppStartTime;

        public DashboardPage()
        {
            this.InitializeComponent();
            
            // Get services from DI container
            _monitoringService = App.Host?.Services.GetRequiredService<IWindowMonitoringService>() 
                ?? throw new InvalidOperationException("WindowMonitoringService not available");
            _dataService = App.Host?.Services.GetRequiredService<IDataService>() 
                ?? throw new InvalidOperationException("DataService not available");
            
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(1);
            _updateTimer.Tick += UpdateTimer_Tick;
            
            // Subscribe to window change events
            _monitoringService.WindowChanged += OnWindowChanged;
            
            _sessionStartTime = DateTime.Now;
            _currentAppStartTime = DateTime.Now;
            
            LoadDashboardData();
        }

        private async void LoadDashboardData()
        {
            try
            {
                // Initialize database
                await _dataService.InitializeDatabaseAsync();
                
                // Load today's usage data
                await UpdateTodayUsage();
                
                // Initialize display values
                CurrentSessionText.Text = "0h 0m";
                CurrentAppText.Text = _currentAppName;
                ActiveTimeText.Text = "0s";
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
                
                // Set default values
                TodayUsageText.Text = "0h 0m";
                ActiveAppsText.Text = "0";
                CurrentSessionText.Text = "0h 0m";
                CurrentAppText.Text = "Error loading data";
                ActiveTimeText.Text = "0s";
            }
        }

        private async Task UpdateTodayUsage()
        {
            try
            {
                var today = DateTime.Today;
                var applications = await _dataService.GetApplicationsAsync();
                
                // This is a placeholder - you'd implement actual usage calculation
                TodayUsageText.Text = "0h 0m"; // Will be updated with real data
                ActiveAppsText.Text = applications.Count.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating today's usage: {ex.Message}");
                TodayUsageText.Text = "Error";
                ActiveAppsText.Text = "0";
            }
        }

        private async void OnWindowChanged(object? sender, WindowChangedEventArgs e)
        {
            try
            {
                // Update current app info
                _currentAppName = e.CurrentWindow?.ProcessName ?? "Unknown Application";
                _currentAppStartTime = DateTime.Now;
                
                // Update database with the previous window session
                if (e.PreviousWindow != null)
                {
                    await SaveWindowSession(e.PreviousWindow, e.Timestamp);
                }
                
                // Update UI on dispatcher thread
                DispatcherQueue.TryEnqueue(() =>
                {
                    CurrentAppText.Text = _currentAppName;
                    ActiveTimeText.Text = "0s";
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling window change: {ex.Message}");
            }
        }

        private async Task SaveWindowSession(WindowInfo windowInfo, DateTime endTime)
        {
            try
            {
                var application = await _dataService.GetOrCreateApplicationAsync(
                    windowInfo.ProcessName, 
                    windowInfo.ExecutablePath);
                
                var session = await _dataService.StartUsageSessionAsync(
                    application.Id, 
                    windowInfo.WindowTitle);
                
                // Calculate duration and end the session
                var duration = endTime - _currentAppStartTime;
                if (duration.TotalSeconds > 1) // Only save sessions longer than 1 second
                {
                    // Update session end time (you may need to modify the session object)
                    // For now, we'll just end the session immediately
                    await _dataService.EndUsageSessionAsync(session);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving window session: {ex.Message}");
            }
        }

        private void UpdateTimer_Tick(object? sender, object e)
        {
            try
            {
                // Update session time
                var sessionDuration = DateTime.Now - _sessionStartTime;
                CurrentSessionText.Text = FormatTimeSpan(sessionDuration);
                
                // Update current app active time
                var appDuration = DateTime.Now - _currentAppStartTime;
                ActiveTimeText.Text = FormatTimeSpan(appDuration);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating timer: {ex.Message}");
            }
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            else if (timeSpan.TotalMinutes >= 1)
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
            else
                return $"{timeSpan.Seconds}s";
        }

        private async void StartMonitoring_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _monitoringService.StartMonitoringAsync();
                _updateTimer.Start();
                _sessionStartTime = DateTime.Now;
                _currentAppStartTime = DateTime.Now;
                
                StartMonitoringBtn.IsEnabled = false;
                StopMonitoringBtn.IsEnabled = true;
                
                // Get current window immediately
                var currentWindow = await _monitoringService.GetCurrentWindowAsync();
                if (currentWindow != null)
                {
                    _currentAppName = currentWindow.ProcessName;
                    CurrentAppText.Text = _currentAppName;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting monitoring: {ex.Message}");
                CurrentAppText.Text = "Error starting monitoring";
            }
        }

        private async void StopMonitoring_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _monitoringService.StopMonitoringAsync();
                _updateTimer.Stop();
                
                StartMonitoringBtn.IsEnabled = true;
                StopMonitoringBtn.IsEnabled = false;
                
                CurrentAppText.Text = "Monitoring stopped";
                ActiveTimeText.Text = "0s";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping monitoring: {ex.Message}");
            }
        }
    }
}