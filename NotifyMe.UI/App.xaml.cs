using System.Windows;
using System.IO;
using Hardcodet.Wpf.TaskbarNotification;
using NotifyMe.Core.Services;
using NotifyMe.Models;
using NotifyMe.UI.Services;

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
    private AutoStartService? _autoStartService;
    private DataLogger? _dataLogger;
    private ProcessMonitorService? _processMonitor;
    private ApplicationsWindow? _applicationsWindow;
    private AnalyticsWindow? _analyticsWindow;
    private readonly AppSettings _settings = new();

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Setup Global Exception Handling
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            LogCrash((Exception)args.ExceptionObject, "AppDomain.UnhandledException");

        DispatcherUnhandledException += (s, args) =>
        {
            LogCrash(args.Exception, "DispatcherUnhandledException");
            args.Handled = true; // Prevent immediate crash if possible
        };

        TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            LogCrash(args.Exception, "TaskScheduler.UnobservedTaskException");
            args.SetObserved();
        };

        try
        {
            InitializeApp();
        }
        catch (Exception ex)
        {
            LogCrash(ex, "Startup Exception");
            MessageBox.Show($"Startup Error: {ex.Message}\nSee crash_log.txt for details.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void LogCrash(Exception ex, string source)
    {
        try
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
            string message = $"[{DateTime.Now}] {source}:\n{ex}\n\n--------------------------------\n\n";
            File.AppendAllText(logPath, message);
        }
        catch { /* Ignore logging errors */ }
    }

    private void InitializeApp()
    {
        // Initialize system tray icon
        _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

        // Initialize services
        var networkInterfaceWrapper = new NetworkInterfaceWrapper();
        var pingWrapper = new PingWrapper();
        
        _settingsService = new SettingsService();
        _soundService = new SoundService();
        _autoStartService = new AutoStartService();
        
        // Sync auto-start setting with registry state
        var currentSettings = _settingsService.CurrentSettings;
        var actualState = _autoStartService.IsEnabled();
        if (currentSettings.StartWithWindows != actualState)
        {
            currentSettings.StartWithWindows = actualState;
            _settingsService.SaveSettings(currentSettings);
        }

        // Apply saved language
        Helpers.LocalizationManager.LoadLanguage(currentSettings.Language);
        
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

        _notificationService = new NotificationService(_settingsService);
        _networkMonitor = new NetworkMonitor(networkInterfaceWrapper, pingWrapper) { CheckIntervalSeconds = _settings.CheckIntervalSeconds };
        _trafficMonitor = new TrafficMonitor(networkInterfaceWrapper) { UpdateIntervalMs = _settings.TrafficUpdateIntervalMs };

        // Subscribe to events
        _networkMonitor.ConnectionStateChanged += OnConnectionStateChanged;
        _networkMonitor.LatencyChanged += OnLatencyChanged;
        _trafficMonitor.TrafficUpdated += OnTrafficUpdated;
        
        // Subscribe to settings changes
        _settingsService.SettingsChanged += OnSettingsChanged;

        // Start monitoring
        _networkMonitor.Start();
        _trafficMonitor.Start();
        
        // Initialize and start process monitor
        _processMonitor = new ProcessMonitorService();
        _processMonitor.Start();
        
        // Apply sound settings
        if (_soundService != null)
        {
            _soundService.IsEnabled = _settingsService.CurrentSettings.EnableSoundNotifications;
        }
        
        // Show floating network usage icon
        _floatingIcon = new FloatingIconWindow(_networkMonitor, _trafficMonitor, _settingsService, ShowMainWindow, ShowSettingsWindow);
        _floatingIcon.Show();

        // DEBUG: Show Main Window at startup
        // ShowMainWindow();

        // Update tray icon tooltip and menu translations
        UpdateTrayIcon();
        UpdateContextMenuTranslations();
    }

    private void OnSettingsChanged(object? sender, UserSettings settings)
    {
        if (_soundService != null)
        {
            _soundService.IsEnabled = settings.EnableSoundNotifications;
        }
        
        // Update network monitor interval if needed
        if (_networkMonitor != null)
        {
             _networkMonitor.CheckIntervalSeconds = settings.UpdateIntervalSeconds;
        }
        
        // Update language if changed
        Helpers.LocalizationManager.LoadLanguage(settings.Language);
        UpdateContextMenuTranslations();
        UpdateTrayIcon();
    }
    
    private void UpdateContextMenuTranslations()
    {
        var lang = Helpers.LocalizationManager.Current;
        
        // Get the taskbar icon
        var notifyIcon = this.FindResource("NotifyIcon") as Hardcodet.Wpf.TaskbarNotification.TaskbarIcon;
        if (notifyIcon?.ContextMenu != null)
        {
            // Update each menu item
            foreach (var item in notifyIcon.ContextMenu.Items)
            {
                if (item is System.Windows.Controls.MenuItem menuItem)
                {
                    // Use Tag to identify which translation to use
                    switch (menuItem.Tag?.ToString())
                    {
                        case "ShowStatistics":
                            menuItem.Header = lang.ContextMenu.ShowStatistics;
                            break;
                        case "ShowWidget":
                            menuItem.Header = lang.ContextMenu.ShowWidget;
                            break;
                        case "ViewHistory":
                            menuItem.Header = lang.ContextMenu.ViewHistory;
                            break;
                        case "Settings":
                            menuItem.Header = lang.ContextMenu.Settings;
                            break;
                        case "ConnectionStatus":
                            menuItem.Header = lang.ContextMenu.ConnectionStatus;
                            break;
                        case "TrafficMonitor":
                            menuItem.Header = lang.ContextMenu.TrafficMonitor;
                            break;
                        case "Exit":
                            menuItem.Header = lang.ContextMenu.Exit;
                            break;
                    }
                }
            }
        }
    }

    private void OnConnectionStateChanged(object? sender, ConnectionEvent e)
    {
        Dispatcher.Invoke(() =>
        {
            bool isDnd = _notificationService?.IsDNDActive() ?? false;

            if (e.EventType == ConnectionEventType.Disconnected)
            {
                // Play sound if enabled (Bypassing DND for critical connection alerts)
                if (_soundService != null && _settingsService != null)
                {
                    _soundService.IsEnabled = _settingsService.CurrentSettings.EnableSoundNotifications;
                    _soundService.PlayConnectionLost();
                }
                
                // Show notification (Handles DND internally)
                _notificationService?.ShowConnectionLost();
            }
            else if (e.EventType == ConnectionEventType.Connected)
            {
                // Play sound if enabled (Bypassing DND for critical connection alerts)
                if (_soundService != null && _settingsService != null)
                {
                    _soundService.IsEnabled = _settingsService.CurrentSettings.EnableSoundNotifications;
                    _soundService.PlayConnectionRestored();
                }
                
                // Show notification (Handles DND internally)
                _notificationService?.ShowConnectionRestored();
            }

            UpdateTrayIcon();
        });
    }
    
    private long _lastLatency = -1;
    private DateTime _lastHighTrafficAlert = DateTime.MinValue;
    
    private void OnTrafficUpdated(object? sender, NetworkStats stats)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateTrayIcon(stats);
            
            // Log to database
            var isConnected = _networkMonitor?.IsConnected ?? false;
            _dataLogger?.LogNetworkStats(isConnected, stats.DownloadSpeedBytesPerSecond, stats.UploadSpeedBytesPerSecond, _lastLatency);
            
            // Check for high traffic alert
            CheckHighTrafficAlert(stats);
        });
    }
    
    private void CheckHighTrafficAlert(NetworkStats stats)
    {
        var settings = _settingsService?.CurrentSettings;
        if (settings == null)
        {
            System.Diagnostics.Debug.WriteLine("Settings is null");
            return;
        }
        
        // Convert download speed from Bytes/s to MB/s
        double speedMBps = stats.DownloadSpeedBytesPerSecond / (1024.0 * 1024.0);
        
        // Convert threshold to MB/s based on unit
        double thresholdMBps = settings.HighTrafficThresholdUnit?.ToUpper() switch
        {
            "KB" => settings.HighTrafficThresholdMBps / 1024.0,
            "GB" => settings.HighTrafficThresholdMBps * 1024.0,
            _ => settings.HighTrafficThresholdMBps
        };
        
        System.Diagnostics.Debug.WriteLine($"Traffic Check - Speed: {speedMBps:F2} MB/s, Threshold: {thresholdMBps:F2} MB/s ({settings.HighTrafficThresholdMBps} {settings.HighTrafficThresholdUnit})");
        
        // Check if threshold exceeded (with throttling - only notify once per 5 minutes)
        if (speedMBps > thresholdMBps)
        {
            var now = DateTime.Now;
            var timeSinceLastAlert = (now - _lastHighTrafficAlert).TotalMinutes;
            System.Diagnostics.Debug.WriteLine($"THRESHOLD EXCEEDED! Time since last alert: {timeSinceLastAlert:F1} minutes");
            
            if (timeSinceLastAlert >= 0.1) // 6 seconds cooldown for testing
            {
                System.Diagnostics.Debug.WriteLine("Showing high traffic alert...");
                _notificationService?.ShowHighTrafficAlert(
                    speedMBps,
                    settings.HighTrafficThresholdUnit ?? "MB",
                    settings.HighTrafficThresholdMBps
                );
                
                // Play warning sound
                if (settings.EnableSoundNotifications && !(_notificationService?.IsDNDActive() ?? false))
                {
                    System.Media.SystemSounds.Exclamation.Play();
                }

                _lastHighTrafficAlert = now;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Skipping alert (throttled) - wait {5 - timeSinceLastAlert:F1} more minutes");
            }
        }
    }
    
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

    public void ShowSettingsWindow()
    {
        try
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new SettingsWindow(_settingsService!, _autoStartService!, _notificationService!, _soundService!);
                _settingsWindow.Closed += (s, e) => _settingsWindow = null;
            }
            _settingsWindow.Show();
            _settingsWindow.Activate();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Settings Window: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void ShowMainWindow()
    {
        try
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
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Main Window: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void ShowApplicationsWindow()
    {
        try
        {
            if (_processMonitor == null)
            {
                MessageBox.Show("Process Monitor is not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_applicationsWindow == null)
            {
                _applicationsWindow = new ApplicationsWindow(_processMonitor);
                _applicationsWindow.Closed += (s, e) => _applicationsWindow = null;
            }

            _applicationsWindow.Show();
            _applicationsWindow.WindowState = WindowState.Normal;
            _applicationsWindow.Activate();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Applications Window: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void ShowAnalyticsWindow()
    {
        try
        {
            if (_analyticsWindow == null)
            {
                _analyticsWindow = new AnalyticsWindow(_dataLogger);
                _analyticsWindow.Closed += (s, e) => _analyticsWindow = null;
            }

            _analyticsWindow.Show();
            _analyticsWindow.WindowState = WindowState.Normal;
            _analyticsWindow.Activate();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Analytics Window: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
    
    public void ShowHistoryWindow()
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

