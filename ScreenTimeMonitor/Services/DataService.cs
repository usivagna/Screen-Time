#nullable enable
using Microsoft.EntityFrameworkCore;
using ScreenTimeMonitor.Data;
using ScreenTimeMonitor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace ScreenTimeMonitor.Services
{
    /// <summary>
    /// Service for data access operations using Entity Framework
    /// </summary>
    public class DataService : IDataService
    {
        private readonly AppDbContext _context;

        public DataService(AppDbContext context)
        {
            _context = context;
        }

        public async Task InitializeDatabaseAsync()
        {
            await _context.EnsureDatabaseCreatedAsync();
        }

        public Task<bool> DatabaseExistsAsync()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder.Path;
                var dbPath = Path.Combine(localFolder, "ScreenTimeMonitor.db");
                return Task.FromResult(File.Exists(dbPath));
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        #region Application Methods

        public async Task<Application> GetOrCreateApplicationAsync(string name, string executablePath)
        {
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.Name == name);

            if (application == null)
            {
                application = new Application
                {
                    Name = name,
                    ExecutablePath = executablePath,
                    Category = DetermineApplicationCategory(name, executablePath),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Applications.Add(application);
                await _context.SaveChangesAsync();
            }
            else if (string.IsNullOrEmpty(application.ExecutablePath) && !string.IsNullOrEmpty(executablePath))
            {
                // Update executable path if it was missing
                application.ExecutablePath = executablePath;
                await _context.SaveChangesAsync();
            }

            return application;
        }

        public async Task<List<Application>> GetApplicationsAsync()
        {
            return await _context.Applications
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task UpdateApplicationCategoryAsync(int applicationId, string category)
        {
            var application = await _context.Applications.FindAsync(applicationId);
            if (application != null)
            {
                application.Category = category;
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Usage Session Methods

        public async Task<UsageSession> StartUsageSessionAsync(int applicationId, string windowTitle)
        {
            // End any existing active session
            var activeSession = await GetActiveUsageSessionAsync();
            if (activeSession != null)
            {
                await EndUsageSessionAsync(activeSession);
            }

            var session = new UsageSession
            {
                ApplicationId = applicationId,
                StartTime = DateTime.UtcNow,
                Date = DateTime.Today,
                WindowTitle = windowTitle,
                IsActive = true
            };

            _context.UsageSessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task EndUsageSessionAsync(UsageSession session)
        {
            if (session.IsActive)
            {
                session.EndTime = DateTime.UtcNow;
                session.DurationSeconds = (int)(session.EndTime.Value - session.StartTime).TotalSeconds;
                session.IsActive = false;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<UsageSession>> GetUsageSessionsAsync(DateTime date)
        {
            return await _context.UsageSessions
                .AsNoTracking()
                .Include(s => s.Application)
                .Where(s => s.Date.Date == date.Date)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<UsageSession>> GetUsageSessionsAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.UsageSessions
                .AsNoTracking()
                .Include(s => s.Application)
                .Where(s => s.Date.Date >= startDate.Date && s.Date.Date <= endDate.Date)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<UsageSession?> GetActiveUsageSessionAsync()
        {
            return await _context.UsageSessions
                .Include(s => s.Application)
                .FirstOrDefaultAsync(s => s.IsActive);
        }

        #endregion

        #region Daily Summary Methods

        public async Task<DailySummary> GetOrCreateDailySummaryAsync(DateTime date)
        {
            var summary = await _context.DailySummaries
                .FirstOrDefaultAsync(s => s.Date.Date == date.Date);

            if (summary == null)
            {
                summary = new DailySummary
                {
                    Date = date.Date,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.DailySummaries.Add(summary);
                await _context.SaveChangesAsync();
            }

            return summary;
        }

        public async Task UpdateDailySummaryAsync(DailySummary summary)
        {
            summary.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<List<DailySummary>> GetDailySummariesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.DailySummaries
                .AsNoTracking()
                .Where(s => s.Date.Date >= startDate.Date && s.Date.Date <= endDate.Date)
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        #endregion

        #region Statistics Methods

        public async Task<TimeSpan> GetTotalScreenTimeAsync(DateTime date)
        {
            var totalSeconds = await _context.UsageSessions
                .AsNoTracking()
                .Where(s => s.Date.Date == date.Date && !s.IsActive)
                .SumAsync(s => s.DurationSeconds);

            return TimeSpan.FromSeconds(totalSeconds);
        }

        public async Task<Dictionary<string, TimeSpan>> GetApplicationUsageAsync(DateTime date)
        {
            var usage = await _context.UsageSessions
                .AsNoTracking()
                .Include(s => s.Application)
                .Where(s => s.Date.Date == date.Date && !s.IsActive)
                .GroupBy(s => s.Application.Name)
                .Select(g => new { AppName = g.Key, TotalSeconds = g.Sum(s => s.DurationSeconds) })
                .ToListAsync();

            return usage.ToDictionary(
                u => u.AppName,
                u => TimeSpan.FromSeconds(u.TotalSeconds)
            );
        }

        public async Task<List<(string AppName, TimeSpan Duration)>> GetTopApplicationsAsync(DateTime date, int count = 5)
        {
            var topApps = await _context.UsageSessions
                .AsNoTracking()
                .Include(s => s.Application)
                .Where(s => s.Date.Date == date.Date && !s.IsActive)
                .GroupBy(s => s.Application.Name)
                .Select(g => new { AppName = g.Key, TotalSeconds = g.Sum(s => s.DurationSeconds) })
                .OrderByDescending(g => g.TotalSeconds)
                .Take(count)
                .ToListAsync();

            return topApps.Select(a => (a.AppName, TimeSpan.FromSeconds(a.TotalSeconds))).ToList();
        }

        #endregion

        #region Private Helper Methods

        private static string DetermineApplicationCategory(string appName, string executablePath)
        {
            // Use ReadOnlySpan for more efficient string comparisons
            var nameSpan = appName.AsSpan();
            var pathSpan = executablePath.AsSpan();

            // Productivity apps
            if (IsProductivityApp(nameSpan, pathSpan))
                return "Productivity";

            // Development tools
            if (IsDevelopmentApp(nameSpan, pathSpan))
                return "Development";

            // Web browsers
            if (IsBrowserApp(nameSpan, pathSpan))
                return "Web Browsing";

            // Entertainment
            if (IsEntertainmentApp(nameSpan, pathSpan))
                return "Entertainment";

            // Communication
            if (IsCommunicationApp(nameSpan, pathSpan))
                return "Communication";

            // Games
            if (IsGameApp(nameSpan, pathSpan))
                return "Gaming";

            return "Uncategorized";
        }

        private static bool IsProductivityApp(ReadOnlySpan<char> name, ReadOnlySpan<char> path)
        {
            ReadOnlySpan<string> productivityApps = new[] { "word", "excel", "powerpoint", "outlook", "onenote", "notepad", "calculator" };
            foreach (var app in productivityApps)
            {
                if (name.Contains(app, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static bool IsDevelopmentApp(ReadOnlySpan<char> name, ReadOnlySpan<char> path)
        {
            ReadOnlySpan<string> devApps = new[] { "visual studio", "vscode", "code", "devenv", "git", "cmd", "powershell", "terminal" };
            foreach (var app in devApps)
            {
                if (name.Contains(app, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static bool IsBrowserApp(ReadOnlySpan<char> name, ReadOnlySpan<char> path)
        {
            ReadOnlySpan<string> browsers = new[] { "chrome", "firefox", "edge", "safari", "opera", "brave" };
            foreach (var browser in browsers)
            {
                if (name.Contains(browser, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static bool IsEntertainmentApp(ReadOnlySpan<char> name, ReadOnlySpan<char> path)
        {
            ReadOnlySpan<string> entertainmentApps = new[] { "spotify", "netflix", "youtube", "vlc", "media player", "photos" };
            foreach (var app in entertainmentApps)
            {
                if (name.Contains(app, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static bool IsCommunicationApp(ReadOnlySpan<char> name, ReadOnlySpan<char> path)
        {
            ReadOnlySpan<string> commApps = new[] { "teams", "slack", "discord", "skype", "zoom", "whatsapp", "telegram" };
            foreach (var app in commApps)
            {
                if (name.Contains(app, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static bool IsGameApp(ReadOnlySpan<char> name, ReadOnlySpan<char> path)
        {
            ReadOnlySpan<string> gameIndicators = new[] { "steam", "epic", "game", "blizzard", "origin", "uplay" };
            foreach (var indicator in gameIndicators)
            {
                if (name.Contains(indicator, StringComparison.OrdinalIgnoreCase) || 
                    path.Contains(indicator, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        #endregion
    }
}
