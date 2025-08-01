using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenTimeMonitor.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ScreenTimeMonitor.ViewModels
{
    /// <summary>
    /// ViewModel for the settings view
    /// </summary>
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;

        [ObservableProperty]
        private bool _startWithWindows;

        [ObservableProperty]
        private bool _breakRemindersEnabled;

        [ObservableProperty]
        private int _breakReminderIntervalMinutes = 60;

        [ObservableProperty]
        private int _dataRetentionDays = 90;

        [ObservableProperty]
        private string _selectedTheme = "Default";

        [ObservableProperty]
        private ObservableCollection<string> _availableThemes = new() { "Default", "Light", "Dark" };

        [ObservableProperty]
        private ObservableCollection<string> _availableCategories = new();

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [RelayCommand]
        private async Task LoadSettingsAsync()
        {
            try
            {
                StartWithWindows = await _settingsService.GetStartWithWindowsAsync();
                BreakRemindersEnabled = await _settingsService.GetBreakRemindersEnabledAsync();
                
                var breakInterval = await _settingsService.GetBreakReminderIntervalAsync();
                BreakReminderIntervalMinutes = (int)breakInterval.TotalMinutes;
                
                DataRetentionDays = await _settingsService.GetDataRetentionDaysAsync();
                SelectedTheme = await _settingsService.GetThemeAsync();
                
                var categories = await _settingsService.GetAvailableCategoriesAsync();
                AvailableCategories.Clear();
                foreach (var category in categories)
                {
                    AvailableCategories.Add(category);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            try
            {
                await _settingsService.SetStartWithWindowsAsync(StartWithWindows);
                await _settingsService.SetBreakRemindersEnabledAsync(BreakRemindersEnabled);
                await _settingsService.SetBreakReminderIntervalAsync(TimeSpan.FromMinutes(BreakReminderIntervalMinutes));
                await _settingsService.SetDataRetentionDaysAsync(DataRetentionDays);
                await _settingsService.SetThemeAsync(SelectedTheme);
                
                // TODO: Show success message
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                // TODO: Show error message
            }
        }

        [RelayCommand]
        private async Task ResetSettingsAsync()
        {
            // Reset to defaults
            StartWithWindows = false;
            BreakRemindersEnabled = true;
            BreakReminderIntervalMinutes = 60;
            DataRetentionDays = 90;
            SelectedTheme = "Default";
            
            await SaveSettingsAsync();
        }
    }
}
