using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenTimeMonitor.Models;
using ScreenTimeMonitor.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.ViewModels
{
    /// <summary>
    /// ViewModel for the Reports page with Daily/Weekly/Monthly analytics
    /// </summary>
    public partial class ReportsViewModel : ObservableObject
    {
        private readonly IReportsService _reportsService;

        [ObservableProperty]
        private ReportPeriodType _selectedPeriod = ReportPeriodType.Daily;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _totalScreenTime = "0h 0m";

        [ObservableProperty]
        private string _averageScreenTime = "0h 0m";

        [ObservableProperty]
        private int _applicationsUsed;

        [ObservableProperty]
        private double _productivityScore;

        [ObservableProperty]
        private string _periodTitle = "Today";

        [ObservableProperty]
        private ObservableCollection<AppUsageViewModel> _topApplications = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> _chartData = new();

        public ReportsViewModel(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [RelayCommand]
        private async Task LoadReportAsync()
        {
            try
            {
                IsLoading = true;

                switch (SelectedPeriod)
                {
                    case ReportPeriodType.Daily:
                        await LoadDailyReportAsync();
                        break;
                    case ReportPeriodType.Weekly:
                        await LoadWeeklyReportAsync();
                        break;
                    case ReportPeriodType.Monthly:
                        await LoadMonthlyReportAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading report: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ChangePeriodAsync(string period)
        {
            if (Enum.TryParse<ReportPeriodType>(period, out var periodType))
            {
                SelectedPeriod = periodType;
                await LoadReportAsync();
            }
        }

        [RelayCommand]
        private async Task NavigateDateAsync(string direction)
        {
            switch (SelectedPeriod)
            {
                case ReportPeriodType.Daily:
                    SelectedDate = direction == "next" ? SelectedDate.AddDays(1) : SelectedDate.AddDays(-1);
                    break;
                case ReportPeriodType.Weekly:
                    SelectedDate = direction == "next" ? SelectedDate.AddDays(7) : SelectedDate.AddDays(-7);
                    break;
                case ReportPeriodType.Monthly:
                    SelectedDate = direction == "next" ? SelectedDate.AddMonths(1) : SelectedDate.AddMonths(-1);
                    break;
            }
            await LoadReportAsync();
        }

        private async Task LoadDailyReportAsync()
        {
            var report = await _reportsService.GetDailyReportAsync(SelectedDate);
            
            PeriodTitle = SelectedDate.ToString("dddd, MMMM dd, yyyy");
            TotalScreenTime = FormatTimeSpan(report.TotalScreenTime);
            AverageScreenTime = FormatTimeSpan(report.TotalScreenTime); // Same as total for daily
            ApplicationsUsed = report.ApplicationsUsed;
            ProductivityScore = report.ProductivityScore;

            TopApplications.Clear();
            foreach (var app in report.TopApplications)
            {
                TopApplications.Add(new AppUsageViewModel
                {
                    ApplicationName = app.ApplicationName,
                    Duration = app.TotalTime,
                    DurationFormatted = FormatTimeSpan(app.TotalTime),
                    Percentage = app.Percentage,
                    Category = app.Category
                });
            }

            // Simple chart data for daily view (single bar)
            ChartData.Clear();
            ChartData.Add(new ChartDataPoint
            {
                Label = "Today",
                Value = report.TotalScreenTime.TotalHours
            });
        }

        private async Task LoadWeeklyReportAsync()
        {
            var startOfWeek = GetStartOfWeek(SelectedDate);
            var report = await _reportsService.GetWeeklyReportAsync(startOfWeek);
            
            PeriodTitle = $"{report.StartDate:MMM dd} - {report.EndDate:MMM dd, yyyy}";
            TotalScreenTime = FormatTimeSpan(report.TotalScreenTime);
            AverageScreenTime = FormatTimeSpan(report.AverageDailyScreenTime);
            ApplicationsUsed = report.TopApplications.Count;

            TopApplications.Clear();
            foreach (var app in report.TopApplications)
            {
                TopApplications.Add(new AppUsageViewModel
                {
                    ApplicationName = app.ApplicationName,
                    Duration = app.TotalTime,
                    DurationFormatted = FormatTimeSpan(app.TotalTime),
                    Percentage = app.Percentage,
                    Category = app.Category
                });
            }

            // Chart data for weekly view (7 days)
            ChartData.Clear();
            foreach (var day in report.DailyBreakdown)
            {
                ChartData.Add(new ChartDataPoint
                {
                    Label = day.Date.ToString("ddd"),
                    Value = day.ScreenTime.TotalHours
                });
            }
        }

        private async Task LoadMonthlyReportAsync()
        {
            var report = await _reportsService.GetMonthlyReportAsync(SelectedDate.Year, SelectedDate.Month);
            
            PeriodTitle = SelectedDate.ToString("MMMM yyyy");
            TotalScreenTime = FormatTimeSpan(report.TotalScreenTime);
            AverageScreenTime = FormatTimeSpan(report.AverageDailyScreenTime);
            ApplicationsUsed = report.TopApplications.Count;

            TopApplications.Clear();
            foreach (var app in report.TopApplications)
            {
                TopApplications.Add(new AppUsageViewModel
                {
                    ApplicationName = app.ApplicationName,
                    Duration = app.TotalTime,
                    DurationFormatted = FormatTimeSpan(app.TotalTime),
                    Percentage = app.Percentage,
                    Category = app.Category
                });
            }

            // Chart data for monthly view (weeks)
            ChartData.Clear();
            foreach (var week in report.WeeklyBreakdown)
            {
                ChartData.Add(new ChartDataPoint
                {
                    Label = $"Week {week.WeekNumber}",
                    Value = week.TotalTime.TotalHours
                });
            }
        }

        private static DateTime GetStartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
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
            return $"{timeSpan.Minutes}m";
        }
    }

    /// <summary>
    /// Chart data point for visualization
    /// </summary>
    public partial class ChartDataPoint : ObservableObject
    {
        [ObservableProperty]
        private string _label = string.Empty;

        [ObservableProperty]
        private double _value;
    }
}
