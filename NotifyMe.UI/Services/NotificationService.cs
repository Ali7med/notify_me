using Microsoft.Toolkit.Uwp.Notifications;
using NotifyMe.Core.Services;
using System;

namespace NotifyMe.UI.Services;

public class NotificationService
{
    private readonly SettingsService _settingsService;

    public NotificationService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public void ShowConnectionLost()
    {
        if (IsDNDActive()) return;

        var settings = _settingsService.CurrentSettings;
        if (!settings.EnableToastNotifications) return;
        
        var lang = Helpers.LocalizationManager.Current;

        if (settings.NotificationType == "Custom")
        {
            ShowCustomNotification(Windows.NotificationType.Error, lang.Notifications.ConnectionLost, 
                $"Connection lost at {DateTime.Now:HH:mm:ss}");
        }
        else
        {
            try
            {
                new ToastContentBuilder()
                    .AddText(lang.Notifications.ConnectionLost)
                    .AddText($"Connection lost at {DateTime.Now:HH:mm:ss}")
                    .AddAttributionText("NotifyMe")
                    .Show();
            }
            catch
            {
                // Silently fail if notifications aren't supported
            }
        }
    }

    public void ShowConnectionRestored()
    {
        if (IsDNDActive()) return;

        var settings = _settingsService.CurrentSettings;
        if (!settings.EnableToastNotifications) return;
        
        var lang = Helpers.LocalizationManager.Current;

        if (settings.NotificationType == "Custom")
        {
            ShowCustomNotification(Windows.NotificationType.Success, lang.Notifications.ConnectionRestored, 
                $"Connection restored at {DateTime.Now:HH:mm:ss}");
        }
        else
        {
            try
            {
                new ToastContentBuilder()
                    .AddText(lang.Notifications.ConnectionRestored)
                    .AddText($"Connection restored at {DateTime.Now:HH:mm:ss}")
                    .AddAttributionText("NotifyMe")
                    .Show();
            }
            catch
            {
                // Silently fail if notifications aren't supported
            }
        }
    }

    public void ShowCustomNotification(string title, string message)
    {
        if (IsDNDActive()) return;
        // Overload for generic usage, defaulting to info
        ShowCustomNotification(Windows.NotificationType.Info, title, message);
    }

    private void ShowCustomNotification(Windows.NotificationType type, string title, string message)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var position = _settingsService.CurrentSettings.CustomNotificationPosition ?? "TopRight";
            var window = new Windows.CustomNotificationWindow(type, title, message, position);
            window.Show();
        });
    }

    public void ShowHighTrafficAlert(double speedMBps, string unit, double threshold)
    {
        if (IsDNDActive()) return;

        System.Diagnostics.Debug.WriteLine($"ShowHighTrafficAlert called: Speed={speedMBps:F2} MB/s, Unit={unit}, Threshold={threshold}");
        
        var settings = _settingsService.CurrentSettings;
        if (!settings.EnableToastNotifications)
        {
            System.Diagnostics.Debug.WriteLine("Toast notifications are disabled");
            return;
        }

        string formattedSpeed;
        switch (unit.ToUpper())
        {
            case "KB":
                formattedSpeed = $"{speedMBps * 1024:F2} KB/s";
                break;
            case "GB":
                formattedSpeed = $"{speedMBps / 1024:F2} GB/s";
                break;
            default:
                formattedSpeed = $"{speedMBps:F2} MB/s";
                break;
        }

        if (settings.NotificationType == "Custom")
        {
            var lang = Helpers.LocalizationManager.Current;
            ShowCustomNotification(Windows.NotificationType.Warning, lang.Notifications.HighTraffic, 
                $"{string.Format(lang.Notifications.TrafficExceeded, threshold, unit)}\\n{string.Format(lang.Notifications.CurrentSpeed, formattedSpeed)}");
        }
        else
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Displaying toast: {formattedSpeed}");
                
                var lang = Helpers.LocalizationManager.Current;
                new ToastContentBuilder()
                    .AddText($"⚠️ {lang.Notifications.HighTraffic}")
                    .AddText(string.Format(lang.Notifications.TrafficExceeded, threshold, unit))
                    .AddText(string.Format(lang.Notifications.CurrentSpeed, formattedSpeed))
                    .AddAttributionText("NotifyMe")
                    .Show();
                    
                System.Diagnostics.Debug.WriteLine("Toast shown successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to show toast: {ex.Message}");
            }
        }
    }

    public bool IsDNDActive()
    {
        var settings = _settingsService.CurrentSettings;
        
        // Manual DND
        if (settings.EnableDND) return true;

        // Scheduled DND
        if (settings.EnableDNDSchedule)
        {
            var now = DateTime.Now.TimeOfDay;
            var start = settings.DNDStartTime;
            var end = settings.DNDEndTime;

            // Check if schedule crosses midnight (e.g., 22:00 to 07:00)
            if (start > end)
            {
                return now >= start || now <= end;
            }
            else
            {
                return now >= start && now <= end;
            }
        }

        return false;
    }
}
