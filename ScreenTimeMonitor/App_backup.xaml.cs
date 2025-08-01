// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using ScreenTimeMonitor.Services;
using ScreenTimeMonitor.ViewModels;
using ScreenTimeMonitor.Data;
using System;

namespace ScreenTimeMonitor
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private static IHost? _host;
        
        /// <summary>
        /// Gets the current App instance in use
        /// </summary>
        public static new App Current => (App)Application.Current;
        
        /// <summary>
        /// Gets the IServiceProvider instance for dependency injection
        /// </summary>
        public static IServiceProvider Services => _host?.Services ?? throw new InvalidOperationException("Services not initialized");

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            ConfigureServices();
        }

        /// <summary>
        /// Configure dependency injection services
        /// </summary>
        private void ConfigureServices()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Database
                    services.AddDbContext<AppDbContext>();
                    
                    // Services
                    services.AddSingleton<IWindowMonitoringService, WindowMonitoringService>();
                    services.AddSingleton<IDataService, DataService>();
                    services.AddSingleton<INotificationService, NotificationService>();
                    services.AddSingleton<ISettingsService, SettingsService>();
                    
                    // ViewModels
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<ReportsViewModel>();
                    services.AddTransient<SettingsViewModel>();
                    
                    // Views
                    services.AddTransient<MainWindow>();
                })
                .Build();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = Services.GetRequiredService<MainWindow>();
            m_window.Activate();
        }

        private Window? m_window;
    }
}
