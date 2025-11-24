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
                CustomNotificationPosition = current.CustomNotificationPosition
            };

            // Initialize UI
            OpacitySlider.Value = _tempSettings.Opacity;
            ThemeComboBox.SelectedIndex = _tempSettings.Theme == "Glass" ? 0 : 1;
            PingHostTextBox.Text = _tempSettings.PingHost;
            UpdateIntervalSlider.Value = _tempSettings.UpdateIntervalSeconds;
            TrafficThresholdTextBox.Text = _tempSettings.HighTrafficThresholdMBps.ToString("F1");
            TrafficUnitComboBox.SelectedIndex = _tempSettings.HighTrafficThresholdUnit switch
            {
                "KB" => 0,
                "MB" => 1,
                "GB" => 2,
                _ => 1
            };
            ToastNotificationsCheckBox.IsChecked = _tempSettings.EnableToastNotifications;
            SoundNotificationsCheckBox.IsChecked = _tempSettings.EnableSoundNotifications;
            AutoStartCheckBox.IsChecked = _tempSettings.StartWithWindows;
            
            // Initialize Notification Type
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
            
            // Initialize Position
            foreach (ComboBoxItem item in NotificationPositionComboBox.Items)
            {
                if (item.Tag?.ToString() == _tempSettings.CustomNotificationPosition)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_tempSettings != null)
            {
                _tempSettings.Opacity = e.NewValue;
                // Live preview: Save immediately to trigger event
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
                var pingHost = PingHostTextBox.Text.Trim();
                
                // Basic validation - check if not empty
                if (string.IsNullOrWhiteSpace(pingHost))
                {
                    System.Windows.MessageBox.Show("Please enter a valid Ping IP address.", "Invalid IP", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                _tempSettings.PingHost = pingHost;
                
                // Advanced settings
                _tempSettings.UpdateIntervalSeconds = (int)UpdateIntervalSlider.Value;
                
                if (double.TryParse(TrafficThresholdTextBox.Text, out double threshold))
                {
                    _tempSettings.HighTrafficThresholdMBps = threshold;
                }
                
                // Traffic threshold unit
                _tempSettings.HighTrafficThresholdUnit = TrafficUnitComboBox.SelectedIndex switch
                {
                    0 => "KB",
                    1 => "MB",
                    2 => "GB",
                    _ => "MB"
                };
                
                _tempSettings.EnableToastNotifications = ToastNotificationsCheckBox.IsChecked ?? true;
                _tempSettings.EnableSoundNotifications = SoundNotificationsCheckBox.IsChecked ?? true;
                
                // Notification Type & Position
                _tempSettings.NotificationType = CustomNotificationRadio.IsChecked == true ? "Custom" : "Toast";
                
                if (NotificationPositionComboBox.SelectedItem is ComboBoxItem selectedPosition)
                {
                    _tempSettings.CustomNotificationPosition = selectedPosition.Tag?.ToString() ?? "TopRight";
                }

                // Auto-Start handling
                var startWithWindows = AutoStartCheckBox.IsChecked ?? false;
                _tempSettings.StartWithWindows = startWithWindows;
                
                // Apply auto-start setting to registry
                if (startWithWindows)
                {
                    if (!_autoStartService.Enable())
                    {
                        MessageBox.Show("Failed to enable auto-start. Please check your permissions.", 
                            "Auto-Start Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        _tempSettings.StartWithWindows = false; // Revert on failure
                    }
                }
                else
                {
                    _autoStartService.Disable();
                }
            }
                _settingsService.SaveSettings(_tempSettings);
            Close();
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
