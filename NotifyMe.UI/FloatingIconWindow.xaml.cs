using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
        private readonly Action _openAppAction;

        public FloatingIconWindow(NetworkMonitor networkMonitor, TrafficMonitor trafficMonitor, Action openAppAction)
        {
            InitializeComponent();
            _networkMonitor = networkMonitor;
            _trafficMonitor = trafficMonitor;
            _openAppAction = openAppAction;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            bool isConnected = _networkMonitor?.IsConnected ?? false;
            NetworkStats? stats = _trafficMonitor?.CurrentStats;

            if (!isConnected)
            {
                MainBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                DownloadText.Text = "--";
                UploadText.Text = "--";
            }
            else
            {
                // Check for traffic (threshold > 0 bytes)
                bool hasTraffic = stats != null && (stats.DownloadSpeedBytesPerSecond > 0 || stats.UploadSpeedBytesPerSecond > 0);
                
                MainBorder.BorderBrush = hasTraffic 
                    ? new SolidColorBrush(Colors.LimeGreen) 
                    : new SolidColorBrush(Colors.Gray);

                if (stats != null)
                {
                    DownloadText.Text = $"↓ {FormatSpeed(stats.DownloadSpeedBytesPerSecond)}";
                    UploadText.Text = $"↑ {FormatSpeed(stats.UploadSpeedBytesPerSecond)}";
                }
            }
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
