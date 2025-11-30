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

        // Apply initial translations and subscribe to changes
        ApplyTranslations();
        Helpers.LocalizationManager.LanguageChanged += (s, e) => ApplyTranslations();
    }

    private void ApplyTranslations()
    {
        var lang = Helpers.LocalizationManager.CurrentLanguage;
        
        // Window Title & Header
        Title = lang.MainWindow.Title;
        if (HeaderTitle != null) HeaderTitle.Text = lang.MainWindow.HeaderTitle;
        
        // Labels
        if (LblConnectionStatus != null) LblConnectionStatus.Text = lang.MainWindow.ConnectionStatus;
        if (LblDownload != null) LblDownload.Text = lang.MainWindow.DownloadSpeed;
        if (LblUpload != null) LblUpload.Text = lang.MainWindow.UploadSpeed;
        if (BtnMinimize != null) BtnMinimize.Content = lang.MainWindow.MinimizeButton;

        // Update dynamic text if we have current state
        if (_networkMonitor != null)
            UpdateConnectionStatus(_networkMonitor.IsConnected);
            
        if (_trafficMonitor != null)
            UpdateTrafficStats(_trafficMonitor.CurrentStats);

        // Handle RTL/LTR
        FlowDirection = lang.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
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
        var lang = Helpers.LocalizationManager.CurrentLanguage;
        StatusText.Text = isConnected ? lang.MainWindow.Connected : lang.MainWindow.Disconnected;
        StatusIndicator.Fill = new SolidColorBrush(isConnected ? Colors.Lime : Colors.Red);
    }

    private void UpdateTrafficStats(NetworkStats stats)
    {
        var lang = Helpers.LocalizationManager.CurrentLanguage;
        DownloadSpeed.Text = stats.DownloadSpeedFormatted;
        UploadSpeed.Text = stats.UploadSpeedFormatted;
        TotalDownload.Text = $"{lang.MainWindow.TotalPrefix}{NetworkStats.FormatBytes(stats.TotalBytesReceived)}";
        TotalUpload.Text = $"{lang.MainWindow.TotalPrefix}{NetworkStats.FormatBytes(stats.TotalBytesSent)}";
        LastUpdate.Text = $"{lang.MainWindow.LastUpdatePrefix}{stats.Timestamp:HH:mm:ss}";
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