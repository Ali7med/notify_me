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
                DNDEndTime = current.DNDEndTime
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
            
            // Initialize DND
            EnableDNDCheckBox.IsChecked = _tempSettings.EnableDND;
            EnableDNDScheduleCheckBox.IsChecked = _tempSettings.EnableDNDSchedule;
            DNDSchedulePanel.Visibility = _tempSettings.EnableDNDSchedule ? Visibility.Visible : Visibility.Collapsed;

            // Populate Time ComboBoxes
            for (int i = 0; i < 24; i++)
            {
                DNDStartHourCombo.Items.Add(i.ToString("00"));
                DNDEndHourCombo.Items.Add(i.ToString("00"));
            }
            for (int i = 0; i < 60; i += 15) // 15 min intervals
            {
                DNDStartMinuteCombo.Items.Add(i.ToString("00"));
                DNDEndMinuteCombo.Items.Add(i.ToString("00"));
            }

            // Set Time Values
            DNDStartHourCombo.SelectedItem = _tempSettings.DNDStartTime.Hours.ToString("00");
            DNDStartMinuteCombo.SelectedItem = _tempSettings.DNDStartTime.Minutes.ToString("00");
            DNDEndHourCombo.SelectedItem = _tempSettings.DNDEndTime.Hours.ToString("00");
            DNDEndMinuteCombo.SelectedItem = _tempSettings.DNDEndTime.Minutes.ToString("00");
            
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

            // Load available languages dynamically
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

            // Ensure correct selection based on current settings
            var selectedItem = LanguageComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(i => i.Tag?.ToString() == _tempSettings.Language);
            
            if (selectedItem != null)
                selectedItem.IsSelected = true;
            else if (LanguageComboBox.Items.Count > 0)
                ((ComboBoxItem)LanguageComboBox.Items[0]).IsSelected = true;
            
            // Apply current language translations
            ApplyTranslations();
        }
        
        private void ApplyTranslations()
        {
            var lang = Helpers.LocalizationManager.Current;
            
            // Window Title
            this.Title = lang.Settings.Title;
            
            // Header
            if (HeaderTitle != null)
                HeaderTitle.Text = lang.Settings.Title;
            if (HeaderSubtitle != null)
                HeaderSubtitle.Text = lang.Settings.Subtitle;
            
            // Tabs
            if (TabAppearance != null)
                TabAppearance.Header = lang.Settings.TabAppearance;
            if (TabNetwork != null)
                TabNetwork.Header = lang.Settings.TabNetwork;
            if (TabNotifications != null)
                TabNotifications.Header = lang.Settings.TabNotifications;
            if (TabAdvanced != null)
                TabAdvanced.Header = lang.Settings.TabAdvanced;
            
            // Save Button
            if (SaveButton != null)
                SaveButton.Content = lang.Settings.SaveChanges;
            
            // Appearance Tab
            if (LblTransparency != null)
                LblTransparency.Text = lang.Settings.Transparency;
            if (LblTransparencyDesc != null)
                LblTransparencyDesc.Text = lang.Settings.TransparencyDesc;
            if (LblTheme != null)
                LblTheme.Text = lang.Settings.Theme;
            if (LblThemeDesc != null)
                LblThemeDesc.Text = lang.Settings.ThemeDesc;
            
            // Network Tab
            if (LblPingHost != null)
                LblPingHost.Text = lang.Settings.PingHost;
            if (LblPingHostDesc != null)
                LblPingHostDesc.Text = lang.Settings.PingHostDesc;
            if (LblQuickSelect != null)
                LblQuickSelect.Text = lang.Settings.QuickSelect;
            
            // Notifications Tab
            if (LblAlertPreferences != null)
                LblAlertPreferences.Text = lang.Settings.AlertPreferences;
            if (LblNotificationType != null)
                LblNotificationType.Text = lang.Settings.NotificationType;
            if (LblWindowsToastDesc != null)
                LblWindowsToastDesc.Text = lang.Settings.WindowsToastDesc;
            if (LblCustomNotificationsDesc != null)
                LblCustomNotificationsDesc.Text = lang.Settings.CustomNotificationsDesc;
            if (LblNotificationPosition != null)
                LblNotificationPosition.Text = lang.Settings.NotificationPosition;
            if (LblShowToastDesc != null)
                LblShowToastDesc.Text = lang.Settings.ShowToastDesc;
            if (LblPlaySoundDesc != null)
                LblPlaySoundDesc.Text = lang.Settings.PlaySoundDesc;
            if (LblDoNotDisturb != null)
                LblDoNotDisturb.Text = lang.Settings.DoNotDisturb;
            if (LblDNDNowDesc != null)
                LblDNDNowDesc.Text = lang.Settings.DNDNowDesc;
            if (LblDNDScheduleDesc != null)
                LblDNDScheduleDesc.Text = lang.Settings.DNDScheduleDesc;
            if (LblStartTime != null)
                LblStartTime.Text = lang.Settings.StartTime;
            if (LblEndTime != null)
                LblEndTime.Text = lang.Settings.EndTime;
            if (LblTipTitle != null)
                LblTipTitle.Text = lang.Notifications.TipTitle;
            if (LblTipBody != null)
                LblTipBody.Text = lang.Notifications.TipBody;
                
            // RadioButtons & CheckBoxes
            if (ToastNotificationRadio != null)
                ToastNotificationRadio.Content = lang.Settings.WindowsToast;
            if (CustomNotificationRadio != null)
                CustomNotificationRadio.Content = lang.Settings.CustomNotifications;
            if (ToastNotificationsCheckBox != null)
                ToastNotificationsCheckBox.Content = lang.Settings.ShowToast;
            if (SoundNotificationsCheckBox != null)
                SoundNotificationsCheckBox.Content = lang.Settings.PlaySound;
            if (EnableDNDCheckBox != null)
                EnableDNDCheckBox.Content = lang.Settings.DNDNow;
            if (EnableDNDScheduleCheckBox != null)
                EnableDNDScheduleCheckBox.Content = lang.Settings.DNDSchedule;
            if (AutoStartCheckBox != null)
                AutoStartCheckBox.Content = lang.Settings.StartWithWindows;

            // ComboBox Items - Theme
            if (ThemeComboBox != null && ThemeComboBox.Items.Count >= 2)
            {
                if (ThemeComboBox.Items[0] is ComboBoxItem item1) item1.Content = lang.Settings.GlassTheme;
                if (ThemeComboBox.Items[1] is ComboBoxItem item2) item2.Content = lang.Settings.ClassicTheme;
            }

            // ComboBox Items - Notification Position
            if (NotificationPositionComboBox != null)
            {
                foreach (ComboBoxItem item in NotificationPositionComboBox.Items)
                {
                    switch (item.Tag?.ToString())
                    {
                        case "TopRight": item.Content = lang.NotificationPositionItems.TopRight; break;
                        case "TopLeft": item.Content = lang.NotificationPositionItems.TopLeft; break;
                        case "BottomRight": item.Content = lang.NotificationPositionItems.BottomRight; break;
                        case "BottomLeft": item.Content = lang.NotificationPositionItems.BottomLeft; break;
                    }
                }
            }
            
            // Advanced Tab
            if (LblUpdateFrequency != null)
                LblUpdateFrequency.Text = lang.Settings.UpdateFrequency;
            if (LblUpdateFrequencyDesc != null)
                LblUpdateFrequencyDesc.Text = lang.Settings.UpdateFrequencyDesc;
            if (LblTrafficThreshold != null)
                LblTrafficThreshold.Text = lang.Settings.TrafficThreshold;
            if (LblTrafficThresholdDesc != null)
                LblTrafficThresholdDesc.Text = lang.Settings.TrafficThresholdDesc;
            if (LblStartupBehavior != null)
                LblStartupBehavior.Text = lang.Settings.StartupBehavior;
            if (LblStartWithWindowsDesc != null)
                LblStartWithWindowsDesc.Text = lang.Settings.StartWithWindowsDesc;
            if (LblLanguage != null)
                LblLanguage.Text = lang.Settings.Language;
            if (LblInterfaceLanguage != null)
                LblInterfaceLanguage.Text = lang.Settings.InterfaceLanguage;
            if (LblSelectLanguage != null)
                LblSelectLanguage.Text = lang.Settings.SelectLanguage;
            
            // Update Language ComboBox to display in current language
            foreach (ComboBoxItem item in LanguageComboBox.Items)
            {
                var code = item.Tag?.ToString();
                if (code != null)
                {
                    // Find the language data to get the localized name
                    var langData = Helpers.LocalizationManager.GetAvailableLanguages()
                        .FirstOrDefault(l => l.LanguageCode == code);
                    if (langData != null)
                    {
                        item.Content = langData.LanguageName;
                    }
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

                // DND Settings
                _tempSettings.EnableDND = EnableDNDCheckBox.IsChecked ?? false;
                _tempSettings.EnableDNDSchedule = EnableDNDScheduleCheckBox.IsChecked ?? false;

                if (int.TryParse(DNDStartHourCombo.SelectedItem?.ToString(), out int startHour) &&
                    int.TryParse(DNDStartMinuteCombo.SelectedItem?.ToString(), out int startMin))
                {
                    _tempSettings.DNDStartTime = new TimeSpan(startHour, startMin, 0);
                }

                if (int.TryParse(DNDEndHourCombo.SelectedItem?.ToString(), out int endHour) &&
                    int.TryParse(DNDEndMinuteCombo.SelectedItem?.ToString(), out int endMin))
                {
                    _tempSettings.DNDEndTime = new TimeSpan(endHour, endMin, 0);
                }

                // Language
                if (LanguageComboBox.SelectedItem is ComboBoxItem selectedLanguage)
                {
                    var newLang = selectedLanguage.Tag?.ToString() ?? "en";
                    _tempSettings.Language = newLang;
                    // Load and apply language immediately
                    Helpers.LocalizationManager.LoadLanguage(newLang);
                    ApplyTranslations();
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

        private void EnableDNDScheduleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DNDSchedulePanel != null)
                DNDSchedulePanel.Visibility = Visibility.Visible;
        }

        private void EnableDNDScheduleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DNDSchedulePanel != null)
                DNDSchedulePanel.Visibility = Visibility.Collapsed;
        }
    }
}
