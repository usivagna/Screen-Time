// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ScreenTimeMonitor.ViewModels;
using ScreenTimeMonitor.Views;

namespace ScreenTimeMonitor
{
    /// <summary>
    /// Main window for the Screen Time Monitor application
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow(MainViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
            
            // Set window properties
            Title = "Screen Time Monitor";
            
            // Initialize the app
            _ = InitializeAsync();
        }

        private async System.Threading.Tasks.Task InitializeAsync()
        {
            await ViewModel.InitializeCommand.ExecuteAsync(null);
            
            // Navigate to default page
            ContentFrame.Navigate(typeof(DashboardPage), ViewModel.DashboardViewModel);
        }

        private void MainNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer?.Tag?.ToString() is string tag)
            {
                NavigateToPage(tag);
            }
            else if (args.IsSettingsSelected)
            {
                NavigateToPage("Settings");
            }
        }

        private void NavigateToPage(string pageTag)
        {
            try
            {
                switch (pageTag)
                {
                    case "Dashboard":
                        ContentFrame.Navigate(typeof(DashboardPage), ViewModel.DashboardViewModel);
                        break;
                    case "Reports":
                        var reportsViewModel = App.Services.GetRequiredService<ReportsViewModel>();
                        ContentFrame.Navigate(typeof(ReportsPage), reportsViewModel);
                        break;
                    case "Settings":
                        var settingsViewModel = App.Services.GetRequiredService<SettingsViewModel>();
                        ContentFrame.Navigate(typeof(SettingsPage), settingsViewModel);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        #region Helper Methods for Data Binding

        public string GetToggleButtonText(bool isMonitoring)
        {
            return isMonitoring ? "Stop Monitoring" : "Start Monitoring";
        }

        public string GetStatusText(bool isMonitoring, bool isInitialized)
        {
            if (!isInitialized)
                return "Initializing...";
            
            return isMonitoring ? "Monitoring screen time activity" : "Screen time monitoring is stopped";
        }

        public string GetMonitoringStatusText(bool isMonitoring)
        {
            return isMonitoring ? "● Active" : "● Stopped";
        }

        public Brush GetMonitoringStatusBrush(bool isMonitoring)
        {
            return isMonitoring 
                ? new SolidColorBrush(Microsoft.UI.Colors.Green) 
                : new SolidColorBrush(Microsoft.UI.Colors.Red);
        }

        public Visibility GetLoadingVisibility(bool isInitialized)
        {
            return isInitialized ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion
    }
}
