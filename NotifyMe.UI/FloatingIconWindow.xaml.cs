using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using NotifyMe.Core.Services;
using NotifyMe.Models;

namespace NotifyMe.UI
{
    public partial class FloatingIconWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly NetworkMonitor _networkMonitor;
        private readonly TrafficMonitor _trafficMonitor;
        private readonly SettingsService _settingsService;
        private readonly Action _openAppAction;
        private readonly Action _openSettingsAction;

        public FloatingIconWindow(NetworkMonitor networkMonitor, TrafficMonitor trafficMonitor, SettingsService settingsService, Action openAppAction, Action openSettingsAction)
        {
            InitializeComponent();
            _networkMonitor = networkMonitor;
            _trafficMonitor = trafficMonitor;
            _settingsService = settingsService;
            _openAppAction = openAppAction;
            _openSettingsAction = openSettingsAction;

            // Apply initial settings
            ApplySettings(_settingsService.CurrentSettings);
            _settingsService.SettingsChanged += (s, settings) => Dispatcher.Invoke(() => ApplySettings(settings));

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void ApplySettings(UserSettings settings)
        {
            // Ensure we are on the UI thread
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => ApplySettings(settings));
                return;
            }

            Opacity = settings.Opacity;
            
            // Apply Theme
            if (settings.Theme == "Glass")
            {
                // Re-apply glass effect if needed (already default)
                MainBorder.Background = new SolidColorBrush(Color.FromArgb((byte)(255 * 0.85), 30, 30, 30)); // #1E1E1E with 0.85 opacity
            }
            else
            {
                // Classic Theme (Solid)
                MainBorder.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)); // #1E1E1E Solid
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            bool isConnected = _networkMonitor?.IsConnected ?? false;
            NetworkStats? stats = _trafficMonitor?.CurrentStats;

            Color startColor, endColor;

            if (!isConnected)
            {
                // Red gradient for disconnected
                startColor = Colors.Red;
                endColor = Colors.DarkRed;
                DownloadText.Text = "--";
                UploadText.Text = "--";
                DownloadText.Foreground = new SolidColorBrush(Colors.Red);
                UploadText.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                // Check for traffic (threshold > 0 bytes)
                bool hasTraffic = stats != null && (stats.DownloadSpeedBytesPerSecond > 0 || stats.UploadSpeedBytesPerSecond > 0);
                
                if (hasTraffic)
                {
                    // Green/Blue gradient for activity
                    startColor = Color.FromRgb(76, 194, 255); // #4CC2FF (Light Blue)
                    endColor = Color.FromRgb(255, 215, 0);    // #FFD700 (Gold)
                }
                else
                {
                    // Gray gradient for idle
                    startColor = Colors.Gray;
                    endColor = Colors.DarkGray;
                }

                if (stats != null)
                {
                    DownloadText.Text = $"↓ {FormatSpeed(stats.DownloadSpeedBytesPerSecond)}";
                    UploadText.Text = $"↑ {FormatSpeed(stats.UploadSpeedBytesPerSecond)}";
                    DownloadText.Foreground = new SolidColorBrush(Color.FromRgb(76, 194, 255));
                    UploadText.Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0));
                }
            }

            AnimateGradient(startColor, endColor);
        }

        private void AnimateGradient(Color toStartColor, Color toEndColor)
        {
            var duration = TimeSpan.FromMilliseconds(500);

            var startAnimation = new ColorAnimation(toStartColor, duration);
            var endAnimation = new ColorAnimation(toEndColor, duration);

            BorderGradient.GradientStops[0].BeginAnimation(GradientStop.ColorProperty, startAnimation);
            BorderGradient.GradientStops[1].BeginAnimation(GradientStop.ColorProperty, endAnimation);
        }

        private string FormatSpeed(double bytesPerSecond)
        {
            if (bytesPerSecond < 1024) return $"{bytesPerSecond:F0} B";
            if (bytesPerSecond < 1024 * 1024) return $"{bytesPerSecond / 1024.0:F1} K";
            return $"{bytesPerSecond / (1024.0 * 1024.0):F1} M";
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _openAppAction?.Invoke();
        }

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            _openAppAction?.Invoke();
        }

        private void MenuItem_Hide_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void MenuItem_Settings_Click(object sender, RoutedEventArgs e)
        {
            _openSettingsAction?.Invoke();
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 20;
            Top = workArea.Bottom - Height - 20;
        }
    }
}
