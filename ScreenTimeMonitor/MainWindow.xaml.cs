using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ScreenTimeMonitor.Views;

namespace ScreenTimeMonitor
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "Screen Time Monitor";
            
            // Navigate to dashboard by default
            ContentFrame.Navigate(typeof(DashboardPage));
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(DashboardPage));
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(ReportsPage));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
        }
    }
}
