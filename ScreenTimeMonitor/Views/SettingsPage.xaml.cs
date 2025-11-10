using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ScreenTimeMonitor.Services;
using System;

namespace ScreenTimeMonitor.Views
{
    public sealed partial class SettingsPage : Page
    {
        private readonly ISettingsService _settingsService;
        private readonly IBreakReminderService _breakReminderService;

        public SettingsPage()
        {
            this.InitializeComponent();
            
            // Get services from DI container
            _settingsService = (ISettingsService)App.Host!.Services.GetService(typeof(ISettingsService))!;
            _breakReminderService = (IBreakReminderService)App.Host!.Services.GetService(typeof(IBreakReminderService))!;
            
            this.Loaded += SettingsPage_Loaded;
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSettingsAsync();
        }

        private async System.Threading.Tasks.Task LoadSettingsAsync()
        {
            try
            {
                // Load break reminder settings
                var breakRemindersEnabled = await _settingsService.GetBreakRemindersEnabledAsync();
                BreakRemindersToggle.IsOn = breakRemindersEnabled;
                BreakReminderSettings.Visibility = breakRemindersEnabled ? Visibility.Visible : Visibility.Collapsed;

                var breakInterval = await _settingsService.GetBreakReminderIntervalAsync();
                if (breakInterval.TotalMinutes == 30)
                    BreakIntervalCombo.SelectedIndex = 0;
                else if (breakInterval.TotalHours == 1)
                    BreakIntervalCombo.SelectedIndex = 1;
                else if (breakInterval.TotalHours == 2)
                    BreakIntervalCombo.SelectedIndex = 2;
                else if (breakInterval.TotalHours == 3)
                    BreakIntervalCombo.SelectedIndex = 3;

                // Load other settings
                var startWithWindows = await _settingsService.GetStartWithWindowsAsync();
                StartWithWindowsToggle.IsOn = startWithWindows;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        private async void BreakRemindersToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                var isEnabled = BreakRemindersToggle.IsOn;
                await _settingsService.SetBreakRemindersEnabledAsync(isEnabled);
                
                BreakReminderSettings.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;

                if (isEnabled)
                {
                    await _breakReminderService.StartAsync();
                }
                else
                {
                    await _breakReminderService.StopAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling break reminders: {ex.Message}");
            }
        }
    }
}