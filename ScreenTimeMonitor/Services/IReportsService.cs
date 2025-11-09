using ScreenTimeMonitor.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Service for generating usage reports
    /// </summary>
    public interface IReportsService
    {
        /// <summary>
        /// Get daily report for a specific date
        /// </summary>
        Task<DailyReport> GetDailyReportAsync(DateTime date);
        
        /// <summary>
        /// Get weekly report for a specific week
        /// </summary>
        Task<WeeklyReport> GetWeeklyReportAsync(DateTime startDate);
        
        /// <summary>
        /// Get monthly report for a specific month
        /// </summary>
        Task<MonthlyReport> GetMonthlyReportAsync(int year, int month);
        
        /// <summary>
        /// Get comparison between two time periods
        /// </summary>
        Task<PeriodComparison> GetPeriodComparisonAsync(DateTime start1, DateTime end1, DateTime start2, DateTime end2);
    }

    /// <summary>
    /// Daily usage report
    /// </summary>
    public class DailyReport
    {
        public DateTime Date { get; set; }
        public TimeSpan TotalScreenTime { get; set; }
        public List<AppUsageData> TopApplications { get; set; } = new();
        public int ApplicationsUsed { get; set; }
        public double ProductivityScore { get; set; }
    }

    /// <summary>
    /// Weekly usage report
    /// </summary>
    public class WeeklyReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan TotalScreenTime { get; set; }
        public TimeSpan AverageDailyScreenTime { get; set; }
        public List<AppUsageData> TopApplications { get; set; } = new();
        public List<DailyData> DailyBreakdown { get; set; } = new();
        public string MostProductiveDay { get; set; } = string.Empty;
    }

    /// <summary>
    /// Monthly usage report
    /// </summary>
    public class MonthlyReport
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public TimeSpan TotalScreenTime { get; set; }
        public TimeSpan AverageDailyScreenTime { get; set; }
        public List<AppUsageData> TopApplications { get; set; } = new();
        public List<WeeklyData> WeeklyBreakdown { get; set; } = new();
        public double TrendPercentage { get; set; }
    }

    /// <summary>
    /// Period comparison data
    /// </summary>
    public class PeriodComparison
    {
        public TimeSpan Period1Total { get; set; }
        public TimeSpan Period2Total { get; set; }
        public double ChangePercentage { get; set; }
        public string Trend { get; set; } = string.Empty;
    }

    /// <summary>
    /// Application usage data for reports
    /// </summary>
    public class AppUsageData
    {
        public string ApplicationName { get; set; } = string.Empty;
        public TimeSpan TotalTime { get; set; }
        public double Percentage { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Daily data point for charts
    /// </summary>
    public class DailyData
    {
        public DateTime Date { get; set; }
        public TimeSpan ScreenTime { get; set; }
        public int ApplicationsUsed { get; set; }
    }

    /// <summary>
    /// Weekly data point for charts
    /// </summary>
    public class WeeklyData
    {
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan TotalTime { get; set; }
    }
}
