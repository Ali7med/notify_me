using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using NotifyMe.Core.Services;
using NotifyMe.Models;

namespace NotifyMe.UI;

public partial class HistoryWindow : Window
{
    private readonly DataLogger _dataLogger;

    public HistoryWindow(DataLogger dataLogger)
    {
        InitializeComponent();
        _dataLogger = dataLogger;
        
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            var range = GetDateRange();
            var logs = _dataLogger.GetLogs(range.from, range.to);
            
            if (!logs.Any())
            {
                ShowNoDataMessage();
                return;
            }

            // Calculate statistics
            CalculateStatistics(logs);
            
            // Convert to display models
            var displayData = logs.Select(log => new NetworkLogDisplay
            {
                Timestamp = log.Timestamp,
                StatusText = log.IsConnected ? "Connected" : "Disconnected",
                StatusColor = log.IsConnected ? Brushes.LimeGreen : Brushes.Red,
                DownloadSpeedFormatted = FormatSpeed(log.DownloadSpeedBps),
                UploadSpeedFormatted = FormatSpeed(log.UploadSpeedBps),
                LatencyText = log.Latency < 0 ? "TIMEOUT" : $"{log.Latency} ms"
            }).ToList();

            HistoryDataGrid.ItemsSource = displayData;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading history: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CalculateStatistics(List<NetworkLog> logs)
    {
        RecordCountText.Text = logs.Count.ToString();
        
        var avgDownload = logs.Average(l => l.DownloadSpeedBps);
        AvgDownloadText.Text = FormatSpeed(avgDownload);
        
        var avgUpload = logs.Average(l => l.UploadSpeedBps);
        AvgUploadText.Text = FormatSpeed(avgUpload);
        
        var connectedLogs = logs.Where(l => l.IsConnected && l.Latency >= 0).ToList();
        var avgPing = connectedLogs.Any() ? connectedLogs.Average(l => l.Latency) : 0;
        AvgPingText.Text = $"{avgPing:F0} ms";
    }

    private void ShowNoDataMessage()
    {
        RecordCountText.Text = "0";
        AvgDownloadText.Text = "No data";
        AvgUploadText.Text = "No data";
        AvgPingText.Text = "No data";
        HistoryDataGrid.ItemsSource = null;
    }

    private (DateTime from, DateTime to) GetDateRange()
    {
        var to = DateTime.Now;
        var from = to;

        if (DateRangeComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            switch (selectedItem.Tag?.ToString())
            {
                case "LastHour": 
                    from = to.AddHours(-1); 
                    break;
                case "Last24Hours": 
                    from = to.AddDays(-1); 
                    break;
                case "Last7Days": 
                    from = to.AddDays(-7); 
                    break;
                case "Last30Days": 
                    from = to.AddDays(-30); 
                    break;
                case "AllTime": 
                    from = DateTime.MinValue; 
                    break;
            }
        }

        return (from, to);
    }

    private string FormatSpeed(double bytesPerSecond)
    {
        if (bytesPerSecond < 1024) 
            return $"{bytesPerSecond:F0} B/s";
        if (bytesPerSecond < 1024 * 1024) 
            return $"{bytesPerSecond / 1024.0:F1} KB/s";
        return $"{bytesPerSecond / (1024.0 * 1024.0):F2} MB/s";
    }

    private void DateRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (HistoryDataGrid != null)
        {
            LoadData();
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadData();
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                FileName = $"NetworkHistory_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var range = GetDateRange();
                var logs = _dataLogger.GetLogs(range.from, range.to);
                
                var csv = new StringBuilder();
                csv.AppendLine("Timestamp,Status,Download (B/s),Upload (B/s),Latency (ms)");
                
                foreach (var log in logs)
                {
                    csv.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                                 $"{(log.IsConnected ? "Connected" : "Disconnected")}," +
                                 $"{log.DownloadSpeedBps}," +
                                 $"{log.UploadSpeedBps}," +
                                 $"{log.Latency}");
                }
                
                File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);
                
                MessageBox.Show($"Data exported successfully to:\n{saveDialog.FileName}", 
                    "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ClearOldLogs_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to delete logs older than 30 days?\n\nThis action cannot be undone.", 
            "Confirm Delete", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Warning);
        
        if (result == MessageBoxResult.Yes)
        {
            _dataLogger.ClearOldLogs(30);
            LoadData();
            MessageBox.Show("Old logs cleared successfully.", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ClearAllHistory_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "⚠️ WARNING ⚠️\n\nThis will permanently delete ALL network history data!\n\nAre you absolutely sure you want to continue?", 
            "Delete All History", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Warning);
        
        if (result == MessageBoxResult.Yes)
        {
            // Double confirmation for safety
            var confirmResult = MessageBox.Show(
                "This is your last chance!\n\nAll historical data will be lost forever.\n\nType 'YES' to confirm deletion.", 
                "Final Confirmation", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Stop);
            
            if (confirmResult == MessageBoxResult.Yes)
            {
                try
                {
                    // Clear all logs (0 days means delete everything)
                    _dataLogger.ClearOldLogs(0);
                    LoadData();
                    MessageBox.Show("All history has been cleared.", "Completed", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing history: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

// Display model for DataGrid
public class NetworkLogDisplay
{
    public DateTime Timestamp { get; set; }
    public string StatusText { get; set; } = "";
    public Brush StatusColor { get; set; } = Brushes.Gray;
    public string DownloadSpeedFormatted { get; set; } = "";
    public string UploadSpeedFormatted { get; set; } = "";
    public string LatencyText { get; set; } = "";
}
