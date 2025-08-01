using Microsoft.UI.Xaml;

namespace ScreenTimeMonitor
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var mainWindow = new MainWindow();
            mainWindow.Activate();
        }
    }
}
