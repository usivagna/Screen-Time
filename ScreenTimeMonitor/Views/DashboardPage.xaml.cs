using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace ScreenTimeMonitor.Views
{
    public sealed partial class DashboardPage : Page
    {
        private readonly DispatcherTimer _updateTimer;

        public DashboardPage()
        {
            this.InitializeComponent();
            
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(1);
            _updateTimer.Tick += UpdateTimer_Tick;
            
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            // Initialize with default values
            TodayUsageText.Text = "0h 0m";
            ActiveAppsText.Text = "0";
            CurrentSessionText.Text = "0h 0m";
            CurrentAppText.Text = "No application detected";
            ActiveTimeText.Text = "0s";
        }

        private void UpdateTimer_Tick(object? sender, object e)
        {
            // Real-time monitoring logic will be added here
            CurrentAppText.Text = "Monitoring active...";
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

        private void StartMonitoring_Click(object sender, RoutedEventArgs e)
        {
            _updateTimer.Start();
            StartMonitoringBtn.IsEnabled = false;
            StopMonitoringBtn.IsEnabled = true;
        }

        private void StopMonitoring_Click(object sender, RoutedEventArgs e)
        {
            _updateTimer.Stop();
            StartMonitoringBtn.IsEnabled = true;
            StopMonitoringBtn.IsEnabled = false;
        }
    }
}