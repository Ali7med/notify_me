namespace NotifyMe.Models;

public class UserSettings
{
    public double Opacity { get; set; } = 0.8;
    public string Theme { get; set; } = "Glass"; // "Glass" or "Classic"
    public bool IsTransparent { get; set; } = true;
    public string PingHost { get; set; } = "8.8.8.8";
    
    // Advanced Settings
    public int UpdateIntervalSeconds { get; set; } = 1;
    public double HighTrafficThresholdMBps { get; set; } = 5.0;
    public string HighTrafficThresholdUnit { get; set; } = "MB"; // KB, MB, GB
    public bool EnableSoundNotifications { get; set; } = true;
    public bool EnableToastNotifications { get; set; } = true;
    public bool StartWithWindows { get; set; } = false;
    
    // Notification Settings
    public string NotificationType { get; set; } = "Toast"; // "Toast" or "Custom"
    public string CustomNotificationPosition { get; set; } = "TopRight"; // TopRight, TopLeft, BottomRight, BottomLeft

    // Do Not Disturb Settings
    public bool EnableDND { get; set; } = false;
    public bool EnableDNDSchedule { get; set; } = false;
    public TimeSpan DNDStartTime { get; set; } = new TimeSpan(22, 0, 0); // 10:00 PM
    public TimeSpan DNDEndTime { get; set; } = new TimeSpan(7, 0, 0);   // 7:00 AM

    // Localization Settings
    public string Language { get; set; } = "ar"; // "ar" (Arabic) or "en" (English)
}
