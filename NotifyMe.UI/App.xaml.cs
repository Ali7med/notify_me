using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using NotifyMe.Core.Services;
using NotifyMe.Models;

namespace NotifyMe.UI;

public partial class App : Application
{
    private TaskbarIcon? _notifyIcon;
    private NetworkMonitor? _networkMonitor;
    private TrafficMonitor? _trafficMonitor;
    private NotificationService? _notificationService;
    private MainWindow? _mainWindow;
        private FloatingIconWindow? _floatingIcon;
    private readonly AppSettings _settings = new();

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Initialize system tray icon
        _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

        // Initialize services
        var networkInterfaceWrapper = new NetworkInterfaceWrapper();
        var pingWrapper = new PingWrapper();

        _notificationService = new NotificationService(_settings);
        _networkMonitor = new NetworkMonitor(networkInterfaceWrapper, pingWrapper) { CheckIntervalSeconds = _settings.CheckIntervalSeconds };
        _trafficMonitor = new TrafficMonitor(networkInterfaceWrapper) { UpdateIntervalMs = _settings.TrafficUpdateIntervalMs };

        // Subscribe to events
        _networkMonitor.ConnectionStateChanged += OnConnectionStateChanged;
        _trafficMonitor.TrafficUpdated += OnTrafficUpdated;

        // Start monitoring
        _networkMonitor.Start();
        _trafficMonitor.Start();
        // Show floating network usage icon
        _floatingIcon = new FloatingIconWindow(_networkMonitor, _trafficMonitor, ShowMainWindow);
        _floatingIcon.Show();

        // Update tray icon tooltip
        UpdateTrayIcon();
    }

    private void OnConnectionStateChanged(object? sender, ConnectionEvent e)
    {
        Dispatcher.Invoke(() =>
        {
            if (e.EventType == ConnectionEventType.Disconnected)
            {
                _notificationService?.ShowConnectionLost();
            }
            else if (e.EventType == ConnectionEventType.Connected)
            {
                _notificationService?.ShowConnectionRestored();
            }

            UpdateTrayIcon();
        });
    }

    private void OnTrafficUpdated(object? sender, NetworkStats stats)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateTrayIcon(stats);
        });
    }

    private void UpdateTrayIcon(NetworkStats? stats = null)
    {
        if (_notifyIcon == null) return;

        var currentStats = stats ?? _trafficMonitor?.CurrentStats;
        var isConnected = _networkMonitor?.IsConnected ?? false;

        if (currentStats != null)
        {
            var tooltip = $"NotifyMe - Network Monitor\n" +
                         $"Status: {(isConnected ? "Connected" : "Disconnected")}\n" +
                         $"↓ {currentStats.DownloadSpeedFormatted}\n" +
                         $"↑ {currentStats.UploadSpeedFormatted}";

            _notifyIcon.ToolTipText = tooltip;
        }
        else
        {
            _notifyIcon.ToolTipText = $"NotifyMe - {(isConnected ? "Connected" : "Disconnected")}";
        }
    }

    private void ShowWindow_Click(object sender, RoutedEventArgs e)
    {
        ShowMainWindow();
    }

    private void ShowWidget_Click(object sender, RoutedEventArgs e)
    {
        if (_floatingIcon == null)
        {
             _floatingIcon = new FloatingIconWindow(_networkMonitor!, _trafficMonitor!, ShowMainWindow);
        }
        _floatingIcon.Show();
        _floatingIcon.Activate();
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null)
        {
            _mainWindow = new MainWindow(_networkMonitor, _trafficMonitor);
            _mainWindow.Closed += (s, e) => _mainWindow = null;
        }

        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Shutdown();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        _networkMonitor?.Stop();
        _trafficMonitor?.Stop();
        _notifyIcon?.Dispose();
        _floatingIcon?.Close();
    }
    private void TaskbarIcon_DoubleClick(object sender, RoutedEventArgs e)
    {
        ShowMainWindow();
    }
}

