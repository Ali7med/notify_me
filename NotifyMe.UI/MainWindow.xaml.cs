using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NotifyMe.Core.Services;
using NotifyMe.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace NotifyMe.UI;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly NetworkMonitor? _networkMonitor;
    private readonly TrafficMonitor? _trafficMonitor;
    
    // Chart Data
    private readonly ObservableCollection<double> _downloadSpeedHistory;
    private readonly ObservableCollection<double> _uploadSpeedHistory;

    private ISeries[] _series;
    public ISeries[] Series 
    { 
        get => _series; 
        set { _series = value; OnPropertyChanged(); } 
    }

    private Axis[] _xAxes;
    public Axis[] XAxes 
    { 
        get => _xAxes; 
        set { _xAxes = value; OnPropertyChanged(); } 
    }

    private Axis[] _yAxes;
    public Axis[] YAxes 
    { 
        get => _yAxes; 
        set { _yAxes = value; OnPropertyChanged(); } 
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public MainWindow(NetworkMonitor? networkMonitor, TrafficMonitor? trafficMonitor)
    {
        InitializeComponent();
        DataContext = this; // Enable Data Binding
        
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
        
        // Initialize Chart Data
        _downloadSpeedHistory = new ObservableCollection<double>(Enumerable.Repeat(0.0, 60));
        _uploadSpeedHistory = new ObservableCollection<double>(Enumerable.Repeat(0.0, 60));
        
        InitializeChart();
        
        // Configure Chart Legend
        if (NetworkChart != null)
        {
            NetworkChart.LegendTextPaint = new SolidColorPaint(SKColors.White) { SKTypeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold) };
            NetworkChart.LegendBackgroundPaint = new SolidColorPaint(new SKColor(30, 30, 30));
        }

        if (_networkMonitor != null)
        {
            _networkMonitor.ConnectionStateChanged += OnConnectionStateChanged;
            _networkMonitor.LatencyChanged += OnLatencyChanged;
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
    
    private void InitializeChart()
    {
        Series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = _downloadSpeedHistory,
                Name = "Download",
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 3 },
                GeometrySize = 0,
                GeometryStroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 3 }
            },
            new LineSeries<double>
            {
                Values = _uploadSpeedHistory,
                Name = "Upload",
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.LimeGreen) { StrokeThickness = 3 },
                GeometrySize = 0,
                GeometryStroke = new SolidColorPaint(SKColors.LimeGreen) { StrokeThickness = 3 }
            }
        };

        XAxes = new Axis[]
        {
            new Axis
            {
                Name = "Time (seconds)",
                NamePaint = new SolidColorPaint(SKColors.LightGray),
                LabelsPaint = new SolidColorPaint(SKColors.LightGray),
                SeparatorsPaint = new SolidColorPaint(new SKColor(50, 50, 50)) { StrokeThickness = 1 },
                ShowSeparatorLines = true
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                Name = "Speed (MB/s)",
                NamePaint = new SolidColorPaint(SKColors.LightGray),
                LabelsPaint = new SolidColorPaint(SKColors.LightGray),
                SeparatorsPaint = new SolidColorPaint(new SKColor(50, 50, 50)) { StrokeThickness = 1 },
                ShowSeparatorLines = true,
                MinLimit = 0
            }
        };
    }

    private void ApplyTranslations()
    {
        var lang = Helpers.LocalizationManager.CurrentLanguage;
        
        // Window Title & Header
        Title = lang.MainWindow.Title;
        if (PageTitle != null) PageTitle.Text = "Dashboard"; // TODO: Add to strings
        
        // Handle RTL/LTR
        FlowDirection = lang.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    private void OnConnectionStateChanged(object? sender, ConnectionEvent e)
    {
        Dispatcher.Invoke(() =>
        {
            // Update UI based on connection state if needed
            // Currently handled by traffic monitor updates (0 speed when disconnected)
        });
    }

    private void OnLatencyChanged(object? sender, long latency)
    {
        Dispatcher.Invoke(() =>
        {
            if (LatencyText != null)
            {
                if (latency < 0)
                {
                    LatencyText.Text = "TIMEOUT";
                    LatencyText.Foreground = Brushes.Red;
                }
                else
                {
                    LatencyText.Text = $"{latency} ms";
                    
                    if (latency < 50) LatencyText.Foreground = Brushes.LimeGreen;
                    else if (latency < 150) LatencyText.Foreground = Brushes.Orange;
                    else LatencyText.Foreground = Brushes.Red;
                }
            }
        });
    }

    private void OnTrafficUpdated(object? sender, NetworkStats stats)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateTrafficStats(stats);
            UpdateChart(stats);
        });
    }

    private void UpdateTrafficStats(NetworkStats stats)
    {
        if (DownloadSpeedText != null)
        {
            DownloadSpeedText.Text = stats.DownloadSpeedFormatted;
        }

        if (UploadSpeedText != null)
        {
            UploadSpeedText.Text = stats.UploadSpeedFormatted;
        }
        
        // Update Signal Strength based on latency (simple heuristic)
        if (SignalStrengthText != null)
        {
            if (!_networkMonitor?.IsConnected ?? true)
            {
                SignalStrengthText.Text = "Disconnected";
                SignalStrengthText.Foreground = Brushes.Red;
            }
            else
            {
                SignalStrengthText.Text = "Strong"; // Default
                var brush = TryFindResource("PrimaryHueMidBrush") as Brush;
                SignalStrengthText.Foreground = brush ?? Brushes.Purple;
            }
        }
    }
    
    private void UpdateChart(NetworkStats stats)
    {
        // Convert bytes per second to megabytes per second
        double downloadMBps = stats.DownloadSpeedBytesPerSecond / (1024.0 * 1024.0);
        double uploadMBps = stats.UploadSpeedBytesPerSecond / (1024.0 * 1024.0);
        
        _downloadSpeedHistory.Add(downloadMBps);
        _uploadSpeedHistory.Add(uploadMBps);
        
        // Keep only last 60 data points
        if (_downloadSpeedHistory.Count > 60) _downloadSpeedHistory.RemoveAt(0);
        if (_uploadSpeedHistory.Count > 60) _uploadSpeedHistory.RemoveAt(0);
    }

    private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Navigation logic
        if (NavList.SelectedItem is ListBoxItem item && item.Tag is string tag)
        {
            if (PageTitle != null) PageTitle.Text = tag;
            
            // Handle navigation
            var app = (App)Application.Current;
            
            switch (tag)
            {
                case "Applications":
                    app.ShowApplicationsWindow();
                    break;
                case "Analytics":
                    app.ShowAnalyticsWindow();
                    break;
                case "History":
                    app.ShowHistoryWindow();
                    break;
                case "Dashboard":
                default:
                    // Already on dashboard
                    break;
            }
        }
    }

    private void ShowSettings_Click(object sender, RoutedEventArgs e)
    {
        // Delegate to App's ShowSettingsWindow which has access to services
        var app = (App)Application.Current;
        app.ShowSettingsWindow();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
