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
        var settings = _settingsService.CurrentSettings;
        if (!settings.EnableToastNotifications) return;

        if (settings.NotificationType == "Custom")
        {
            ShowCustomNotification(Windows.NotificationType.Error, "Internet Connection Lost", 
                $"Connection lost at {DateTime.Now:HH:mm:ss}");
        }
        else
        {
            try
            {
                new ToastContentBuilder()
                    .AddText("Internet Connection Lost")
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
        var settings = _settingsService.CurrentSettings;
        if (!settings.EnableToastNotifications) return;

        if (settings.NotificationType == "Custom")
        {
            ShowCustomNotification(Windows.NotificationType.Success, "Internet Connection Restored", 
                $"Connection restored at {DateTime.Now:HH:mm:ss}");
        }
        else
        {
            try
            {
                new ToastContentBuilder()
                    .AddText("Internet Connection Restored")
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
            ShowCustomNotification(Windows.NotificationType.Warning, "High Traffic Alert", 
                $"Traffic exceeded {threshold:F1} {unit}/s\nCurrent speed: {formattedSpeed}");
        }
        else
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Displaying toast: {formattedSpeed}");
                
                new ToastContentBuilder()
                    .AddText("⚠️ High Traffic Alert")
                    .AddText($"Traffic exceeded {threshold:F1} {unit}/s")
                    .AddText($"Current speed: {formattedSpeed}")
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
}
