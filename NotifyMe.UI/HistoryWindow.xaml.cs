using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NotifyMe.Core.Services;
using NotifyMe.Models;

namespace NotifyMe.UI
{
    public partial class HistoryWindow : Window
    {
        private readonly DataLogger _dataLogger;

        public HistoryWindow(DataLogger dataLogger)
        {
            InitializeComponent();
            _dataLogger = dataLogger;
            
            // Apply initial translations and subscribe to changes
            ApplyTranslations();
            Helpers.LocalizationManager.LanguageChanged += (s, e) => 
            {
                ApplyTranslations();
                LoadData(); // Reload data to update localized status text
            };
            
            LoadData();
        }

        private void ApplyTranslations()
        {
            var lang = Helpers.LocalizationManager.CurrentLanguage;
            
            // Window Title & Header
            Title = lang.History.Title;
            if (HeaderTitle != null) HeaderTitle.Text = lang.History.HeaderTitle;
            
            // Labels & Buttons
            if (LblDateRange != null) LblDateRange.Text = lang.History.DateRange;
            if (BtnRefresh != null) BtnRefresh.Content = lang.History.Refresh;
            if (BtnClear != null) BtnClear.Content = lang.History.ClearLogs;

            // DataGrid Columns
            if (ColTime != null) ColTime.Header = lang.History.ColTime;
            if (ColDate != null) ColDate.Header = lang.History.ColDate;
            if (ColStatus != null) ColStatus.Header = lang.History.ColStatus;
            if (ColDownload != null) ColDownload.Header = lang.History.ColDownload;
            if (ColUpload != null) ColUpload.Header = lang.History.ColUpload;
            if (ColPing != null) ColPing.Header = lang.History.ColPing;

            // ComboBox Items
            if (DateRangeComboBox != null)
            {
                foreach (ComboBoxItem item in DateRangeComboBox.Items)
                {
                    switch (item.Tag?.ToString())
                    {
                        case "LastHour": item.Content = lang.History.RangeLastHour; break;
                        case "Last24Hours": item.Content = lang.History.RangeLast24Hours; break;
                        case "Last7Days": item.Content = lang.History.RangeLast7Days; break;
                        case "Last30Days": item.Content = lang.History.RangeLast30Days; break;
                        case "AllTime": item.Content = lang.History.RangeAllTime; break;
                    }
                }
            }

            // Handle RTL/LTR
            FlowDirection = lang.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        private void LoadData()
        {
            try
            {
                var lang = Helpers.LocalizationManager.CurrentLanguage;
                var range = GetDateRange();
                var logs = _dataLogger.GetLogs(range.from, range.to);
                
                // Convert to display models
                var displayData = logs.Select(log => new NetworkLogDisplay
                {
                    Timestamp = log.Timestamp,
                    StatusText = log.IsConnected ? lang.MainWindow.Connected : lang.MainWindow.Disconnected,
                    DownloadSpeedFormatted = FormatSpeed(log.DownloadSpeedBps),
                    UploadSpeedFormatted = FormatSpeed(log.UploadSpeedBps),
                    LatencyText = log.Latency < 0 ? "TIMEOUT" : $"{log.Latency}"
                }).ToList();

                HistoryDataGrid.ItemsSource = displayData;
                if (RecordCountText != null)
                    RecordCountText.Text = string.Format(lang.History.RecordsFormat, displayData.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private (DateTime from, DateTime to) GetDateRange()
        {
            var to = DateTime.Now;
            var from = to;

            var selectedIndex = DateRangeComboBox.SelectedIndex;
            switch (selectedIndex)
            {
                case 0: from = to.AddHours(-1); break;      // Last Hour
                case 1: from = to.AddDays(-1); break;       // Last 24 Hours
                case 2: from = to.AddDays(-7); break;       // Last 7 Days
                case 3: from = to.AddDays(-30); break;      // Last 30 Days
                case 4: from = DateTime.MinValue; break;    // All Time
            }

            return (from, to);
        }

        private string FormatSpeed(double bytesPerSecond)
        {
            if (bytesPerSecond < 1024) return $"{bytesPerSecond:F0} B/s";
            if (bytesPerSecond < 1024 * 1024) return $"{bytesPerSecond / 1024.0:F1} KB/s";
            return $"{bytesPerSecond / (1024.0 * 1024.0):F1} MB/s";
        }

        private void DateRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryDataGrid != null) // Check if initialized
            {
                LoadData();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void ClearOldLogs_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to delete logs older than 30 days?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _dataLogger.ClearOldLogs(30);
                LoadData();
                MessageBox.Show("Old logs cleared successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }

    // Display model for DataGrid
    public class NetworkLogDisplay
    {
        public DateTime Timestamp { get; set; }
        public string StatusText { get; set; } = "";
        public string DownloadSpeedFormatted { get; set; } = "";
        public string UploadSpeedFormatted { get; set; } = "";
        public string LatencyText { get; set; } = "";
    }
}
