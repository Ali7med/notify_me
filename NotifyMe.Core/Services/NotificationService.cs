using Microsoft.Toolkit.Uwp.Notifications;
using NotifyMe.Models;

namespace NotifyMe.Core.Services;

public class NotificationService
{
    private readonly AppSettings _settings;

    public NotificationService(AppSettings settings)
    {
        _settings = settings;
    }

    public void ShowConnectionLost()
    {
        if (!_settings.ShowNotifications) return;

        try
        {
            var builder = new ToastContentBuilder()
                .AddText("Internet Connection Lost")
                .AddText($"Connection lost at {DateTime.Now:HH:mm:ss}")
                .AddAttributionText("NotifyMe");
            
            builder.Show();
        }
        catch (Exception ex)
        {
            // Log error for debugging
            Console.WriteLine($"Failed to show notification: {ex.Message}");
        }
    }

    public void ShowConnectionRestored()
    {
        if (!_settings.ShowNotifications) return;

        try
        {
            var builder = new ToastContentBuilder()
                .AddText("Internet Connection Restored")
                .AddText($"Connection restored at {DateTime.Now:HH:mm:ss}")
                .AddAttributionText("NotifyMe");
            
            builder.Show();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to show notification: {ex.Message}");
        }
    }

    public void ShowCustomNotification(string title, string message)
    {
        if (!_settings.ShowNotifications) return;

        try
        {
            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .AddAttributionText("NotifyMe");
            
            builder.Show();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to show notification: {ex.Message}");
        }
    }
}
