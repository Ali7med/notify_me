using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NotifyMe.Core.Services;
using NotifyMe.Models;

namespace NotifyMe.UI
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settingsService;
        private readonly AutoStartService _autoStartService;
        private UserSettings _tempSettings;

        public SettingsWindow(SettingsService settingsService, AutoStartService autoStartService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _autoStartService = autoStartService;
            
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

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Appearance
            OpacitySlider.Value = _tempSettings.Opacity;
            ThemeComboBox.SelectedIndex = _tempSettings.Theme == "Glass" ? 0 : 1;

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
            if (_tempSettings != null)
            {
                _tempSettings.Opacity = e.NewValue;
                _settingsService.SaveSettings(_tempSettings); 
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_tempSettings != null && ThemeComboBox.SelectedItem is ComboBoxItem item)
            {
                _tempSettings.Theme = item.Content?.ToString()?.Contains("Glass") == true ? "Glass" : "Classic";
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
    }
}
