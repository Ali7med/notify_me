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
        if (!_settingsService.CurrentSettings.EnableToastNotifications) return;

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

    public void ShowConnectionRestored()
    {
        if (!_settingsService.CurrentSettings.EnableToastNotifications) return;

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

    public void ShowCustomNotification(string title, string message)
    {
        if (!_settingsService.CurrentSettings.EnableToastNotifications) return;

        try
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .AddAttributionText("NotifyMe")
                .Show();
        }
        catch
        {
            // Silently fail if notifications aren't supported
        }
    }
}
