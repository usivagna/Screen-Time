#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ScreenTimeMonitor.Views;

namespace ScreenTimeMonitor
{
    public sealed partial class MainWindow : Window
    {
        private Type? _currentPageType;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "Screen Time Monitor";
            
            // Navigate to dashboard by default
            NavigateToPage(typeof(DashboardPage));
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(DashboardPage));
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(ReportsPage));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(SettingsPage));
        }

        private void NavigateToPage(Type pageType)
        {
            // Only navigate if we're not already on this page
            if (_currentPageType != pageType)
            {
                ContentFrame.Navigate(pageType);
                _currentPageType = pageType;
            }
        }
    }
}
