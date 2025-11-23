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
    private SettingsWindow? _settingsWindow;
    private HistoryWindow? _historyWindow;
    private SettingsService? _settingsService;
    private SoundService? _soundService;
    private DataLogger? _dataLogger;
    private readonly AppSettings _settings = new();

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Initialize system tray icon
        _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

        // Initialize services
        var networkInterfaceWrapper = new NetworkInterfaceWrapper();
        var pingWrapper = new PingWrapper();
        
        _settingsService = new SettingsService();
        _soundService = new SoundService();
        
        // Initialize DataLogger with error handling
        try
        {
            _dataLogger = new DataLogger();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize DataLogger: {ex.Message}");
            // Continue without logging - non-critical feature
        }

        _notificationService = new NotificationService(_settings);
        _networkMonitor = new NetworkMonitor(networkInterfaceWrapper, pingWrapper) { CheckIntervalSeconds = _settings.CheckIntervalSeconds };
        _trafficMonitor = new TrafficMonitor(networkInterfaceWrapper) { UpdateIntervalMs = _settings.TrafficUpdateIntervalMs };

        // Subscribe to events
        _networkMonitor.ConnectionStateChanged += OnConnectionStateChanged;
        _networkMonitor.LatencyChanged += OnLatencyChanged;
        _trafficMonitor.TrafficUpdated += OnTrafficUpdated;

        // Start monitoring
        _networkMonitor.Start();
        _trafficMonitor.Start();
        
        // Apply sound settings
        if (_soundService != null)
        {
            _soundService.IsEnabled = _settingsService.CurrentSettings.EnableSoundNotifications;
        }
        
        // Show floating network usage icon
        _floatingIcon = new FloatingIconWindow(_networkMonitor, _trafficMonitor, _settingsService, ShowMainWindow, ShowSettingsWindow);
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
                _soundService?.PlayConnectionLost();
            }
            else if (e.EventType == ConnectionEventType.Connected)
            {
                _notificationService?.ShowConnectionRestored();
                _soundService?.PlayConnectionRestored();
            }

            UpdateTrayIcon();
        });
    }

    private void OnTrafficUpdated(object? sender, NetworkStats stats)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateTrayIcon(stats);
            
            // Log to database
            var isConnected = _networkMonitor?.IsConnected ?? false;
            _dataLogger?.LogNetworkStats(isConnected, stats.DownloadSpeedBytesPerSecond, stats.UploadSpeedBytesPerSecond, _lastLatency);
        });
    }
    
    private long _lastLatency = -1;
    private void OnLatencyChanged(object? sender, long latency)
    {
        _lastLatency = latency;
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
             _floatingIcon = new FloatingIconWindow(_networkMonitor!, _trafficMonitor!, _settingsService!, ShowMainWindow, ShowSettingsWindow);
        }
        _floatingIcon.Show();
        _floatingIcon.Activate();
    }

    private void ShowSettings_Click(object sender, RoutedEventArgs e)
    {
        ShowSettingsWindow();
    }

    private void ShowSettingsWindow()
    {
        if (_settingsWindow == null)
        {
            _settingsWindow = new SettingsWindow(_settingsService!);
            _settingsWindow.Closed += (s, e) => _settingsWindow = null;
        }
        _settingsWindow.Show();
        _settingsWindow.Activate();
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
    
    private void ShowHistory_Click(object sender, RoutedEventArgs e)
    {
        ShowHistoryWindow();
    }
    
    private void ShowHistoryWindow()
    {
        if (_historyWindow == null || !_historyWindow.IsLoaded)
        {
            if (_dataLogger != null)
            {
                _historyWindow = new HistoryWindow(_dataLogger);
                _historyWindow.Show();
            }
            else
            {
                MessageBox.Show("Data logging is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        else
        {
            _historyWindow.WindowState = WindowState.Normal;
            _historyWindow.Activate();
        }
    }
}

