namespace NotifyMe.UI.Models;

public class LanguageData
{
    public string LanguageName { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public bool IsRTL { get; set; } = false;
    
    public SettingsStrings Settings { get; set; } = new();
    public ContextMenuStrings ContextMenu { get; set; } = new();
    public FloatingWidgetStrings FloatingWidget { get; set; } = new();
    public MainWindowStrings MainWindow { get; set; } = new();
    public NotificationStrings Notifications { get; set; } = new();
    public NotificationPositionStrings NotificationPositionItems { get; set; } = new();
    public HistoryStrings History { get; set; } = new();
}

public class SettingsStrings
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string SaveChanges { get; set; } = string.Empty;
    
    // Tabs
    public string TabAppearance { get; set; } = string.Empty;
    public string TabNetwork { get; set; } = string.Empty;
    public string TabNotifications { get; set; } = string.Empty;
    public string TabAdvanced { get; set; } = string.Empty;
    
    // Appearance
    public string Transparency { get; set; } = string.Empty;
    public string TransparencyDesc { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string ThemeDesc { get; set; } = string.Empty;
    public string GlassTheme { get; set; } = string.Empty;
    public string ClassicTheme { get; set; } = string.Empty;
    
    // Network
    public string PingHost { get; set; } = string.Empty;
    public string PingHostDesc { get; set; } = string.Empty;
    public string QuickSelect { get; set; } = string.Empty;
    
    // Notifications
    public string AlertPreferences { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string WindowsToast { get; set; } = string.Empty;
    public string WindowsToastDesc { get; set; } = string.Empty;
    public string CustomNotifications { get; set; } = string.Empty;
    public string CustomNotificationsDesc { get; set; } = string.Empty;
    public string NotificationPosition { get; set; } = string.Empty;
    public string ShowToast { get; set; } = string.Empty;
    public string ShowToastDesc { get; set; } = string.Empty;
    public string PlaySound { get; set; } = string.Empty;
    public string PlaySoundDesc { get; set; } = string.Empty;
    public string DoNotDisturb { get; set; } = string.Empty;
    public string DNDNow { get; set; } = string.Empty;
    public string DNDNowDesc { get; set; } = string.Empty;
    public string DNDSchedule { get; set; } = string.Empty;
    public string DNDScheduleDesc { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Tip { get; set; } = string.Empty;
    public string TipText { get; set; } = string.Empty;
    
    // Advanced
    public string UpdateFrequency { get; set; } = string.Empty;
    public string UpdateFrequencyDesc { get; set; } = string.Empty;
    public string TrafficThreshold { get; set; } = string.Empty;
    public string TrafficThresholdDesc { get; set; } = string.Empty;
    public string StartupBehavior { get; set; } = string.Empty;
    public string StartWithWindows { get; set; } = string.Empty;
    public string StartWithWindowsDesc { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string LanguageDesc { get; set; } = string.Empty;
    public string InterfaceLanguage { get; set; } = string.Empty;
    public string SelectLanguage { get; set; } = string.Empty;
}

public class ContextMenuStrings
{
    public string ShowStatistics { get; set; } = string.Empty;
    public string ShowWidget { get; set; } = string.Empty;
    public string ViewHistory { get; set; } = string.Empty;
    public string Settings { get; set; } = string.Empty;
    public string ConnectionStatus { get; set; } = string.Empty;
    public string TrafficMonitor { get; set; } = string.Empty;
    public string Exit { get; set; } = string.Empty;
}

public class FloatingWidgetStrings
{
    public string OpenNotifyMe { get; set; } = string.Empty;
    public string Settings { get; set; } = string.Empty;
    public string HideWidget { get; set; } = string.Empty;
    public string ExitApplication { get; set; } = string.Empty;
}

public class MainWindowStrings
{
    public string Title { get; set; } = string.Empty;
    public string HeaderTitle { get; set; } = string.Empty;
    public string Statistics { get; set; } = string.Empty;
    public string ConnectionStatus { get; set; } = string.Empty;
    public string Connected { get; set; } = string.Empty;
    public string Disconnected { get; set; } = string.Empty;
    public string DownloadSpeed { get; set; } = string.Empty;
    public string UploadSpeed { get; set; } = string.Empty;
    public string TotalPrefix { get; set; } = string.Empty;
    public string LastUpdatePrefix { get; set; } = string.Empty;
    public string MinimizeButton { get; set; } = string.Empty;
    public string Latency { get; set; } = string.Empty;
    public string BottomRight { get; set; } = string.Empty;
    public string BottomLeft { get; set; } = string.Empty;
}

public class NotificationStrings
{
    public string ConnectionLost { get; set; } = string.Empty;
    public string ConnectionRestored { get; set; } = string.Empty;
    public string HighTraffic { get; set; } = string.Empty;
    public string TrafficExceeded { get; set; } = string.Empty;
    public string CurrentSpeed { get; set; } = string.Empty;
    public string TipTitle { get; set; } = string.Empty;
    public string TipBody { get; set; } = string.Empty;
}



public class NotificationPositionStrings
{
    public string TopRight { get; set; } = string.Empty;
    public string TopLeft { get; set; } = string.Empty;
    public string BottomRight { get; set; } = string.Empty;
    public string BottomLeft { get; set; } = string.Empty;
}

public class HistoryStrings
{
    public string Title { get; set; } = string.Empty;
    public string HeaderTitle { get; set; } = string.Empty;
    public string DateRange { get; set; } = string.Empty;
    public string Refresh { get; set; } = string.Empty;
    public string RecordsFormat { get; set; } = string.Empty;
    public string ClearLogs { get; set; } = string.Empty;
    public string ColTime { get; set; } = string.Empty;
    public string ColDate { get; set; } = string.Empty;
    public string ColStatus { get; set; } = string.Empty;
    public string ColDownload { get; set; } = string.Empty;
    public string ColUpload { get; set; } = string.Empty;
    public string ColPing { get; set; } = string.Empty;
    public string RangeLastHour { get; set; } = string.Empty;
    public string RangeLast24Hours { get; set; } = string.Empty;
    public string RangeLast7Days { get; set; } = string.Empty;
    public string RangeLast30Days { get; set; } = string.Empty;
    public string RangeAllTime { get; set; } = string.Empty;
}
