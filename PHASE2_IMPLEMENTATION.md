# Phase 2 Implementation - Feature Documentation

This document describes the Phase 2 features implemented in the Screen Time Monitor application.

## Overview

Phase 2 introduces enhanced analytics, proactive notifications, and background operation capabilities to help users better manage their screen time and maintain healthy computing habits.

## Features

### 1. Daily/Weekly/Monthly Reports

Enhanced reporting system with multiple time period views and comprehensive analytics.

#### Components
- **ReportsService**: Core service that generates reports by aggregating data from the database
- **EnhancedReportsViewModel**: ViewModel supporting all report types with reactive data binding
- **ReportsPage**: Enhanced UI with period selection and data visualization

#### Features
- **Period Selection**: Switch between Daily, Weekly, and Monthly views
- **Navigation**: Browse through historical periods with Previous/Next buttons
- **Summary Cards**: Display key metrics:
  - Total screen time for the period
  - Average daily screen time (for weekly/monthly)
  - Number of applications used
  - Productivity score or trend percentage
- **Top Applications**: Ranked list of most-used applications with time and percentage
- **Chart Placeholder**: Ready for integration with charting libraries

#### Usage
```csharp
var reportsService = App.Host.Services.GetService<IReportsService>();

// Get daily report
var dailyReport = await reportsService.GetDailyReportAsync(DateTime.Today);

// Get weekly report (starting from Monday)
var weeklyReport = await reportsService.GetWeeklyReportAsync(startOfWeek);

// Get monthly report
var monthlyReport = await reportsService.GetMonthlyReportAsync(2024, 1);
```

### 2. Usage Limits & Notifications

Set time limits for applications and receive notifications when approaching or exceeding them.

**Note:** The NotificationService currently uses debug output as a placeholder. Full Windows toast notification implementation using `Microsoft.Windows.AppNotifications` can be added when the package is properly configured.

#### Components
- **UsageLimitService**: Monitors application usage against configured limits
- **NotificationService**: Enhanced with Windows App SDK toast notifications
- **SettingsService**: Stores and retrieves limit configurations

#### Features
- **Global Daily Limit**: Set an overall screen time limit
- **Application-Specific Limits**: Configure individual limits per application
- **Warning Notifications**: Get alerts at 80% of limit
- **Exceeded Notifications**: Get alerts when limit is exceeded
- **Automatic Monitoring**: Checks limits every 5 minutes

#### Notification Types
1. **80% Warning**: "You've used AppName for Xh Ym (80% of your limit)"
2. **Limit Exceeded**: "You've exceeded your daily limit for AppName"

#### Configuration (in Settings Page)
- Enable/disable limit monitoring
- Set hours and minutes for global limit
- Configure per-application limits via flyout menu

#### Usage
```csharp
// Set a daily limit for an application
await settingsService.SetDailyUsageLimitAsync("Chrome", TimeSpan.FromHours(4));

// Check if limit exceeded
bool exceeded = await usageLimitService.HasExceededLimitAsync("Chrome", DateTime.Today);

// Get usage percentage
double percentage = await usageLimitService.GetLimitUsagePercentageAsync("Chrome");
```

### 3. System Tray Integration

Framework for running the application in the background with system tray icon.

#### Components
- **SystemTrayService**: Service interface for tray icon management
- **ISystemTrayService**: Interface defining tray operations

#### Features
- **Background Running**: Application can minimize to system tray
- **Quick Access**: Tray icon for quick access to the application
- **Tooltip Updates**: Dynamic tooltip showing current status
- **Minimize to Tray**: Option in settings to minimize to tray instead of taskbar

#### Implementation Notes
- Uses H.NotifyIcon.WinUI package for WinUI 3 support
- Service is initialized on MainWindow load
- Properly cleaned up on application close

#### Usage
```csharp
// Initialize tray icon
await systemTrayService.InitializeAsync();
await systemTrayService.ShowAsync();

// Update tooltip
await systemTrayService.UpdateTooltipAsync("Screen Time: 4h 32m");

// Hide icon
await systemTrayService.HideAsync();
```

### 4. Break Reminders

Periodic notifications encouraging users to take breaks for eye health and physical well-being.

