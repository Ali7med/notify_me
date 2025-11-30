using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using NotifyMe.Core.Services;
using NotifyMe.Models;
using SkiaSharp;

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

        // Chart Properties
        private readonly ObservableCollection<double> _downloadValues;
        private readonly ObservableCollection<double> _uploadValues;
        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        public FloatingIconWindow(NetworkMonitor networkMonitor, TrafficMonitor trafficMonitor, SettingsService settingsService, Action openAppAction, Action openSettingsAction)
        {
            InitializeComponent();
            _networkMonitor = networkMonitor;
            _trafficMonitor = trafficMonitor;
            _settingsService = settingsService;
            _openAppAction = openAppAction;
            _openSettingsAction = openSettingsAction;

            // Initialize Chart Data
            _downloadValues = new ObservableCollection<double>(Enumerable.Repeat(0.0, 20));
            _uploadValues = new ObservableCollection<double>(Enumerable.Repeat(0.0, 20));

            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = _downloadValues,
                    Fill = new SolidColorPaint(SKColors.DodgerBlue.WithAlpha(50)),
                    Stroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 1 },
                    GeometrySize = 0,
                    LineSmoothness = 1
                },
                new LineSeries<double>
                {
                    Values = _uploadValues,
                    Fill = new SolidColorPaint(SKColors.Gold.WithAlpha(50)),
                    Stroke = new SolidColorPaint(SKColors.Gold) { StrokeThickness = 1 },
                    GeometrySize = 0,
                    LineSmoothness = 1
                }
            };

            XAxes = new Axis[] { new Axis { IsVisible = false } };
            YAxes = new Axis[] { new Axis { IsVisible = false } };

            MiniChart.Series = Series;
            MiniChart.XAxes = XAxes;
            MiniChart.YAxes = YAxes;

            // Apply initial settings
            ApplySettings(_settingsService.CurrentSettings);
            _settingsService.SettingsChanged += (s, settings) => Dispatcher.Invoke(() => ApplySettings(settings));

            _networkMonitor.LatencyChanged += OnLatencyChanged;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(_settingsService.CurrentSettings.UpdateIntervalSeconds) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            // Apply context menu translations
            UpdateContextMenuTranslations();
            Helpers.LocalizationManager.LanguageChanged += (s, e) => Dispatcher.Invoke(UpdateContextMenuTranslations);
        }
        
        private void UpdateContextMenuTranslations()
        {
            var lang = Helpers.LocalizationManager.Current;
            
            // Access the Grid's ContextMenu
            var grid = this.Content as System.Windows.Controls.Grid;
            if (grid?.ContextMenu != null)
            {
                foreach (var item in grid.ContextMenu.Items)
                {
                    if (item is System.Windows.Controls.MenuItem menuItem)
                    {
                        switch (menuItem.Tag?.ToString())
                        {
                            case "OpenNotifyMe":
                                menuItem.Header = lang.FloatingWidget.OpenNotifyMe;
                                break;
                            case "ShowMiniChart":
                                menuItem.Header = "Show Mini Chart"; // TODO: Add to strings
                                break;
                            case "Settings":
                                menuItem.Header = lang.FloatingWidget.Settings;
                                break;
                            case "HideWidget":
                                menuItem.Header = lang.FloatingWidget.HideWidget;
                                break;
                            case "ExitApplication":
                                menuItem.Header = lang.FloatingWidget.ExitApplication;
                                break;
                        }
                    }
                }
            }
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
            
            // Update Mini Chart Visibility and Menu State
            MiniChart.Visibility = settings.ShowMiniChart ? Visibility.Visible : Visibility.Collapsed;
            if (MenuShowMiniChart != null)
            {
                MenuShowMiniChart.IsChecked = settings.ShowMiniChart;
            }
            
            // Apply Widget Shape
            ApplyShape(settings.FloatingWidgetShape);
            
            // Update timer interval (only if timer is already initialized)
            if (_timer != null)
            {
                _timer.Interval = TimeSpan.FromSeconds(settings.UpdateIntervalSeconds);
            }
        }

        private void ApplyShape(string shape)
        {
            // Default to Pill if null
            if (string.IsNullOrEmpty(shape)) shape = "Pill";

            switch (shape)
            {
                case "Square":
                    Width = 150;
                    Height = 70;
                    MainBorder.CornerRadius = new CornerRadius(4);
                    GlowBorder.CornerRadius = new CornerRadius(4);
                    NormalViewGrid.Visibility = Visibility.Visible;
                    CompactViewGrid.Visibility = Visibility.Collapsed;
                    StatusBar.Visibility = Visibility.Visible;
                    break;

                case "Circle":
                    Width = 80;
                    Height = 80;
                    MainBorder.CornerRadius = new CornerRadius(40);
                    GlowBorder.CornerRadius = new CornerRadius(40);
                    NormalViewGrid.Visibility = Visibility.Collapsed;
                    CompactViewGrid.Visibility = Visibility.Visible;
                    StatusBar.Visibility = Visibility.Collapsed;
                    break;

                case "Pill":
                default:
                    Width = 180;
                    Height = 70;
                    MainBorder.CornerRadius = new CornerRadius(10);
                    GlowBorder.CornerRadius = new CornerRadius(10);
                    NormalViewGrid.Visibility = Visibility.Visible;
                    CompactViewGrid.Visibility = Visibility.Collapsed;
                    StatusBar.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void OnLatencyChanged(object? sender, long latency)
        {
            Dispatcher.Invoke(() =>
            {
                if (latency < 0)
                {
                    // Timeout or error
                    PingText.Text = "TIMEOUT";
                    PingText.Foreground = new SolidColorBrush(Color.FromRgb(255, 76, 76)); // Red
                }
                else
                {
                    PingText.Text = $"{latency} ms";
                    
                    // Color code latency
                    if (latency < 50) PingText.Foreground = new SolidColorBrush(Color.FromRgb(76, 255, 76)); // Green
                    else if (latency < 150) PingText.Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)); // Yellow
                    else PingText.Foreground = new SolidColorBrush(Color.FromRgb(255, 76, 76)); // Red
                }
            });
        }

        private long _initialBytesReceived = -1;
        private long _initialBytesSent = -1;

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
                TotalSpeedText.Text = "--";
                
                TooltipNetworkName.Text = "Disconnected";
                TooltipIPAddress.Text = "N/A";
                
                // Update Chart with zero
                UpdateChart(0, 0);
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
                    
                    // Update Compact View
                    double totalSpeed = stats.DownloadSpeedBytesPerSecond + stats.UploadSpeedBytesPerSecond;
                    TotalSpeedText.Text = FormatSpeed(totalSpeed);
                    
                    // Update Tooltip
                    TooltipNetworkName.Text = stats.InterfaceName;
                    TooltipIPAddress.Text = stats.IPv4Address;
                    
                    // Initialize session counters if needed
                    if (_initialBytesReceived == -1) _initialBytesReceived = stats.TotalBytesReceived;
                    if (_initialBytesSent == -1) _initialBytesSent = stats.TotalBytesSent;
                    
                    // Calculate session usage
                    long sessionDownload = Math.Max(0, stats.TotalBytesReceived - _initialBytesReceived);
                    long sessionUpload = Math.Max(0, stats.TotalBytesSent - _initialBytesSent);
                    
                    TooltipSessionDownload.Text = NetworkStats.FormatBytes(sessionDownload);
                    TooltipSessionUpload.Text = NetworkStats.FormatBytes(sessionUpload);
                    
                    // Update Chart
                    if (MiniChart.Visibility == Visibility.Visible)
                    {
                        UpdateChart(stats.DownloadSpeedBytesPerSecond, stats.UploadSpeedBytesPerSecond);
                    }
                }
            }
        }
        
        private void UpdateChart(double download, double upload)
        {
            _downloadValues.Add(download);
            _uploadValues.Add(upload);
            
            if (_downloadValues.Count > 20) _downloadValues.RemoveAt(0);
            if (_uploadValues.Count > 20) _uploadValues.RemoveAt(0);
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
            
            IndicatorBorderBrush.Color = color;
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

        private void MenuItem_ShowMiniChart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MenuItem menuItem)
            {
                var settings = _settingsService.CurrentSettings;
                settings.ShowMiniChart = menuItem.IsChecked;
                _settingsService.SaveSettings(settings);
                
                // Apply immediately
                ApplySettings(settings);
            }
        }

        private void MenuItem_Shape_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MenuItem menuItem && menuItem.Tag is string tag)
            {
                var shape = tag.Replace("Shape_", "");
                var settings = _settingsService.CurrentSettings;
                settings.FloatingWidgetShape = shape;
                _settingsService.SaveSettings(settings);
                
                // Apply immediately
                ApplySettings(settings);
            }
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
            var settings = _settingsService.CurrentSettings;
            var workArea = SystemParameters.WorkArea;
            
            // Use saved position if available, otherwise default to bottom-right
            if (settings.FloatingWidgetLeft >= 0 && settings.FloatingWidgetTop >= 0)
            {
                // Ensure position is still within screen bounds
                Left = Math.Max(0, Math.Min(settings.FloatingWidgetLeft, workArea.Right - Width));
                Top = Math.Max(0, Math.Min(settings.FloatingWidgetTop, workArea.Bottom - Height));
            }
            else
            {
                // Default position: bottom-right
                Left = workArea.Right - Width - 20;
                Top = workArea.Bottom - Height - 20;
            }
        }
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Save current position
            var settings = _settingsService.CurrentSettings;
            settings.FloatingWidgetLeft = Left;
            settings.FloatingWidgetTop = Top;
            _settingsService.SaveSettings(settings);
            
            base.OnClosing(e);
        }
    }
}
