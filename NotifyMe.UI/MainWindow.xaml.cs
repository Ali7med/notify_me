using System.Windows;
using System.Windows.Media;
using NotifyMe.Core.Services;
using NotifyMe.Models;

namespace NotifyMe.UI;

public partial class MainWindow : Window
{
    private readonly NetworkMonitor? _networkMonitor;
    private readonly TrafficMonitor? _trafficMonitor;

    public MainWindow(NetworkMonitor? networkMonitor, TrafficMonitor? trafficMonitor)
    {
        InitializeComponent();
        try 
        {
            Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/icon.ico"));
        }
        catch
        {
            // Ignore if icon is missing or invalid to prevent crash
        }

        _networkMonitor = networkMonitor;
        _trafficMonitor = trafficMonitor;

        if (_networkMonitor != null)
        {
            _networkMonitor.ConnectionStateChanged += OnConnectionStateChanged;
            UpdateConnectionStatus(_networkMonitor.IsConnected);
        }

        if (_trafficMonitor != null)
        {
            _trafficMonitor.TrafficUpdated += OnTrafficUpdated;
            UpdateTrafficStats(_trafficMonitor.CurrentStats);
        }
    }

    private void OnConnectionStateChanged(object? sender, ConnectionEvent e)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateConnectionStatus(e.EventType == ConnectionEventType.Connected);
        });
    }

    private void OnTrafficUpdated(object? sender, NetworkStats stats)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateTrafficStats(stats);
        });
    }

    private void UpdateConnectionStatus(bool isConnected)
    {
        StatusText.Text = isConnected ? "Connected" : "Disconnected";
        StatusIndicator.Fill = new SolidColorBrush(isConnected ? Colors.Lime : Colors.Red);
    }

    private void UpdateTrafficStats(NetworkStats stats)
    {
        DownloadSpeed.Text = stats.DownloadSpeedFormatted;
        UploadSpeed.Text = stats.UploadSpeedFormatted;
        TotalDownload.Text = $"Total: {NetworkStats.FormatBytes(stats.TotalBytesReceived)}";
        TotalUpload.Text = $"Total: {NetworkStats.FormatBytes(stats.TotalBytesSent)}";
        LastUpdate.Text = $"Last update: {stats.Timestamp:HH:mm:ss}";
    }

    private void MinimizeToTray_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Minimize to tray instead of closing
        e.Cancel = true;
        Hide();
    }
}