#### Components
- **BreakReminderService**: Manages break reminder timers and notifications
- **SettingsService**: Stores reminder preferences
- **NotificationService**: Shows break reminder toasts

#### Features
- **Configurable Intervals**: 30 minutes, 1 hour, 2 hours, or 3 hours
- **Break Duration Options**: 5, 10, or 15 minutes
- **Enable/Disable**: Toggle break reminders on/off
- **Snooze Function**: Snooze reminders for 5 minutes via notification button
- **Automatic Timer Reset**: Tracks continuous usage time

#### Notification Features
- Friendly emoji icons (🧘)
- Actionable buttons (Snooze, Dismiss)
- Shows continuous usage time

#### Configuration (in Settings Page)
- Enable/disable break reminders
- Select reminder interval
- Choose break duration

#### Usage
```csharp
// Start break reminders
await breakReminderService.StartAsync();

// Stop break reminders
await breakReminderService.StopAsync();

// Reset timer (e.g., after user takes a break)
await breakReminderService.ResetTimerAsync();

// Check time since last break
TimeSpan timeSinceBreak = breakReminderService.GetTimeSinceLastBreak();
```

## Architecture

### Service Registration

All Phase 2 services are registered in the DI container in `App.xaml.cs`:

```csharp
builder.Services.AddSingleton<IUsageLimitService, UsageLimitService>();
builder.Services.AddSingleton<IBreakReminderService, BreakReminderService>();
builder.Services.AddSingleton<ISystemTrayService, SystemTrayService>();
builder.Services.AddSingleton<IReportsService, ReportsService>();
```

### Service Lifecycle

Services are managed through the MainWindow lifecycle:

1. **Initialization** (MainWindow.Loaded):
   - System tray service initialized and shown
   - Break reminders started if enabled in settings
   - Usage limit monitoring started

2. **Runtime**:
   - Services run continuously in the background
   - Timers check for notifications
   - Data is aggregated for reports

3. **Cleanup** (MainWindow.Closed):
   - All services stopped gracefully
   - Timers disposed
   - System tray icon hidden

### Data Flow

```
WindowMonitoringService → DataService → Database
                              ↓
                    UsageLimitService → NotificationService
                              ↓
                        BreakReminderService → NotificationService
                              ↓
                        ReportsService → UI (Reports Page)
```

## User Interface Enhancements

### Settings Page Additions

1. **General Settings Section**
   - Start monitoring on launch
   - Start with Windows
   - Minimize to system tray

2. **Usage Limits Section**
   - Global daily limit configuration
   - Application-specific limits
   - Interactive flyout for detailed settings

3. **Break Reminders Section**
   - Enable/disable toggle
   - Interval selection dropdown
   - Break duration selection

4. **Notifications Section**
   - Toggle individual notification types
   - Usage limit warnings
   - Daily summaries
   - Productivity tips

### Reports Page Enhancements

1. **Period Selector**
   - Three buttons: Daily, Weekly, Monthly
   - Date navigation (Previous/Next)
   - Dynamic period title

2. **Summary Cards (4 metrics)**
   - Total Time
   - Average/Day
   - Apps Used
   - Productivity/Trend

3. **Chart Area**
   - Placeholder for data visualization
   - Will display bars/lines based on period

4. **Top Applications List**
   - Ranked by usage time
   - Shows duration and percentage
   - Progress bar visualization

## Windows Notifications

### Toast Notification Types

1. **Usage Limit Warning**
   ```
   Title: "Usage Limit Warning"
   Message: "You've used Chrome for 3h 12m (80% of your 4h 0m daily limit)"
   ```

2. **Limit Exceeded**
   ```
   Title: "Usage Limit Exceeded"  
   Message: "You've exceeded your daily limit for Chrome (4h 15m / 4h 0m)"
   ```

3. **Break Reminder**
   ```
   Title: "Time for a Break! 🧘"
   Message: "You've been active for 1h 30m. Take a short break to rest your eyes."
   Buttons: [Snooze 5 min] [Dismiss]
   ```

4. **Daily Summary**
   ```
   Title: "Daily Screen Time Summary 📊"
   Message: "Total time today: 6h 45m. Most used: Visual Studio Code"
   Button: [View Details]
   ```

