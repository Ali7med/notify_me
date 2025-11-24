using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace NotifyMe.Core.Services;

public class AutoStartService
{
    private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "NotifyMe";
    
    /// <summary>
    /// Check if auto-start is currently enabled in Windows Registry
    /// </summary>
    public bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
            var value = key?.GetValue(AppName) as string;
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to check auto-start status: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Enable auto-start by adding registry key
    /// </summary>
    public bool Enable()
    {
        try
        {
            var exePath = GetExecutablePath();
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
            if (key != null)
            {
                key.SetValue(AppName, $"\"{exePath}\"");
                Debug.WriteLine($"Auto-start enabled: {exePath}");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to enable auto-start: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Disable auto-start by removing registry key
    /// </summary>
    public bool Disable()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
            if (key != null)
            {
                key.DeleteValue(AppName, false);
                Debug.WriteLine("Auto-start disabled");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to disable auto-start: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Get the full path to the executable
    /// </summary>
    private string GetExecutablePath()
    {
        var processModule = Process.GetCurrentProcess().MainModule;
        return processModule?.FileName ?? AppDomain.CurrentDomain.BaseDirectory;
    }
}
