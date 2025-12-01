using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using NotifyMe.Core.Services;
using NotifyMe.Models;

namespace NotifyMe.UI;

public partial class ApplicationsWindow : Window
{
    private readonly ProcessMonitorService _processMonitor;
    private readonly ObservableCollection<ApplicationNetworkInfo> _applications;
    private List<ApplicationNetworkInfo> _allApplications = new();

    public ApplicationsWindow(ProcessMonitorService processMonitor)
    {
        InitializeComponent();
        
        _processMonitor = processMonitor;
        _applications = new ObservableCollection<ApplicationNetworkInfo>();
        
        ApplicationsDataGrid.ItemsSource = _applications;
        
        _processMonitor.StatsUpdated += OnStatsUpdated;
        
        // Load initial data
        UpdateApplicationsList(_processMonitor.GetCurrentStats());
    }

    private void OnStatsUpdated(object? sender, Dictionary<int, ProcessNetworkStats> stats)
    {
        Dispatcher.Invoke(() => UpdateApplicationsList(stats));
    }

    private void UpdateApplicationsList(Dictionary<int, ProcessNetworkStats> stats)
    {
        _allApplications = stats.Values.Select(s => new ApplicationNetworkInfo
        {
            ProcessId = s.ProcessId,
            ProcessName = s.ProcessName,
            ExecutablePath = s.ExecutablePath,
            ConnectionCount = s.ConnectionCount,
            BytesSent = s.BytesSent,
            BytesReceived = s.BytesReceived,
            LastActivity = s.LastActivity,
            Category = DetermineCategory(s.ProcessName)
        }).ToList();

        ApplyFiltersAndSort();
    }

    private void ApplyFiltersAndSort()
    {
        var filtered = _allApplications.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchBox.Text))
        {
            var searchTerm = SearchBox.Text.ToLower();
            filtered = filtered.Where(a => a.ProcessName.ToLower().Contains(searchTerm));
        }

        // Apply sorting
        var sortTag = (SortComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Usage";
        filtered = sortTag switch
        {
            "Name" => filtered.OrderBy(a => a.ProcessName),
            "Usage" => filtered.OrderByDescending(a => a.BytesSent + a.BytesReceived),
            "Connections" => filtered.OrderByDescending(a => a.ConnectionCount),
            "Activity" => filtered.OrderByDescending(a => a.LastActivity),
            _ => filtered
        };

        _applications.Clear();
        foreach (var app in filtered)
        {
            _applications.Add(app);
        }
    }

    private string DetermineCategory(string processName)
    {
        var lower = processName.ToLower();
        
        if (lower.Contains("chrome") || lower.Contains("firefox") || lower.Contains("edge") || lower.Contains("browser"))
            return "Browser";
        if (lower.Contains("steam") || lower.Contains("game"))
            return "Gaming";
        if (lower.Contains("discord") || lower.Contains("teams") || lower.Contains("zoom") || lower.Contains("skype"))
            return "Communication";
        if (lower.Contains("system") || lower.Contains("windows") || lower.Contains("svchost"))
            return "System";
        if (lower.Contains("torrent") || lower.Contains("download"))
            return "Download";
        
        return "Other";
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFiltersAndSort();
    }

    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_allApplications.Any())
        {
            ApplyFiltersAndSort();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _processMonitor.StatsUpdated -= OnStatsUpdated;
        base.OnClosed(e);
    }
}

public class ApplicationNetworkInfo
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public int ConnectionCount { get; set; }
    public long BytesSent { get; set; }
    public long BytesReceived { get; set; }
    public DateTime LastActivity { get; set; }
    public string Category { get; set; } = "Other";

    public string BytesSentFormatted => NetworkStats.FormatBytes(BytesSent);
    public string BytesReceivedFormatted => NetworkStats.FormatBytes(BytesReceived);
    public string LastActivityFormatted => 
        (DateTime.Now - LastActivity).TotalSeconds < 60 
            ? "Just now" 
            : $"{(int)(DateTime.Now - LastActivity).TotalMinutes}m ago";
}
