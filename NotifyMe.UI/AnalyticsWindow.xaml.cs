using System.Collections.ObjectModel;
using System.Windows;
using NotifyMe.Core.Services;
using NotifyMe.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace NotifyMe.UI;

public partial class AnalyticsWindow : Window
{
    private readonly DataLogger? _dataLogger;
    private DateTime _startDate;
    private DateTime _endDate;

    public ISeries[] UsageTimeSeries { get; set; }
    public ISeries[] UploadDownloadSeries { get; set; }
    public ISeries[] PeakHoursSeries { get; set; }

    public AnalyticsWindow(DataLogger? dataLogger)
    {
        InitializeComponent();
        
        _dataLogger = dataLogger;
        _startDate = DateTime.Today;
        _endDate = DateTime.Now;

        // Initialize charts
        InitializeCharts();
        
        // Load data after window is fully loaded
        Loaded += (s, e) => LoadAnalytics();
    }

    private void InitializeCharts()
    {
        // Usage Over Time Chart
        UsageTimeSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Name = "Download",
                Values = new ObservableCollection<double>(),
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 3 },
                GeometrySize = 0
            },
            new LineSeries<double>
            {
                Name = "Upload",
                Values = new ObservableCollection<double>(),
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.LimeGreen) { StrokeThickness = 3 },
                GeometrySize = 0
            }
        };
        UsageChart.Series = UsageTimeSeries;

        // Upload vs Download Pie Chart
        UploadDownloadSeries = new ISeries[]
        {
            new PieSeries<double>
            {
                Name = "Download",
                Values = new ObservableCollection<double> { 1 },
                Fill = new SolidColorPaint(SKColors.DeepSkyBlue)
            },
            new PieSeries<double>
            {
                Name = "Upload",
                Values = new ObservableCollection<double> { 1 },
                Fill = new SolidColorPaint(SKColors.LimeGreen)
            }
        };
        UploadDownloadPie.Series = UploadDownloadSeries;

        // Peak Hours Chart
        PeakHoursSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "Traffic",
                Values = new ObservableCollection<double>(),
                Fill = new SolidColorPaint(SKColors.Orange)
            }
        };
        PeakHoursChart.Series = PeakHoursSeries;
    }

    private void LoadAnalytics()
    {
        if (_dataLogger == null)
        {
            ShowPlaceholderData();
            return;
        }

        try
        {
            var logs = _dataLogger.GetLogs(_startDate, _endDate);
            
            if (!logs.Any())
            {
                ShowPlaceholderData();
                return;
            }

            // Calculate and display statistics
            CalculateStatistics(logs);
            
            // Update charts
            UpdateCharts(logs);
        }
        catch (Exception ex)
        {
            ShowPlaceholderData();
            System.Diagnostics.Debug.WriteLine($"Analytics error: {ex.Message}");
        }
    }

    private void CalculateStatistics(List<NetworkLog> logs)
    {
        // Calculate approximate total data (speed * time interval estimate)
        var avgInterval = 5.0; // Assuming ~5 seconds between logs
        var totalDownload = logs.Sum(l => l.DownloadSpeedBps * avgInterval);
        var totalUpload = logs.Sum(l => l.UploadSpeedBps * avgInterval);
        var totalBytes = (long)(totalDownload + totalUpload);
        
        // Total Usage
        TotalUsageText.Text = NetworkStats.FormatBytes(totalBytes);
        TotalUsageTrend.Text = $"{logs.Count} data points";
        
        // Average Speed
        var avgSpeed = logs.Any() ? 
            logs.Average(l => l.DownloadSpeedBps + l.UploadSpeedBps) : 0;
        AvgSpeedText.Text = NetworkStats.FormatBytes((long)avgSpeed) + "/s";
        AvgSpeedTrend.Text = "Average combined speed";
        
        // Peak Speed
        var peakSpeed = logs.Any() ? 
            logs.Max(l => l.DownloadSpeedBps + l.UploadSpeedBps) : 0;
        PeakSpeedText.Text = NetworkStats.FormatBytes((long)peakSpeed) + "/s";
        
        var peakRecord = logs.FirstOrDefault(l => 
            (l.DownloadSpeedBps + l.UploadSpeedBps) == peakSpeed);
        PeakSpeedTime.Text = peakRecord != null ? peakRecord.Timestamp.ToString("HH:mm") : "Unknown";
        
        // Active Time
        var timeSpan = _endDate - _startDate;
        var hours = (int)timeSpan.TotalHours;
        var minutes = (int)(timeSpan.TotalMinutes % 60);
        ActiveTimeText.Text = $"{hours}h {minutes}m";
        ActiveTimeTrend.Text = $"{_startDate:MMM dd} - {_endDate:MMM dd}";

        // Update pie chart legend
        DownloadLegend.Text = $"Download: {NetworkStats.FormatBytes((long)totalDownload)}";
        UploadLegend.Text = $"Upload: {NetworkStats.FormatBytes((long)totalUpload)}";
    }

    private void UpdateCharts(List<NetworkLog> logs)
    {
        // Calculate approximate totals
        var avgInterval = 5.0;
        var totalDownload = logs.Sum(l => l.DownloadSpeedBps * avgInterval);
        var totalUpload = logs.Sum(l => l.UploadSpeedBps * avgInterval);

        var downloadPie = (PieSeries<double>)UploadDownloadSeries[0];
        var uploadPie = (PieSeries<double>)UploadDownloadSeries[1];
        
        ((ObservableCollection<double>)downloadPie.Values!).Clear();
        ((ObservableCollection<double>)downloadPie.Values!).Add(totalDownload / (1024.0 * 1024.0));
        
        ((ObservableCollection<double>)uploadPie.Values!).Clear();
        ((ObservableCollection<double>)uploadPie.Values!).Add(totalUpload / (1024.0 * 1024.0));

        // Update Usage Over Time
        var downloadLine = (LineSeries<double>)UsageTimeSeries[0];
        var uploadLine = (LineSeries<double>)UsageTimeSeries[1];
        
        var downloadValues = (ObservableCollection<double>)downloadLine.Values!;
        var uploadValues = (ObservableCollection<double>)uploadLine.Values!;
        
        downloadValues.Clear();
        uploadValues.Clear();

        // Group by hour
        var hourlyData = logs
            .GroupBy(l => new DateTime(l.Timestamp.Year, l.Timestamp.Month, l.Timestamp.Day, l.Timestamp.Hour, 0, 0))
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var hourGroup in hourlyData)
        {
            var downloadMB = hourGroup.Sum(l => l.DownloadSpeedBps * avgInterval) / (1024.0 * 1024.0);
            var uploadMB = hourGroup.Sum(l => l.UploadSpeedBps * avgInterval) / (1024.0 * 1024.0);
            
            downloadValues.Add(downloadMB);
            uploadValues.Add(uploadMB);
        }

        // Update Peak Hours
        var peakValues = (ObservableCollection<double>)((ColumnSeries<double>)PeakHoursSeries[0]).Values!;
        peakValues.Clear();

        var hourlyTraffic = logs
            .GroupBy(l => l.Timestamp.Hour)
            .ToDictionary(g => g.Key, g => g.Sum(l => (l.DownloadSpeedBps + l.UploadSpeedBps) * avgInterval) / (1024.0 * 1024.0));

        for (int hour = 0; hour < 24; hour++)
        {
            var totalMB = hourlyTraffic.ContainsKey(hour) ? hourlyTraffic[hour] : 0;
            peakValues.Add(totalMB);
        }
    }

    private void ShowPlaceholderData()
    {
        if (TotalUsageText == null) return; // Not loaded yet
        
        TotalUsageText.Text = "No data";
        TotalUsageTrend.Text = "Start monitoring";
        AvgSpeedText.Text = "No data";
        AvgSpeedTrend.Text = "Waiting...";
        PeakSpeedText.Text = "No data";
        PeakSpeedTime.Text = "N/A";
        ActiveTimeText.Text = "0h 0m";
        ActiveTimeTrend.Text = "No period";
        DownloadLegend.Text = "Download: No data";
        UploadLegend.Text = "Upload: No data";
    }

    private void TimeRange_Changed(object sender, System.Windows.RoutedEventArgs e)
    {
        if (RadioToday.IsChecked == true)
        {
            _startDate = DateTime.Today;
            _endDate = DateTime.Now;
        }
        else if (RadioWeek.IsChecked == true)
        {
            _startDate = DateTime.Today.AddDays(-7);
            _endDate = DateTime.Now;
        }
        else if (RadioMonth.IsChecked == true)
        {
            _startDate = DateTime.Today.AddMonths(-1);
            _endDate = DateTime.Now;
        }

        LoadAnalytics();
    }
}
