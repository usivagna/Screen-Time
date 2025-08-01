using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScreenTimeMonitor.Services;
using ScreenTimeMonitor.Data;
using System;
using System.Diagnostics;

namespace ScreenTimeMonitor
{
    public partial class App : Application
    {
        public static IHost? Host { get; private set; }

        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += App_UnhandledException;
            
            // Set up dependency injection
            var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
            
            // Register services
            builder.Services.AddSingleton<IDataService, DataService>();
            builder.Services.AddSingleton<IWindowMonitoringService, WindowMonitoringService>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddDbContext<AppDbContext>();
            
            Host = builder.Build();
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Unhandled exception: {e.Exception}");
            try
            {
                System.IO.File.WriteAllText("error.log", $"Unhandled exception: {e.Exception}");
            }
            catch { }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                Debug.WriteLine("OnLaunched called");
                var mainWindow = new MainWindow();
                Debug.WriteLine("MainWindow created");
                mainWindow.Activate();
                Debug.WriteLine("MainWindow activated");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in OnLaunched: {ex}");
                try
                {
                    System.IO.File.WriteAllText("launch_error.log", $"Exception in OnLaunched: {ex}");
                }
                catch { }
                throw;
            }
        }
    }
}
