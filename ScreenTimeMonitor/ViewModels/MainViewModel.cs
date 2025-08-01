using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenTimeMonitor.Services;
using System;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.ViewModels
{
    /// <summary>
    /// Main ViewModel for the application window
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IWindowMonitoringService _windowMonitoringService;
        private readonly IDataService _dataService;
        private readonly ISettingsService _settingsService;

        [ObservableProperty]
        private string _title = "Screen Time Monitor";

        [ObservableProperty]
        private bool _isMonitoring;

        [ObservableProperty]
        private string _currentStatus = "Ready";

        [ObservableProperty]
        private DashboardViewModel _dashboardViewModel;

        [ObservableProperty]
        private bool _isInitialized;

        public MainViewModel(
            IWindowMonitoringService windowMonitoringService,
            IDataService dataService,
            ISettingsService settingsService,
            DashboardViewModel dashboardViewModel)
        {
            _windowMonitoringService = windowMonitoringService;
            _dataService = dataService;
            _settingsService = settingsService;
            _dashboardViewModel = dashboardViewModel;

            // Subscribe to monitoring service events
            _windowMonitoringService.WindowChanged += OnWindowChanged;
        }

        [RelayCommand]
        private async Task InitializeAsync()
        {
            try
            {
                CurrentStatus = "Initializing database...";
                await _dataService.InitializeDatabaseAsync();

                CurrentStatus = "Loading settings...";
                IsMonitoring = await _settingsService.GetIsMonitoringEnabledAsync();

                if (IsMonitoring)
                {
                    CurrentStatus = "Starting monitoring...";
                    await StartMonitoringAsync();
                }
                else
                {
                    CurrentStatus = "Monitoring disabled";
                }

                // Initialize dashboard
                if (DashboardViewModel.LoadDataCommand.CanExecute(null))
                {
                    await DashboardViewModel.LoadDataCommand.ExecuteAsync(null);
                }

                IsInitialized = true;
                CurrentStatus = IsMonitoring ? "Monitoring active" : "Ready";
            }
            catch (Exception ex)
            {
                CurrentStatus = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task ToggleMonitoringAsync()
        {
            try
            {
                if (IsMonitoring)
                {
                    await StopMonitoringAsync();
                }
                else
                {
                    await StartMonitoringAsync();
                }
            }
            catch (Exception ex)
            {
                CurrentStatus = $"Error: {ex.Message}";
            }
        }

        private async Task StartMonitoringAsync()
        {
            await _windowMonitoringService.StartMonitoringAsync();
            await _settingsService.SetIsMonitoringEnabledAsync(true);
            IsMonitoring = true;
            CurrentStatus = "Monitoring active";
        }

        private async Task StopMonitoringAsync()
        {
            await _windowMonitoringService.StopMonitoringAsync();
            await _settingsService.SetIsMonitoringEnabledAsync(false);
            IsMonitoring = false;
            CurrentStatus = "Monitoring stopped";
        }

        private async void OnWindowChanged(object? sender, WindowChangedEventArgs e)
        {
            try
            {
                // Update current status
                CurrentStatus = $"Active: {e.CurrentWindow.ProcessName}";

                // Handle application tracking
                var application = await _dataService.GetOrCreateApplicationAsync(
                    e.CurrentWindow.ProcessName,
                    e.CurrentWindow.ExecutablePath);

                // Start new usage session
                await _dataService.StartUsageSessionAsync(application.Id, e.CurrentWindow.WindowTitle);

                // Refresh dashboard data
                if (DashboardViewModel.RefreshCurrentDataCommand.CanExecute(null))
                {
                    await DashboardViewModel.RefreshCurrentDataCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                CurrentStatus = $"Error tracking: {ex.Message}";
            }
        }
    }
}
