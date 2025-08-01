// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ScreenTimeMonitor.ViewModels;

namespace ScreenTimeMonitor.Views
{
    /// <summary>
    /// Settings page for configuring application preferences
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; private set; } = null!;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationPageEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            if (e.Parameter is SettingsViewModel viewModel)
            {
                ViewModel = viewModel;
                DataContext = ViewModel;
                
                // Load settings
                await ViewModel.LoadSettingsCommand.ExecuteAsync(null);
            }
        }
    }
}
