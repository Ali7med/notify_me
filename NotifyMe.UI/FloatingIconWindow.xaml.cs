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

            _networkMonitor.LatencyChanged += OnLatencyChanged;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void ApplySettings(UserSettings settings)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => ApplySettings(settings));
                return;
            }

            if (MainBorder.Background is SolidColorBrush brush)
            {
                // Create a new brush to avoid freezing issues if it's frozen
                var newBrush = new SolidColorBrush(brush.Color) { Opacity = settings.Opacity };
                MainBorder.Background = newBrush;
            }
            
            _networkMonitor.PingHost = settings.PingHost;
        }

        private void OnLatencyChanged(object? sender, long latency)
        {
            Dispatcher.Invoke(() =>
            {
                PingText.Text = $"{latency} ms";
                
                // Color code latency
                if (latency < 50) PingText.Foreground = new SolidColorBrush(Color.FromRgb(76, 255, 76)); // Green
                else if (latency < 150) PingText.Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)); // Yellow
                else PingText.Foreground = new SolidColorBrush(Color.FromRgb(255, 76, 76)); // Red
            });
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            bool isConnected = _networkMonitor?.IsConnected ?? false;
            NetworkStats? stats = _trafficMonitor?.CurrentStats;

            if (!isConnected)
            {
                // Disconnected State: Red
                SetStatusColor(Colors.Red);
                DownloadText.Text = "--";
                UploadText.Text = "--";
            }
            else
            {
                // Connected State
                bool hasTraffic = stats != null && (stats.DownloadSpeedBytesPerSecond > 0 || stats.UploadSpeedBytesPerSecond > 0);
                
                if (hasTraffic)
                {
                    // Active Traffic: Blue/Green
                    SetStatusColor(Color.FromRgb(76, 194, 255)); // #4CC2FF
                }
                else
                {
                    // Idle: Gray
                    SetStatusColor(Colors.Gray);
                }

                if (stats != null)
                {
                    DownloadText.Text = $"{FormatSpeed(stats.DownloadSpeedBytesPerSecond)}";
                    UploadText.Text = $"{FormatSpeed(stats.UploadSpeedBytesPerSecond)}";
                }
            }
        }

        private void SetStatusColor(Color color)
        {
            var brush = new SolidColorBrush(color);
            StatusBar.Background = brush;
            StatusBar.Effect = new System.Windows.Media.Effects.DropShadowEffect 
            { 
                Color = color, 
                BlurRadius = 8, 
                ShadowDepth = 0, 
                Opacity = 0.6 
            };
            
            BorderBrush.Color = color;
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

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            // Hover Effect: Scale Up slightly
            var anim = new DoubleAnimation(1.05, TimeSpan.FromMilliseconds(200));
            MainBorder.RenderTransform = new ScaleTransform(1, 1);
            MainBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            MainBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            MainBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            // Restore Scale
            var anim = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(200));
            MainBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            MainBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
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
