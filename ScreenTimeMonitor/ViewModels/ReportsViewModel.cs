using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenTimeMonitor.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.ViewModels
{
    /// <summary>
    /// ViewModel for the reports view showing historical data and analytics
    /// </summary>
    public partial class ReportsViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today.AddDays(-7);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<DailyReportViewModel> _dailyReports = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _reportTitle = "Last 7 Days";

        public ReportsViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        [RelayCommand]
        private async Task LoadReportsAsync()
        {
            try
            {
                IsLoading = true;
                ReportTitle = $"{StartDate:MMM dd} - {EndDate:MMM dd}";

                var summaries = await _dataService.GetDailySummariesAsync(StartDate, EndDate);
                
                DailyReports.Clear();
                foreach (var summary in summaries)
                {
                    DailyReports.Add(new DailyReportViewModel
                    {
                        Date = summary.Date,
                        TotalScreenTime = summary.TotalScreenTime,
                        MostUsedApp = summary.MostUsedApp ?? "None",
                        ProductivityScore = summary.ProductivityScore,
                        AppsUsedCount = summary.AppsUsedCount
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading reports: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SetDateRangeAsync(string range)
        {
            switch (range.ToLower())
            {
                case "today":
                    StartDate = EndDate = DateTime.Today;
                    break;
                case "week":
                    StartDate = DateTime.Today.AddDays(-7);
                    EndDate = DateTime.Today;
                    break;
                case "month":
                    StartDate = DateTime.Today.AddDays(-30);
                    EndDate = DateTime.Today;
                    break;
            }

            await LoadReportsAsync();
        }
    }

    /// <summary>
    /// ViewModel representing a daily report summary
    /// </summary>
    public partial class DailyReportViewModel : ObservableObject
    {
        [ObservableProperty]
        private DateTime _date;

        [ObservableProperty]
        private TimeSpan _totalScreenTime;

        [ObservableProperty]
        private string _mostUsedApp = string.Empty;

        [ObservableProperty]
        private double _productivityScore;

        [ObservableProperty]
        private int _appsUsedCount;
    }
}
