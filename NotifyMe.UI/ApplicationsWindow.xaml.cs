using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NotifyMe.Core.Services;
using NotifyMe.Models;

namespace NotifyMe.UI;

public partial class ApplicationsWindow : Window
{
    private readonly ProcessMonitorService _processMonitor;
    private readonly FirewallService _firewallService;
    private readonly ObservableCollection<ApplicationNetworkInfo> _applications;
    private List<ApplicationNetworkInfo> _allApplications = new();
    private string _currentSortColumn = "Usage";
    private ListSortDirection _currentSortDirection = ListSortDirection.Descending;
    private System.Windows.Threading.DispatcherTimer? _searchDebounceTimer;

    public ApplicationsWindow(ProcessMonitorService processMonitor)
    {
        InitializeComponent();
        
        _processMonitor = processMonitor;
        _firewallService = new FirewallService();
        _applications = new ObservableCollection<ApplicationNetworkInfo>();
        
        ApplicationsDataGrid.ItemsSource = _applications;
        
        _processMonitor.StatsUpdated += OnStatsUpdated;
        
        // Load initial data
        UpdateApplicationsList(_processMonitor.GetCurrentStats());
    }

    private void OnStatsUpdated(object? sender, Dictionary<int, ProcessNetworkStats> stats)
    {
        Dispatcher.Invoke(async () => await UpdateApplicationsListAsync(stats));
    }

    private async Task UpdateApplicationsListAsync(Dictionary<int, ProcessNetworkStats> stats)
    {
        var apps = new List<ApplicationNetworkInfo>();
        
        foreach (var s in stats.Values)
        {
            var app = new ApplicationNetworkInfo
            {
                ProcessId = s.ProcessId,
                ProcessName = s.ProcessName,
                ExecutablePath = s.ExecutablePath,
                ConnectionCount = s.ConnectionCount,
                BytesSent = s.BytesSent,
                BytesReceived = s.BytesReceived,
                LastActivity = s.LastActivity,
                Category = DetermineCategory(s.ProcessName),
                IconSource = await GetProcessIconAsync(s.ExecutablePath), // Async!
                IsBlocked = _firewallService.IsApplicationBlocked(s.ProcessName)
            };
            apps.Add(app);
        }

        _allApplications = apps;
        ApplyFiltersAndSort();
        UpdateStatistics();
    }

