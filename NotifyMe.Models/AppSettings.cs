namespace NotifyMe.Models;

public class AppSettings
{
    public int CheckIntervalSeconds { get; set; } = 5;
    public bool ShowNotifications { get; set; } = true;
    public bool PlayNotificationSound { get; set; } = true;
    public bool StartWithWindows { get; set; } = false;
    public string? PreferredNetworkInterface { get; set; }
    public int TrafficUpdateIntervalMs { get; set; } = 1000;
}
