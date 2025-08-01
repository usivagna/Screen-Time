// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ScreenTimeMonitor.ViewModels;
using System;

namespace ScreenTimeMonitor.Views
{
    /// <summary>
    /// Dashboard page showing current day statistics and top applications
    /// </summary>
    public sealed partial class DashboardPage : Page
    {
        public DashboardViewModel ViewModel { get; private set; } = null!;

        public DashboardPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationPageEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            if (e.Parameter is DashboardViewModel viewModel)
            {
                ViewModel = viewModel;
                DataContext = ViewModel;
                
                // Load initial data
                await ViewModel.LoadDataCommand.ExecuteAsync(null);
            }
        }

        private async void CalendarDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate.HasValue && ViewModel != null)
            {
                await ViewModel.SelectDateCommand.ExecuteAsync(args.NewDate.Value.DateTime);
            }
        }
    }
}
