using ScreenTimeMonitor.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Interface for data access operations
    /// </summary>
    public interface IDataService
    {
        // Application methods
        Task<Application> GetOrCreateApplicationAsync(string name, string executablePath);
        Task<List<Application>> GetApplicationsAsync();
        Task UpdateApplicationCategoryAsync(int applicationId, string category);

        // Usage session methods
        Task<UsageSession> StartUsageSessionAsync(int applicationId, string windowTitle);
        Task EndUsageSessionAsync(UsageSession session);
        Task<List<UsageSession>> GetUsageSessionsAsync(DateTime date);
        Task<List<UsageSession>> GetUsageSessionsAsync(DateTime startDate, DateTime endDate);
        Task<UsageSession?> GetActiveUsageSessionAsync();

        // Daily summary methods
        Task<DailySummary> GetOrCreateDailySummaryAsync(DateTime date);
        Task UpdateDailySummaryAsync(DailySummary summary);
        Task<List<DailySummary>> GetDailySummariesAsync(DateTime startDate, DateTime endDate);

        // Statistics methods
        Task<TimeSpan> GetTotalScreenTimeAsync(DateTime date);
        Task<Dictionary<string, TimeSpan>> GetApplicationUsageAsync(DateTime date);
        Task<List<(string AppName, TimeSpan Duration)>> GetTopApplicationsAsync(DateTime date, int count = 5);
        
        // Database management
        Task InitializeDatabaseAsync();
        Task<bool> DatabaseExistsAsync();
    }
}
