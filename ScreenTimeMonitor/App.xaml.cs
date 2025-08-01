using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;

namespace ScreenTimeMonitor
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Unhandled exception: {e.Exception}");
            System.IO.File.WriteAllText("error.log", $"Unhandled exception: {e.Exception}");
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
                System.IO.File.WriteAllText("launch_error.log", $"Exception in OnLaunched: {ex}");
                throw;
            }
        }
    }
}