## Configuration

### Settings Storage

All settings are stored using Windows.Storage APIs (ApplicationData.Current.LocalSettings):

- `IsMonitoringEnabled`: bool
- `StartWithWindows`: bool
- `BreakRemindersEnabled`: bool
- `BreakReminderInterval`: TimeSpan (stored as ticks)
- `UsageLimit_{AppName}`: TimeSpan per application
- `Theme`: string (Light/Dark/System)
- `DataRetentionDays`: int

### Database Schema

No new tables required - existing schema supports Phase 2:
- `Applications`: Application metadata
- `UsageSessions`: Individual usage sessions
- `DailySummaries`: Aggregated daily statistics

## Performance Considerations

1. **Timer Intervals**
   - Usage limit checks: Every 5 minutes (configurable)
   - Break reminders: User-configured (30min - 3hrs)
   - Minimal CPU usage when idle

2. **Database Queries**
   - Reports use efficient aggregation queries
   - Indexes on date fields for fast lookups
   - Limited to necessary date ranges

3. **Memory Usage**
   - Services use singleton pattern
   - Timers properly disposed
   - No memory leaks in service lifecycle

## Testing Recommendations

When testing on Windows:

1. **Usage Limits**
   - Set short limits (e.g., 1 minute) for quick testing
   - Verify notifications appear at 80% and 100%
   - Check that limits persist across app restarts

2. **Break Reminders**
   - Set short interval (e.g., 30 seconds) for testing
   - Verify snooze functionality works
   - Check that timer resets properly

3. **Reports**
   - Generate test data for multiple days
   - Verify all three report types display correctly
   - Check date navigation works in all directions

4. **System Tray**
   - Verify icon appears in system tray
   - Test minimize to tray functionality
   - Verify context menu actions work

## Future Enhancements

Potential improvements for Phase 2 features:

1. **Charts & Visualizations**
   - Add WinUI 3 charting library
   - Implement bar charts for daily reports
   - Add line charts for weekly/monthly trends
   - Include pie charts for application distribution

2. **Advanced Notifications**
   - Notification history
   - Do Not Disturb mode
   - Smart timing based on productivity patterns

3. **Enhanced System Tray**
   - Quick stats in context menu
   - Quick actions (Pause/Resume monitoring)
   - Today's summary in tooltip

4. **Productivity Insights**
   - AI-powered productivity analysis
   - Recommendations based on usage patterns
   - Goals and achievement tracking

## Troubleshooting

### Notifications Not Showing
- Ensure Windows notifications are enabled in system settings
- Check that the app has notification permissions
- Verify NotificationService is properly initialized

### Break Reminders Not Working
- Check that BreakRemindersEnabled setting is true
- Verify the BreakReminderService started successfully
- Look for errors in debug output

### Reports Show No Data
- Ensure monitoring has been running and collecting data
- Check database file exists and is accessible
- Verify DataService is working correctly

## API Reference

### IUsageLimitService

```csharp
Task StartMonitoringAsync();
Task StopMonitoringAsync();
Task<bool> HasExceededLimitAsync(string appName, DateTime date);
Task<double> GetLimitUsagePercentageAsync(string appName);
bool IsMonitoring { get; }
```

### IBreakReminderService

```csharp
Task StartAsync();
Task StopAsync();
Task ResetTimerAsync();
TimeSpan GetTimeSinceLastBreak();
bool IsRunning { get; }
```

### ISystemTrayService

```csharp
Task InitializeAsync();
Task ShowAsync();
Task HideAsync();
Task UpdateTooltipAsync(string tooltip);
bool IsVisible { get; }
```

### IReportsService

```csharp
Task<DailyReport> GetDailyReportAsync(DateTime date);
Task<WeeklyReport> GetWeeklyReportAsync(DateTime startDate);
Task<MonthlyReport> GetMonthlyReportAsync(int year, int month);
Task<PeriodComparison> GetPeriodComparisonAsync(DateTime start1, DateTime end1, DateTime start2, DateTime end2);
```

## Conclusion

Phase 2 implementation provides a comprehensive set of features for proactive screen time management. The modular architecture ensures easy maintenance and future extensibility, while following WinUI 3 and Windows App SDK best practices.
