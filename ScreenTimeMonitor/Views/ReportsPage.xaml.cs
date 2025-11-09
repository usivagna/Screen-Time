using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ScreenTimeMonitor.Models;
using ScreenTimeMonitor.Services;
using System;

namespace ScreenTimeMonitor.Views
{
    public sealed partial class ReportsPage : Page
    {
        private readonly IReportsService _reportsService;
        private ReportPeriodType _currentPeriod = ReportPeriodType.Daily;
        private DateTime _currentDate = DateTime.Today;

        public ReportsPage()
        {
            this.InitializeComponent();
            
            // Get service from DI container
            _reportsService = (IReportsService)App.Host!.Services.GetService(typeof(IReportsService))!;
            
            this.Loaded += ReportsPage_Loaded;
        }

        private async void ReportsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadReportAsync();
        }

        private async void ShowDailyReport_Click(object sender, RoutedEventArgs e)
        {
            _currentPeriod = ReportPeriodType.Daily;
            _currentDate = DateTime.Today;
            DailyReportBtn.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
            WeeklyReportBtn.ClearValue(Button.StyleProperty);
            MonthlyReportBtn.ClearValue(Button.StyleProperty);
            await LoadReportAsync();
        }

        private async void ShowWeeklyReport_Click(object sender, RoutedEventArgs e)
        {
            _currentPeriod = ReportPeriodType.Weekly;
            WeeklyReportBtn.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
            DailyReportBtn.ClearValue(Button.StyleProperty);
            MonthlyReportBtn.ClearValue(Button.StyleProperty);
            await LoadReportAsync();
        }

        private async void ShowMonthlyReport_Click(object sender, RoutedEventArgs e)
        {
            _currentPeriod = ReportPeriodType.Monthly;
            MonthlyReportBtn.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
            DailyReportBtn.ClearValue(Button.StyleProperty);
            WeeklyReportBtn.ClearValue(Button.StyleProperty);
            await LoadReportAsync();
        }

        private async void PreviousPeriod_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentPeriod)
            {
                case ReportPeriodType.Daily:
                    _currentDate = _currentDate.AddDays(-1);
                    break;
                case ReportPeriodType.Weekly:
                    _currentDate = _currentDate.AddDays(-7);
                    break;
                case ReportPeriodType.Monthly:
                    _currentDate = _currentDate.AddMonths(-1);
                    break;
            }
            await LoadReportAsync();
        }

        private async void NextPeriod_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentPeriod)
            {
                case ReportPeriodType.Daily:
                    _currentDate = _currentDate.AddDays(1);
                    break;
                case ReportPeriodType.Weekly:
                    _currentDate = _currentDate.AddDays(7);
                    break;
                case ReportPeriodType.Monthly:
                    _currentDate = _currentDate.AddMonths(1);
                    break;
            }
            await LoadReportAsync();
        }

        private async System.Threading.Tasks.Task LoadReportAsync()
        {
            try
            {
                switch (_currentPeriod)
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
        }

        private async System.Threading.Tasks.Task LoadDailyReportAsync()
        {
            var report = await _reportsService.GetDailyReportAsync(_currentDate);
            
            PeriodTitle.Text = _currentDate.ToString("dddd, MMMM dd, yyyy");
            TotalTimeText.Text = FormatTimeSpan(report.TotalScreenTime);
            AverageTimeText.Text = FormatTimeSpan(report.TotalScreenTime);
            AppsUsedText.Text = report.ApplicationsUsed.ToString();
            ProductivityText.Text = report.ProductivityScore > 0 ? $"{report.ProductivityScore:F1}/10" : "N/A";
        }

        private async System.Threading.Tasks.Task LoadWeeklyReportAsync()
        {
            var startOfWeek = GetStartOfWeek(_currentDate);
            var report = await _reportsService.GetWeeklyReportAsync(startOfWeek);
            
            PeriodTitle.Text = $"{report.StartDate:MMM dd} - {report.EndDate:MMM dd, yyyy}";
            TotalTimeText.Text = FormatTimeSpan(report.TotalScreenTime);
            AverageTimeText.Text = FormatTimeSpan(report.AverageDailyScreenTime);
            AppsUsedText.Text = report.TopApplications.Count.ToString();
            ProductivityText.Text = "N/A";
        }

        private async System.Threading.Tasks.Task LoadMonthlyReportAsync()
        {
            var report = await _reportsService.GetMonthlyReportAsync(_currentDate.Year, _currentDate.Month);
            
            PeriodTitle.Text = _currentDate.ToString("MMMM yyyy");
            TotalTimeText.Text = FormatTimeSpan(report.TotalScreenTime);
            AverageTimeText.Text = FormatTimeSpan(report.AverageDailyScreenTime);
            AppsUsedText.Text = report.TopApplications.Count.ToString();
            
            if (report.TrendPercentage != 0)
            {
                var trend = report.TrendPercentage > 0 ? "↑" : "↓";
                ProductivityText.Text = $"{trend} {Math.Abs(report.TrendPercentage):F0}%";
            }
            else
            {
                ProductivityText.Text = "N/A";
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
}