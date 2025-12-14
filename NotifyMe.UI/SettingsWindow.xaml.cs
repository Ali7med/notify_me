using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NotifyMe.Core.Services;
using NotifyMe.Models;
using NotifyMe.UI.Services;

namespace NotifyMe.UI
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settingsService;
        private readonly AutoStartService _autoStartService;
        private readonly NotificationService _notificationService;
        private readonly SoundService _soundService;
        private UserSettings _tempSettings;

        public SettingsWindow(SettingsService settingsService, AutoStartService autoStartService, NotificationService notificationService, SoundService soundService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _autoStartService = autoStartService;
            _notificationService = notificationService;
            _soundService = soundService;
            
            // Clone current settings for temporary editing
            var current = _settingsService.CurrentSettings;
            _tempSettings = new UserSettings 
            { 
                Opacity = current.Opacity, 
                Theme = current.Theme,
                IsTransparent = current.IsTransparent,
                PingHost = current.PingHost,
                UpdateIntervalSeconds = current.UpdateIntervalSeconds,
                HighTrafficThresholdMBps = current.HighTrafficThresholdMBps,
                HighTrafficThresholdUnit = current.HighTrafficThresholdUnit,
                EnableSoundNotifications = current.EnableSoundNotifications,
                EnableToastNotifications = current.EnableToastNotifications,
                StartWithWindows = current.StartWithWindows,
                NotificationType = current.NotificationType,
                CustomNotificationPosition = current.CustomNotificationPosition,
                EnableDND = current.EnableDND,
                EnableDNDSchedule = current.EnableDNDSchedule,
                DNDStartTime = current.DNDStartTime,
                DNDEndTime = current.DNDEndTime,
                Language = current.Language
            };

            try
            {
                InitializeUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Settings UI: {ex.Message}\n\n{ex.StackTrace}", "Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool _isInitializing = true;

        private void InitializeUI()
        {
            _isInitializing = true;

            // Appearance
            OpacitySlider.Value = _tempSettings.Opacity;
            ThemeComboBox.SelectedIndex = _tempSettings.Theme == "Glass" ? 0 : 1;
            
            // Initial state for Opacity Slider
            if (TransparencyPanel != null)
            {
                TransparencyPanel.IsEnabled = (_tempSettings.Theme == "Glass");
                TransparencyPanel.Opacity = (_tempSettings.Theme == "Glass") ? 1.0 : 0.5;
            }

            // Language
            LanguageComboBox.Items.Clear();
            foreach (var lang in Helpers.LocalizationManager.GetAvailableLanguages())
            {
                var item = new ComboBoxItem
                {
                    Content = lang.LanguageName,
                    Tag = lang.LanguageCode
                };
                LanguageComboBox.Items.Add(item);
            }
            
            var selectedLang = LanguageComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(i => i.Tag?.ToString() == _tempSettings.Language);
            if (selectedLang != null) selectedLang.IsSelected = true;

            // Network
            PingHostTextBox.Text = _tempSettings.PingHost;
            
            // Notifications
            ToastNotificationsCheckBox.IsChecked = _tempSettings.EnableToastNotifications;
            SoundNotificationsCheckBox.IsChecked = _tempSettings.EnableSoundNotifications;
            
            if (_tempSettings.NotificationType == "Custom")
            {
                CustomNotificationRadio.IsChecked = true;
                CustomPositionPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ToastNotificationRadio.IsChecked = true;
                CustomPositionPanel.Visibility = Visibility.Collapsed;
            }

            foreach (ComboBoxItem item in NotificationPositionComboBox.Items)
            {
                if (item.Tag?.ToString() == _tempSettings.CustomNotificationPosition)
                {
                    item.IsSelected = true;
                    break;
                }
            }

            // Advanced
            UpdateIntervalSlider.Value = _tempSettings.UpdateIntervalSeconds;
            TrafficThresholdTextBox.Text = _tempSettings.HighTrafficThresholdMBps.ToString("F1");
            TrafficUnitComboBox.SelectedIndex = _tempSettings.HighTrafficThresholdUnit switch
            {
                "KB" => 0,
                "MB" => 1,
                "GB" => 2,
                _ => 1
            };
            RunAtStartupCheckBox.IsChecked = _tempSettings.StartWithWindows;

            // Do Not Disturb
            if (DNDCheckBox != null) DNDCheckBox.IsChecked = _tempSettings.EnableDND;
            if (DNDScheduleCheckBox != null) DNDScheduleCheckBox.IsChecked = _tempSettings.EnableDNDSchedule;
            if (DNDStartBox != null) DNDStartBox.Text = _tempSettings.DNDStartTime.ToString(@"hh\:mm");
            if (DNDEndBox != null) DNDEndBox.Text = _tempSettings.DNDEndTime.ToString(@"hh\:mm");
            if (DNDSchedulePanel != null) DNDSchedulePanel.Visibility = _tempSettings.EnableDNDSchedule ? Visibility.Visible : Visibility.Collapsed;

            _isInitializing = false;
        }

        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavList.SelectedItem is ListBoxItem item && item.Tag is string tag)
            {
                // Update Title
                if (PageTitle != null) PageTitle.Text = item.Content.ToString().Substring(3); // Remove emoji

                // Hide all views
                if (ViewAppearance != null) ViewAppearance.Visibility = Visibility.Collapsed;
                if (ViewNetwork != null) ViewNetwork.Visibility = Visibility.Collapsed;
                if (ViewNotifications != null) ViewNotifications.Visibility = Visibility.Collapsed;
                if (ViewAdvanced != null) ViewAdvanced.Visibility = Visibility.Collapsed;

                // Show selected view
                switch (tag)
                {
                    case "Appearance":
                        if (ViewAppearance != null) ViewAppearance.Visibility = Visibility.Visible;
                        break;
                    case "Network":
                        if (ViewNetwork != null) ViewNetwork.Visibility = Visibility.Visible;
                        break;
                    case "Notifications":
                        if (ViewNotifications != null) ViewNotifications.Visibility = Visibility.Visible;
                        break;
                    case "Advanced":
                        if (ViewAdvanced != null) ViewAdvanced.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isInitializing) return;

            if (_tempSettings != null)
            {
                _tempSettings.Opacity = e.NewValue;
                _settingsService.SaveSettings(_tempSettings); 
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            if (_tempSettings != null && ThemeComboBox.SelectedItem is ComboBoxItem item)
            {
                var theme = item.Content?.ToString()?.Contains("Glass") == true ? "Glass" : "Classic";
                _tempSettings.Theme = theme;
                
                // Enable/Disable Opacity Slider
                if (TransparencyPanel != null)
                {
                    TransparencyPanel.IsEnabled = (theme == "Glass");
                    TransparencyPanel.Opacity = (theme == "Glass") ? 1.0 : 0.5;
                }

                _settingsService.SaveSettings(_tempSettings);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_tempSettings != null)
            {
                // Validate Ping Host
                var pingHost = PingHostTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(pingHost))
                {
                    MessageBox.Show("Please enter a valid Ping IP address.", "Invalid IP", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _tempSettings.PingHost = pingHost;

                // Advanced Settings
                _tempSettings.UpdateIntervalSeconds = (int)UpdateIntervalSlider.Value;
                
                if (double.TryParse(TrafficThresholdTextBox.Text, out double threshold))
                {
                    _tempSettings.HighTrafficThresholdMBps = threshold;
                }

                _tempSettings.HighTrafficThresholdUnit = TrafficUnitComboBox.SelectedIndex switch
                {
                    0 => "KB",
                    1 => "MB",
                    2 => "GB",
                    _ => "MB"
                };

                _tempSettings.EnableToastNotifications = ToastNotificationsCheckBox.IsChecked ?? true;
                _tempSettings.EnableSoundNotifications = SoundNotificationsCheckBox.IsChecked ?? true;
                _tempSettings.NotificationType = CustomNotificationRadio.IsChecked == true ? "Custom" : "Toast";

                if (NotificationPositionComboBox.SelectedItem is ComboBoxItem selectedPosition)
                {
                    _tempSettings.CustomNotificationPosition = selectedPosition.Tag?.ToString() ?? "TopRight";
                }

                // Do Not Disturb
                _tempSettings.EnableDND = DNDCheckBox.IsChecked ?? false;
                _tempSettings.EnableDNDSchedule = DNDScheduleCheckBox.IsChecked ?? false;
                if (TimeSpan.TryParse(DNDStartBox.Text, out var start)) _tempSettings.DNDStartTime = start;
                else _tempSettings.DNDStartTime = new TimeSpan(22, 0, 0); // Default if invalid

                if (TimeSpan.TryParse(DNDEndBox.Text, out var end)) _tempSettings.DNDEndTime = end;
                else _tempSettings.DNDEndTime = new TimeSpan(7, 0, 0); // Default if invalid

                // Language
                if (LanguageComboBox.SelectedItem is ComboBoxItem selectedLang)
                {
                    var newLang = selectedLang.Tag?.ToString() ?? "en";
                    _tempSettings.Language = newLang;
                    Helpers.LocalizationManager.LoadLanguage(newLang);
                }

                // Auto Start
                var startWithWindows = RunAtStartupCheckBox.IsChecked ?? false;
                _tempSettings.StartWithWindows = startWithWindows;

                if (startWithWindows)
                {
                    if (!_autoStartService.Enable())
                    {
                        MessageBox.Show("Failed to enable auto-start. Please check your permissions.", "Auto-Start Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        _tempSettings.StartWithWindows = false;
                    }
                }
                else
                {
                    _autoStartService.Disable();
                }

                _settingsService.SaveSettings(_tempSettings);
                Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SetPingHost_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string host)
            {
                PingHostTextBox.Text = host;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CustomNotificationRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (CustomPositionPanel != null)
                CustomPositionPanel.Visibility = Visibility.Visible;
        }

        private void ToastNotificationRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (CustomPositionPanel != null)
                CustomPositionPanel.Visibility = Visibility.Collapsed;
        }

        private void TestNotification_Click(object sender, RoutedEventArgs e)
        {
            if (_notificationService != null)
            {
                // Temporarily apply current UI settings to test what's selected
                var tempSettings = _settingsService.CurrentSettings; // Use current saved settings for test
                
                // Override with UI state if needed, but for now let's test the service directly
                _notificationService.ShowCustomNotification("Test Notification", "This is a test notification from NotifyMe.");
                
                // Also try standard toast if selected
                if (ToastNotificationRadio.IsChecked == true)
                {
                     new Microsoft.Toolkit.Uwp.Notifications.ToastContentBuilder()
                    .AddText("Test Notification")
                    .AddText("This is a standard Windows Toast notification.")
                    .Show();
                }
            }
        }

        private void TestSound_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. System Beep (Hardware/OS)
                System.Media.SystemSounds.Beep.Play();
                
                if (_soundService != null)
                {
                    // Ensure enabled for test
                    var wasEnabled = _soundService.IsEnabled;
                    _soundService.IsEnabled = true;
                    
                    // 2. Service Sound
                    _soundService.PlayConnectionLost();
                    
                    _soundService.IsEnabled = wasEnabled; // Restore state
                }
                else
                {
                    MessageBox.Show("Sound Service is not initialized!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sound Test Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DNDCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Toggle logic if needed in real-time
        }

        private void DNDScheduleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DNDSchedulePanel != null)
                DNDSchedulePanel.Visibility = (DNDScheduleCheckBox.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
