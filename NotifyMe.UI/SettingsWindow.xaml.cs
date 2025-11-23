using System.Windows;
using System.Windows.Controls;
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
                IsTransparent = current.IsTransparent
            };

            // Initialize UI
            OpacitySlider.Value = _tempSettings.Opacity;
            ThemeComboBox.SelectedIndex = _tempSettings.Theme == "Glass" ? 0 : 1;
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_tempSettings != null)
            {
                _tempSettings.Opacity = e.NewValue;
                // Live preview (optional, requires binding in FloatingIconWindow)
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
            _settingsService.SaveSettings(_tempSettings);
            Close();
        }
    }
}