    // Keep sync version for initial load
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
            Category = DetermineCategory(s.ProcessName),
            IconSource = null, // Load icons later
            IsBlocked = _firewallService.IsApplicationBlocked(s.ProcessName)
        }).ToList();

        ApplyFiltersAndSort();
        UpdateStatistics();
        
        // Load icons async after initial display
        Task.Run(async () =>
        {
            foreach (var app in _allApplications)
            {
                if (!string.IsNullOrEmpty(app.ExecutablePath))
                {
                    var icon = await GetProcessIconAsync(app.ExecutablePath);
                    await Dispatcher.InvokeAsync(() => app.IconSource = icon);
                }
            }
        });
    }

    private void UpdateStatistics()
    {
        TotalAppsText.Text = _allApplications.Count.ToString();
        TotalUploadText.Text = NetworkStats.FormatBytes(_allApplications.Sum(a => a.BytesSent));
        TotalDownloadText.Text = NetworkStats.FormatBytes(_allApplications.Sum(a => a.BytesReceived));
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

        // Apply category filter
        var selectedCategory = (CategoryFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();
        if (!string.IsNullOrEmpty(selectedCategory) && selectedCategory != "All Categories")
        {
            filtered = filtered.Where(a => a.Category == selectedCategory);
        }

        // Apply sorting
        filtered = _currentSortColumn switch
        {
            "ProcessName" => _currentSortDirection == ListSortDirection.Ascending 
                ? filtered.OrderBy(a => a.ProcessName) 
                : filtered.OrderByDescending(a => a.ProcessName),
            "ConnectionCount" => _currentSortDirection == ListSortDirection.Ascending 
                ? filtered.OrderBy(a => a.ConnectionCount) 
                : filtered.OrderByDescending(a => a.ConnectionCount),
            "BytesSent" => _currentSortDirection == ListSortDirection.Ascending 
                ? filtered.OrderBy(a => a.BytesSent) 
                : filtered.OrderByDescending(a => a.BytesSent),
            "BytesReceived" => _currentSortDirection == ListSortDirection.Ascending 
                ? filtered.OrderBy(a => a.BytesReceived) 
                : filtered.OrderByDescending(a => a.BytesReceived),
            "Category" => _currentSortDirection == ListSortDirection.Ascending 
                ? filtered.OrderBy(a => a.Category) 
                : filtered.OrderByDescending(a => a.Category),
            "UserRating" => _currentSortDirection == ListSortDirection.Ascending 
                ? filtered.OrderBy(a => a.UserRating) 
                : filtered.OrderByDescending(a => a.UserRating),
            "LastActivity" => _currentSortDirection == ListSortDirection.Ascending 
                ? filtered.OrderBy(a => a.LastActivity) 
                : filtered.OrderByDescending(a => a.LastActivity),
            _ => filtered.OrderByDescending(a => a.BytesSent + a.BytesReceived)
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

    private ImageSource? GetProcessIcon(string executablePath)
    {
        try
        {
            if (string.IsNullOrEmpty(executablePath) || !File.Exists(executablePath))
                return null;

            using var icon = System.Drawing.Icon.ExtractAssociatedIcon(executablePath);
            if (icon == null) return null;

            using var bitmap = icon.ToBitmap();
            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            memoryStream.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
        catch
        {
            return null;
        }
    }

    private async Task<ImageSource?> GetProcessIconAsync(string executablePath)
    {
        return await Task.Run(() => GetProcessIcon(executablePath));
    }

    // Event Handlers
    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Debounce search to avoid filtering on every keystroke
        _searchDebounceTimer?.Stop();
        _searchDebounceTimer = new System.Windows.Threading.DispatcherTimer 
        { 
            Interval = TimeSpan.FromMilliseconds(300) 
        };
        _searchDebounceTimer.Tick += (s, args) =>
        {
            _searchDebounceTimer.Stop();
            ApplyFiltersAndSort();
        };
        _searchDebounceTimer.Start();
    }

    private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_allApplications.Any())
        {
            ApplyFiltersAndSort();
        }
    }

    private void ApplicationsDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;
        
        var column = e.Column;
        var sortPropertyName = column.SortMemberPath;

        if (string.IsNullOrEmpty(sortPropertyName))
            return;

        // Toggle sort direction
        if (_currentSortColumn == sortPropertyName)
        {
            _currentSortDirection = _currentSortDirection == ListSortDirection.Ascending 
                ? ListSortDirection.Descending 
                : ListSortDirection.Ascending;
        }
        else
        {
            _currentSortColumn = sortPropertyName;
            _currentSortDirection = ListSortDirection.Descending;
        }

        // Update column header
        column.SortDirection = _currentSortDirection;

        // Clear other column sort indicators
        foreach (var col in ApplicationsDataGrid.Columns)
        {
            if (col != column)
                col.SortDirection = null;
        }

        ApplyFiltersAndSort();
    }

    private void Star_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not ApplicationNetworkInfo app)
            return;

        var starNumber = int.Parse(element.Tag.ToString() ?? "0");
        app.UserRating = starNumber;
    }

    private void ToggleBlock_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not ApplicationNetworkInfo app)
            return;

        try
        {
            if (app.IsBlocked)
            {
                // Unblock
                var result = _firewallService.UnblockApplication(app.ProcessName);
                if (result)
                {
                    app.IsBlocked = false;
                    MessageBox.Show($"{app.ProcessName} has been unblocked.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Failed to unblock {app.ProcessName}. Make sure you run as Administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Block
                var confirmResult = MessageBox.Show(
                    $"Are you sure you want to block {app.ProcessName} from accessing the internet?\n\nThis will create firewall rules to prevent all network access.",
                    "Confirm Block",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    var result = _firewallService.BlockApplication(app.ExecutablePath, app.ProcessName);
                    if (result)
                    {
                        app.IsBlocked = true;
                        MessageBox.Show($"{app.ProcessName} has been blocked.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to block {app.ProcessName}. Make sure you run as Administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Window Controls
    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        else
        {
            this.DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _processMonitor.StatsUpdated -= OnStatsUpdated;
        base.OnClosed(e);
    }
}

public class ApplicationNetworkInfo : INotifyPropertyChanged
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public int ConnectionCount { get; set; }
    public long BytesSent { get; set; }
    public long BytesReceived { get; set; }
    public DateTime LastActivity { get; set; }
    public string Category { get; set; } = "Other";
    public ImageSource? IconSource { get; set; }

    private bool _isBlocked;
    public bool IsBlocked
    {
        get => _isBlocked;
        set
        {
            _isBlocked = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BlockStatusText));
            OnPropertyChanged(nameof(BlockStatusColor));
            OnPropertyChanged(nameof(BlockButtonText));
        }
    }

    public string BlockStatusText => IsBlocked ? "Blocked" : "Allowed";
    public string BlockStatusColor => IsBlocked ? "#FF5252" : "#4CAF50";
    public string BlockButtonText => IsBlocked ? "Unblock" : "Block";

    private int _userRating;
    public int UserRating
    {
        get => _userRating;
        set
        {
            _userRating = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Star1Color));
            OnPropertyChanged(nameof(Star2Color));
            OnPropertyChanged(nameof(Star3Color));
            OnPropertyChanged(nameof(Star4Color));
            OnPropertyChanged(nameof(Star5Color));
        }
    }

    // Star colors based on rating
    public string Star1Color => UserRating >= 1 ? "#FFC107" : "#666666";
    public string Star2Color => UserRating >= 2 ? "#FFC107" : "#666666";
    public string Star3Color => UserRating >= 3 ? "#FFC107" : "#666666";
    public string Star4Color => UserRating >= 4 ? "#FFC107" : "#666666";
    public string Star5Color => UserRating >= 5 ? "#FFC107" : "#666666";

    public string BytesSentFormatted => NetworkStats.FormatBytes(BytesSent);
    public string BytesReceivedFormatted => NetworkStats.FormatBytes(BytesReceived);
    public string LastActivityFormatted => 
        (DateTime.Now - LastActivity).TotalSeconds < 60 
            ? "Just now" 
            : $"{(int)(DateTime.Now - LastActivity).TotalMinutes}m ago";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
