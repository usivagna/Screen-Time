# Screen Time Monitor - WinUI 3 Application

A Windows desktop application built with WinUI 3 and .NET 6 to monitor and track screen time usage across different applications.

## Features

### Phase 1 (Current Implementation)
- **Active Window Tracking**: Monitors which applications are currently in focus
- **Time Calculation**: Tracks duration spent in each application
- **Data Storage**: Local SQLite database to store usage data
- **Dashboard**: Shows today's usage statistics and top applications
- **Reports**: Historical data viewing with date range selection
- **Settings**: Configure monitoring preferences and application categories

### Phase 2 (Planned)
- **Daily/Weekly/Monthly Reports**: Enhanced analytics with charts
- **Usage Limits & Notifications**: Set daily limits with toast notifications
- **System Tray Integration**: Run in background with tray icon
- **Break Reminders**: Periodic notifications to take breaks

### Phase 3 (Future)
- **Website Tracking**: Monitor browser tabs and websites
- **Detailed Analytics**: Trends, patterns, most productive hours
- **Data Export**: Export reports to CSV/PDF

## Architecture

This application follows Microsoft's recommended patterns and best practices:

- **MVVM Pattern**: Using CommunityToolkit.Mvvm for clean separation of concerns
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection for service management
- **Entity Framework Core**: For data persistence with SQLite
- **Windows App SDK**: Latest WinUI 3 framework for modern Windows apps

## Project Structure

```
ScreenTimeMonitor/
├── App.xaml & App.xaml.cs          # Application entry point with DI setup
├── MainWindow.xaml & .cs           # Main application window
├── Models/                         # Data models (Application, UsageSession, DailySummary)
├── ViewModels/                     # MVVM ViewModels using CommunityToolkit.Mvvm
├── Views/                          # XAML pages (Dashboard, Reports, Settings)
├── Services/                       # Business logic services
│   ├── WindowMonitoringService     # Windows API integration for active window tracking
│   ├── DataService                 # Entity Framework data access
│   ├── SettingsService             # User preferences management
│   └── NotificationService         # Toast notifications
├── Data/                           # Entity Framework DbContext
├── Converters/                     # XAML value converters
└── Assets/                         # Application icons and resources
```

## Technologies Used

- **WinUI 3** (Windows App SDK 1.4)
- **.NET 6** (Windows-specific)
- **Entity Framework Core** with SQLite
- **CommunityToolkit.Mvvm** for MVVM implementation
- **Microsoft.Extensions.DependencyInjection** for IoC
- **Windows APIs** for window monitoring (user32.dll)

## Development Setup

### Prerequisites
- Visual Studio Code with C# Dev Kit extension
- .NET 6.0+ SDK
- Windows 10 version 1809+ or Windows 11

### Building and Running

1. **Restore packages**:
   ```bash
   dotnet restore
   ```

2. **Build the project**:
   ```bash
   dotnet build
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```

### VS Code Integration

The project includes VS Code configuration files:
- `.vscode/launch.json` - Debug configuration
- `.vscode/tasks.json` - Build tasks

## Database

The application uses SQLite for local data storage. The database file is created automatically in the user's local app data folder:
- Location: `%LOCALAPPDATA%/Packages/[AppId]/LocalState/ScreenTimeMonitor.db`

### Schema
- **Applications**: Stores information about monitored applications
- **UsageSessions**: Records individual usage sessions with start/end times
- **DailySummaries**: Aggregated daily statistics

## Privacy

- All data is stored locally on your device
- No data is transmitted to external servers
- The application only monitors active window information
- Users can control data retention periods in settings

## Contributing

This project follows Microsoft's coding standards and best practices:
- Clean Code principles
- SOLID design patterns
- Comprehensive error handling
- Proper async/await usage
- Modern C# features

## License

Licensed under the MIT License - see LICENSE file for details.

## Acknowledgments

Built following Microsoft Learn documentation and best practices:
- [WinUI 3 Documentation](https://docs.microsoft.com/windows/apps/winui/winui3/)
- [MVVM Toolkit](https://docs.microsoft.com/dotnet/communitytoolkit/mvvm/)
- [Windows App SDK](https://docs.microsoft.com/windows/apps/windows-app-sdk/)
