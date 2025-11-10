#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ScreenTimeMonitor.Views;
using ScreenTimeMonitor.Services;
using System;

namespace ScreenTimeMonitor
{
    public sealed partial class MainWindow : Window
    {
        private Type? _currentPageType;
        private readonly IUsageLimitService? _usageLimitService;
        private readonly IBreakReminderService? _breakReminderService;
        private readonly ISystemTrayService? _systemTrayService;
        private readonly ISettingsService? _settingsService;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "Screen Time Monitor";
            
            // Get services from DI container
            try
            {
                _usageLimitService = (IUsageLimitService?)App.Host?.Services.GetService(typeof(IUsageLimitService));
                _breakReminderService = (IBreakReminderService?)App.Host?.Services.GetService(typeof(IBreakReminderService));
                _systemTrayService = (ISystemTrayService?)App.Host?.Services.GetService(typeof(ISystemTrayService));
                _settingsService = (ISettingsService?)App.Host?.Services.GetService(typeof(ISettingsService));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting services: {ex.Message}");
            }
            
            // Initialize services
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
            
            // Navigate to dashboard by default
            NavigateToPage(typeof(DashboardPage));
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialize system tray
                if (_systemTrayService != null)
                {
                    await _systemTrayService.InitializeAsync();
                    await _systemTrayService.ShowAsync();
                }

                // Start break reminders if enabled
                if (_breakReminderService != null && _settingsService != null)
                {
                    var breakRemindersEnabled = await _settingsService.GetBreakRemindersEnabledAsync();
                    if (breakRemindersEnabled)
                    {
                        await _breakReminderService.StartAsync();
                    }
                }

                // Start usage limit monitoring
                if (_usageLimitService != null)
                {
                    await _usageLimitService.StartMonitoringAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing services: {ex.Message}");
            }
        }

        private async void MainWindow_Closed(object sender, WindowEventArgs e)
        {
            try
            {
                // Stop services gracefully
                if (_usageLimitService != null && _usageLimitService.IsMonitoring)
                {
                    await _usageLimitService.StopMonitoringAsync();
                }

                if (_breakReminderService != null && _breakReminderService.IsRunning)
                {
                    await _breakReminderService.StopAsync();
                }

                if (_systemTrayService != null && _systemTrayService.IsVisible)
                {
                    await _systemTrayService.HideAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping services: {ex.Message}");
            }
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(DashboardPage));
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(ReportsPage));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(SettingsPage));
        }

        private void NavigateToPage(Type pageType)
        {
            // Only navigate if we're not already on this page
            if (_currentPageType != pageType)
            {
                ContentFrame.Navigate(pageType);
                _currentPageType = pageType;
            }
        }
    }
}
