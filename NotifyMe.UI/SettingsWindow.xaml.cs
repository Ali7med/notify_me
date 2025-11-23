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
        private UserSettings _tempSettings;

        public SettingsWindow(SettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            
            // Clone current settings for temporary editing
            var current = _settingsService.CurrentSettings;
            _tempSettings = new UserSettings 
            { 
                Opacity = current.Opacity, 
                Theme = current.Theme,
                IsTransparent = current.IsTransparent,
                PingHost = current.PingHost
            };

            // Initialize UI
            OpacitySlider.Value = _tempSettings.Opacity;
            ThemeComboBox.SelectedIndex = _tempSettings.Theme == "Glass" ? 0 : 1;
            PingHostTextBox.Text = _tempSettings.PingHost;
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
                _tempSettings.Theme = item.Content.ToString().Contains("Glass") ? "Glass" : "Classic";
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
            }
            _settingsService.SaveSettings(_tempSettings);
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
