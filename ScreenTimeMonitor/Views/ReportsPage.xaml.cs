// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ScreenTimeMonitor.ViewModels;

namespace ScreenTimeMonitor.Views
{
    /// <summary>
    /// Reports page showing historical screen time data and analytics
    /// </summary>
    public sealed partial class ReportsPage : Page
    {
        public ReportsViewModel ViewModel { get; private set; } = null!;

        public ReportsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationPageEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            if (e.Parameter is ReportsViewModel viewModel)
            {
                ViewModel = viewModel;
                DataContext = ViewModel;
                
                // Load initial data
                await ViewModel.LoadReportsCommand.ExecuteAsync(null);
            }
        }

        #region Helper Methods for Data Binding

        public Visibility GetReportsVisibility(int count)
        {
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility GetNoDataVisibility(int count, bool isLoading)
        {
            return !isLoading && count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public Brush GetProductivityScoreBrush(double score)
        {
            if (score >= 7.0)
                return new SolidColorBrush(Microsoft.UI.Colors.Green);
            else if (score >= 4.0)
                return new SolidColorBrush(Microsoft.UI.Colors.Orange);
            else
                return new SolidColorBrush(Microsoft.UI.Colors.Red);
        }

        #endregion
    }
}
