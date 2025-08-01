using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenTimeMonitor.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.ViewModels
{
    /// <summary>
    /// ViewModel for the dashboard view showing current day statistics
    /// </summary>
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private TimeSpan _totalScreenTimeToday;

        [ObservableProperty]
        private string _totalScreenTimeTodayFormatted = "0h 0m";

        [ObservableProperty]
        private string _mostUsedAppToday = "None";

        [ObservableProperty]
        private string _currentlyActiveApp = "None";

        [ObservableProperty]
        private ObservableCollection<AppUsageViewModel> _topAppsToday = new();

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        [ObservableProperty]
        private bool _isLoading;

        public DashboardViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            await LoadDataForDateAsync(SelectedDate);
        }

        [RelayCommand]
        private async Task RefreshCurrentDataAsync()
        {
            if (!IsLoading)
            {
                await LoadDataForDateAsync(DateTime.Today);
            }
        }

        [RelayCommand]
        private async Task SelectDateAsync(DateTime date)
        {
            SelectedDate = date;
            await LoadDataForDateAsync(date);
        }

        private async Task LoadDataForDateAsync(DateTime date)
        {
            try
            {
                IsLoading = true;

                // Get total screen time for the selected date
                TotalScreenTimeToday = await _dataService.GetTotalScreenTimeAsync(date);
                TotalScreenTimeTodayFormatted = FormatTimeSpan(TotalScreenTimeToday);

                // Get top applications for the selected date
                var topApps = await _dataService.GetTopApplicationsAsync(date, 5);
                
                TopAppsToday.Clear();
                foreach (var (appName, duration) in topApps)
                {
                    TopAppsToday.Add(new AppUsageViewModel
                    {
                        ApplicationName = appName,
                        Duration = duration,
                        DurationFormatted = FormatTimeSpan(duration),
                        Percentage = TotalScreenTimeToday.TotalSeconds > 0 
                            ? (duration.TotalSeconds / TotalScreenTimeToday.TotalSeconds) * 100 
                            : 0
                    });
                }

                // Set most used app
                MostUsedAppToday = topApps.FirstOrDefault().AppName ?? "None";

                // Get currently active app (only for today)
                if (date.Date == DateTime.Today)
                {
                    var activeSession = await _dataService.GetActiveUsageSessionAsync();
                    CurrentlyActiveApp = activeSession?.Application?.Name ?? "None";
                }
                else
                {
                    CurrentlyActiveApp = "N/A";
                }
            }
            catch (Exception ex)
            {
                // Handle error - in a production app, you'd want proper error handling
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
            {
                return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m";
            }
            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            }
            return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        }
    }

    /// <summary>
    /// ViewModel representing application usage statistics
    /// </summary>
    public partial class AppUsageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationName = string.Empty;

        [ObservableProperty]
        private TimeSpan _duration;

        [ObservableProperty]
        private string _durationFormatted = string.Empty;

        [ObservableProperty]
        private double _percentage;

        [ObservableProperty]
        private string _category = "Uncategorized";
    }
}
