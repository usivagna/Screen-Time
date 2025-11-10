using ScreenTimeMonitor.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Service for generating various usage reports
    /// </summary>
    public class ReportsService : IReportsService
    {
        private readonly IDataService _dataService;
        private readonly ISettingsService _settingsService;

        public ReportsService(IDataService dataService, ISettingsService settingsService)
        {
            _dataService = dataService;
            _settingsService = settingsService;
        }

        public async Task<DailyReport> GetDailyReportAsync(DateTime date)
        {
            var summary = await _dataService.GetOrCreateDailySummaryAsync(date);
            var topApps = await _dataService.GetTopApplicationsAsync(date, 10);
            
            var report = new DailyReport
            {
                Date = date,
                TotalScreenTime = summary.TotalScreenTime,
                ApplicationsUsed = summary.AppsUsedCount,
                ProductivityScore = summary.ProductivityScore
            };

            var totalSeconds = summary.TotalScreenTimeSeconds;
            foreach (var (appName, duration) in topApps)
            {
                var category = await _settingsService.GetApplicationCategoryAsync(appName);
                report.TopApplications.Add(new AppUsageData
                {
                    ApplicationName = appName,
                    TotalTime = duration,
                    Percentage = totalSeconds > 0 ? (duration.TotalSeconds / totalSeconds) * 100 : 0,
                    Category = category
                });
            }

            return report;
        }

        public async Task<WeeklyReport> GetWeeklyReportAsync(DateTime startDate)
        {
            var endDate = startDate.AddDays(7);
            var summaries = await _dataService.GetDailySummariesAsync(startDate, endDate);
            
            var report = new WeeklyReport
            {
                StartDate = startDate,
                EndDate = endDate
            };

            // Calculate totals
            var totalSeconds = 0;
            foreach (var summary in summaries)
            {
                totalSeconds += summary.TotalScreenTimeSeconds;
                report.DailyBreakdown.Add(new DailyData
                {
                    Date = summary.Date,
                    ScreenTime = summary.TotalScreenTime,
                    ApplicationsUsed = summary.AppsUsedCount
                });
            }

            report.TotalScreenTime = TimeSpan.FromSeconds(totalSeconds);
            report.AverageDailyScreenTime = summaries.Count > 0 
                ? TimeSpan.FromSeconds(totalSeconds / summaries.Count) 
                : TimeSpan.Zero;

            // Find most productive day
            var mostProductive = summaries.OrderByDescending(s => s.ProductivityScore).FirstOrDefault();
            report.MostProductiveDay = mostProductive?.Date.ToString("dddd, MMM dd") ?? "N/A";

            // Get top applications for the week
            var appUsages = new Dictionary<string, TimeSpan>();
            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                var dayUsage = await _dataService.GetApplicationUsageAsync(date);
                foreach (var (app, time) in dayUsage)
                {
                    if (!appUsages.ContainsKey(app))
                        appUsages[app] = TimeSpan.Zero;
                    appUsages[app] = appUsages[app].Add(time);
                }
            }

            var topApps = appUsages.OrderByDescending(x => x.Value).Take(10);
            foreach (var (app, time) in topApps)
            {
                var category = await _settingsService.GetApplicationCategoryAsync(app);
                report.TopApplications.Add(new AppUsageData
                {
                    ApplicationName = app,
                    TotalTime = time,
                    Percentage = totalSeconds > 0 ? (time.TotalSeconds / totalSeconds) * 100 : 0,
                    Category = category
                });
            }

            return report;
        }

        public async Task<MonthlyReport> GetMonthlyReportAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);
            var summaries = await _dataService.GetDailySummariesAsync(startDate, endDate);
            
            var report = new MonthlyReport
            {
                Year = year,
                Month = month
            };

            // Calculate totals
            var totalSeconds = 0;
            foreach (var summary in summaries)
            {
                totalSeconds += summary.TotalScreenTimeSeconds;
            }

            report.TotalScreenTime = TimeSpan.FromSeconds(totalSeconds);
            report.AverageDailyScreenTime = summaries.Count > 0 
                ? TimeSpan.FromSeconds(totalSeconds / summaries.Count) 
                : TimeSpan.Zero;

            // Weekly breakdown
            var weeklyData = new Dictionary<int, (DateTime Start, TimeSpan Total)>();
            foreach (var summary in summaries)
            {
                var weekNum = GetWeekOfMonth(summary.Date);
                if (!weeklyData.ContainsKey(weekNum))
                {
                    weeklyData[weekNum] = (GetStartOfWeek(summary.Date), TimeSpan.Zero);
                }
                weeklyData[weekNum] = (weeklyData[weekNum].Start, weeklyData[weekNum].Total.Add(summary.TotalScreenTime));
            }

            foreach (var (weekNum, (start, total)) in weeklyData)
            {
                report.WeeklyBreakdown.Add(new WeeklyData
                {
                    WeekNumber = weekNum,
                    StartDate = start,
                    TotalTime = total
                });
            }

            // Get top applications for the month
            var appUsages = new Dictionary<string, TimeSpan>();
            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                var dayUsage = await _dataService.GetApplicationUsageAsync(date);
                foreach (var (app, time) in dayUsage)
                {
                    if (!appUsages.ContainsKey(app))
                        appUsages[app] = TimeSpan.Zero;
                    appUsages[app] = appUsages[app].Add(time);
                }
            }

            var topApps = appUsages.OrderByDescending(x => x.Value).Take(10);
            foreach (var (app, time) in topApps)
            {
                var category = await _settingsService.GetApplicationCategoryAsync(app);
                report.TopApplications.Add(new AppUsageData
                {
                    ApplicationName = app,
                    TotalTime = time,
                    Percentage = totalSeconds > 0 ? (time.TotalSeconds / totalSeconds) * 100 : 0,
                    Category = category
                });
            }

            // Calculate trend (compare to previous month)
            var prevMonthStart = startDate.AddMonths(-1);
            var prevMonthEnd = startDate;
            var prevSummaries = await _dataService.GetDailySummariesAsync(prevMonthStart, prevMonthEnd);
            var prevTotal = prevSummaries.Sum(s => s.TotalScreenTimeSeconds);
            
            if (prevTotal > 0)
            {
                report.TrendPercentage = ((totalSeconds - prevTotal) / (double)prevTotal) * 100;
            }

            return report;
        }

        public async Task<PeriodComparison> GetPeriodComparisonAsync(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            var period1Time = await GetTotalTimeForPeriodAsync(start1, end1);
            var period2Time = await GetTotalTimeForPeriodAsync(start2, end2);

            var comparison = new PeriodComparison
            {
                Period1Total = period1Time,
                Period2Total = period2Time
            };

            if (period2Time.TotalSeconds > 0)
            {
                comparison.ChangePercentage = ((period1Time.TotalSeconds - period2Time.TotalSeconds) / period2Time.TotalSeconds) * 100;
                comparison.Trend = comparison.ChangePercentage > 0 ? "Increase" : comparison.ChangePercentage < 0 ? "Decrease" : "No Change";
            }

            return comparison;
        }

        private async Task<TimeSpan> GetTotalTimeForPeriodAsync(DateTime start, DateTime end)
        {
            var summaries = await _dataService.GetDailySummariesAsync(start, end);
            var totalSeconds = summaries.Sum(s => s.TotalScreenTimeSeconds);
            return TimeSpan.FromSeconds(totalSeconds);
        }

        private static int GetWeekOfMonth(DateTime date)
        {
            var firstOfMonth = new DateTime(date.Year, date.Month, 1);
            return (date.Day + (int)firstOfMonth.DayOfWeek) / 7 + 1;
        }

        private static DateTime GetStartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
    }
}
